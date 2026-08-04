using HrmApi.Application;
using HrmApi.Infrastructure;
using HrmApi.Infrastructure.Persistence;
using HrmApi.Domain.Entities.Permission;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HrmApi.Application.Common.Interfaces.ICurrentUserService, HrmApi.WebApi.Services.CurrentUserService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        await DatabaseBootstrap.EnsureUtf8DatabaseAsync(app.Configuration, logger);

        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        var seedUsername = Environment.GetEnvironmentVariable("SEED_ADMIN_USERNAME") ?? "admin";
        var seedPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? "admin123";
        var seedEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL") ?? "admin@hrm.com";
        var hasher = services.GetRequiredService<IPasswordHasher<UserEntity>>();

        var adminUser = context.UserEntities.FirstOrDefault(u => u.Username.ToLower() == seedUsername.ToLower());
        if (adminUser == null)
        {
            adminUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = seedUsername,
                Type = "ADMIN",
                Email = seedEmail,
                IsActive = true,
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, seedPassword);

            context.UserEntities.Add(adminUser);
            context.SaveChanges();
        }
        else
        {
            var verifyResult = hasher.VerifyHashedPassword(adminUser, adminUser.PasswordHash, seedPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                adminUser.PasswordHash = hasher.HashPassword(adminUser, seedPassword);
                adminUser.Email = seedEmail;
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database bootstrap, migration, or seeding failed.");
        throw;
    }
}

if (app.Environment.IsDevelopment() || true) 
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HRM API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();