using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HrmApi.WebApi.Authorization
{
    /// <summary>
    /// AuthZ: Admin short-circuit · claim permission (legacy JWT) · fallback IPermissionCache / DB.
    /// </summary>
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            string? userType = context.User.FindFirstValue(ClaimTypesEx.UserType)
                ?? context.User.FindFirstValue("role")
                ?? context.User.FindFirstValue(ClaimTypes.Role);

            if (string.Equals(userType, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(userType, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(userType, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase)
                || context.User.IsInRole("SuperAdmin")
                || context.User.IsInRole("Admin")
                || context.User.IsInRole(RoleCodes.Admin)
                || context.User.HasClaim(ClaimTypes.Role, "SuperAdmin")
                || context.User.HasClaim(ClaimTypes.Role, "Admin")
                || context.User.HasClaim("role", "SuperAdmin")
                || context.User.HasClaim("role", "Admin")
                || context.User.HasClaim(ClaimTypes.Role, RoleCodes.Admin))
            {
                context.Succeed(requirement);
                return;
            }

            // Legacy fat JWT vẫn hoạt động trong thời gian chuyển tiếp
            if (context.User.HasClaim(ClaimTypesEx.Permission, requirement.Permission))
            {
                context.Succeed(requirement);
                return;
            }

            string? userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return;
            }

            using IServiceScope scope = _scopeFactory.CreateScope();
            IPermissionCache cache = scope.ServiceProvider.GetRequiredService<IPermissionCache>();
            IAuthContextService authContextService = scope.ServiceProvider.GetRequiredService<IAuthContextService>();

            AuthContextDto auth;
            if (cache.TryGet(userId, out AuthContextDto? cached) && cached != null)
            {
                auth = cached;
            }
            else
            {
                auth = await authContextService.LoadAuthContextAsync(userId);
            }

            if (auth.Permissions.Any(p =>
                    string.Equals(p, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
        }
    }
}
