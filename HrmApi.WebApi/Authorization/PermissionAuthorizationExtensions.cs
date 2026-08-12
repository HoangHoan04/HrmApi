using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HrmApi.WebApi.Authorization
{
    public static class PermissionAuthorizationExtensions
    {
        public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            return services;
        }
    }
}
