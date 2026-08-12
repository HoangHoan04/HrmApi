using System.Security.Claims;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;

namespace HrmApi.WebApi.Authorization
{
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            string? userType = context.User.FindFirstValue(ClaimTypesEx.UserType)
                ?? context.User.FindFirstValue(ClaimTypes.Role);

            if (string.Equals(userType, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.User.IsInRole(RoleCodes.Admin)
                || context.User.HasClaim(ClaimTypes.Role, RoleCodes.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.User.HasClaim(ClaimTypesEx.Permission, requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
