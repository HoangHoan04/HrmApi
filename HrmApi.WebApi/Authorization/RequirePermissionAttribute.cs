using System;
using Microsoft.AspNetCore.Authorization;

namespace HrmApi.WebApi.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Policy = $"{PermissionPolicyProvider.PolicyPrefix}{permission}";
        }
    }
}
