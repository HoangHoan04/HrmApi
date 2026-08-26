using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;

namespace HrmApi.WebApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                string? idStr = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User?.FindFirstValue("sub");
                return Guid.TryParse(idStr, out Guid id) ? id : null;
            }
        }

        public Guid? EmployeeId
        {
            get
            {
                string? idStr = User?.FindFirstValue("EmployeeId") ?? User?.FindFirstValue("employee_id");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public Guid? CompanyId
        {
            get
            {
                string? idStr = User?.FindFirstValue("company_id") ?? User?.FindFirstValue("CompanyId");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public Guid? BranchId
        {
            get
            {
                string? idStr = User?.FindFirstValue("BranchId") ?? User?.FindFirstValue("branch_id");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public string? UserCode => User?.FindFirstValue("UserCode") ?? Username;

        public string? Username => User?.FindFirstValue(ClaimTypes.Name)
            ?? User?.FindFirstValue("name")
            ?? User?.FindFirstValue(ClaimTypes.Email)
            ?? User?.FindFirstValue("email");

        public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent => _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

        public IReadOnlyList<string> Roles =>
            User?.FindAll(ClaimTypes.Role)
                .Concat(User?.FindAll("role") ?? Enumerable.Empty<Claim>())
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
            ?? (IReadOnlyList<string>)Array.Empty<string>();

        public IReadOnlyList<string> Permissions =>
            User?.FindAll(ClaimTypesEx.Permission)
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
            ?? (IReadOnlyList<string>)Array.Empty<string>();

        public bool IsAdmin
        {
            get
            {
                string? userType = User?.FindFirstValue(ClaimTypesEx.UserType)
                    ?? User?.FindFirstValue("role")
                    ?? User?.FindFirstValue(ClaimTypes.Role);
                if (string.Equals(userType, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(userType, "Admin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(userType, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase))
                    return true;
                return Roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(r, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase));
            }
        }

        public bool HasPermission(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;
            if (IsAdmin)
                return true;
            return Permissions.Any(p => string.Equals(p, code, StringComparison.OrdinalIgnoreCase));
        }
    }
}
