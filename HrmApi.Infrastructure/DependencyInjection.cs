using System.Text;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Infrastructure.Persistence;
using HrmApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HrmApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var npgsqlBuilder = NpgsqlConnectionFactory.Build(configuration.GetConnectionString("DefaultConnection"));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(npgsqlBuilder.ConnectionString, builder =>
                {
                    builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    builder.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<HrmApi.Domain.Entities.Permission.UserEntity>, Microsoft.AspNetCore.Identity.PasswordHasher<HrmApi.Domain.Entities.Permission.UserEntity>>();
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddHttpClient(nameof(UploadFileService));
            services.AddScoped<IUploadFileService, UploadFileService>();
            // Tài khoản đăng nhập được tạo trực tiếp trong DB HRM (không còn gọi sang AuthApi/cổng SSO).
            services.AddScoped<IAuthProvisioningService, LocalAuthProvisioningService>();

            // JWT do chính HrmApi phát hành (AuthController) — ký HMAC bằng JwtSettings:Secret.
            var jwtSecret = configuration["JwtSettings:Secret"] ?? JwtDefaults.Secret;
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var issuer = configuration["JwtSettings:Issuer"] ?? JwtDefaults.Issuer;
            var audience = configuration["JwtSettings:Audience"] ?? JwtDefaults.Audience;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmetricKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }

    /// <summary>
    /// Giá trị mặc định cho JWT khi appsettings không cấu hình (dùng chung cho phát hành và xác thực token).
    /// </summary>
    public static class JwtDefaults
    {
        public const string Secret = "SuperSecretKeyForHrmSystem2026!AwesomeDesignPleaseChangeMeInProduction";
        public const string Issuer = "HrmApi";
        public const string Audience = "HrmAdmin";
    }
}
