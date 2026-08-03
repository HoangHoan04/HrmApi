using System.Reflection;
using HrmApi.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HrmApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            services.AddScoped<IActionLogService, HrmApi.Application.Services.ActionLogService>();

            return services;
        }
    }
}
