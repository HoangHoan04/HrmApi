using HrmApi.Application.Common.Interfaces;
using HrmApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HrmApi.Domain.Entities.Permission;
using Microsoft.AspNetCore.Identity;

using HrmApi.Infrastructure.Services;

namespace HrmApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var npgsqlBuilder = NpgsqlConnectionFactory.Build(configuration.GetConnectionString("DefaultConnection"));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(npgsqlBuilder.ConnectionString, builder =>
                {
                    builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    builder.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
            services.AddScoped<IEmailService, EmailService>();

            var jwtSecret = configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForHrmSystem2026!AwesomeDesignPleaseChangeMeInProduction";
            var key = Encoding.ASCII.GetBytes(jwtSecret);

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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"] ?? "HrmApi",
                    ValidateAudience = true,
                    ValidAudience = configuration["JwtSettings:Audience"] ?? "HrmAdmin",
                    ValidateLifetime = true,
                    ClockSkew = System.TimeSpan.Zero
                };
            });

            return services;
        }
    }
}
