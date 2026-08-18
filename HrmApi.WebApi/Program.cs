using System.Text.Json.Serialization;
using HrmApi.Application;
using HrmApi.Infrastructure;
using HrmApi.Infrastructure.Persistence;
using HrmApi.WebApi.Authorization;
using HrmApi.WebApi.Background;
using HrmApi.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;

string envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (string line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
        {
            continue;
        }

        string[] parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPermissionAuthorization();
builder.Services.AddHostedService<HrmPeriodicJobsService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HrmApi.Application.Common.Interfaces.ICurrentUserService, HrmApi.WebApi.Services.CurrentUserService>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();
builder.Services.AddSingleton<HrmApi.Application.Common.Interfaces.INotificationRealtimeService, HrmApi.WebApi.Services.NotificationRealtimeService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        _ = policy.WithOrigins("http://localhost:4200", "http://localhost:4201", "http://localhost:8081", "http://localhost:19006")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        await DatabaseBootstrap.EnsureUtf8DatabaseAsync(app.Configuration, logger);

        ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database bootstrap or migration failed.");
        throw;
    }
}

if (app.Environment.IsDevelopment() || true)
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HRM API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAngularDev");

app.UseMiddleware<IpAllowlistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<HrmApi.WebApi.Hubs.NotificationHub>("/hubs/notifications");
app.Run();
