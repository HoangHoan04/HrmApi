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
                string? idStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(idStr, out Guid id) ? id : null;
            }
        }

        public Guid? EmployeeId
        {
            get
            {
                string? idStr = User?.FindFirstValue("EmployeeId");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public Guid? CompanyId
        {
            get
            {
                string? idStr = User?.FindFirstValue("CompanyId");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public Guid? BranchId
        {
            get
            {
                string? idStr = User?.FindFirstValue("BranchId");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public string? UserCode => User?.FindFirstValue("UserCode") ?? Username;

        public string? Username => User?.FindFirstValue(ClaimTypes.Name);

        public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent => _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

        public IReadOnlyList<string> Roles =>
            User?.FindAll(ClaimTypes.Role)
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
                string? userType = User?.FindFirstValue(ClaimTypesEx.UserType);
                if (string.Equals(userType, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase))
                    return true;
                return Roles.Any(r => string.Equals(r, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase));
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
