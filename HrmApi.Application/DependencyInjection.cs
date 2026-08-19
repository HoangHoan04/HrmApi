using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HrmApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            _ = services.AddMediatR(cfg =>
            {
                _ = cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            _ = services.AddMemoryCache();
            _ = services.AddSingleton<IPermissionCache, MemoryPermissionCache>();
            _ = services.AddSingleton<IIpAllowlistCache, MemoryIpAllowlistCache>();

            _ = services.AddScoped<IActionLogService, HrmApi.Application.Services.ActionLogService>();
            _ = services.AddScoped<IAttendanceRuleService, AttendanceRuleService>();
            _ = services.AddScoped<IAuthContextService, AuthContextService>();
            _ = services.AddScoped<IDataScopeService, DataScopeService>();
            _ = services.AddScoped<IWorkflowEngine, WorkflowEngine>();
            _ = services.AddHttpClient(nameof(WebhookDeliveryService));
            _ = services.AddScoped<IWebhookDeliveryService, WebhookDeliveryService>();

            return services;
        }
    }
}
