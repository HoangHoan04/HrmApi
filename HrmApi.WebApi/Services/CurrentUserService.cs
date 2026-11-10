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

        /// <summary>
        /// Tìm claim KHÔNG phân biệt hoa thường. Từ Microsoft.IdentityModel 8.x, JwtBearer tạo
        /// CaseSensitiveClaimsIdentity nên FindFirstValue("EmployeeId") không khớp claim "employeeId"
        /// mà AuthController phát hành -> EmployeeId/CompanyId/BranchId luôn null.
        /// </summary>
        private string? FindClaimValue(params string[] claimTypes)
        {
            if (User == null) return null;
            foreach (string type in claimTypes)
            {
                Claim? claim = User.Claims.FirstOrDefault(c =>
                    string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(c.Value));
                if (claim != null) return claim.Value;
            }
            return null;
        }

        private Guid? FindGuidClaim(params string[] claimTypes)
        {
            string? idStr = FindClaimValue(claimTypes);
            return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
        }

        public Guid? UserId
        {
            get
            {
                string? idStr = FindClaimValue(ClaimTypes.NameIdentifier, "sub");
                return Guid.TryParse(idStr, out Guid id) ? id : null;
            }
        }

        public Guid? EmployeeId => FindGuidClaim("employeeId", "employee_id");

        public Guid? CompanyId => FindGuidClaim("companyId", "company_id");

        public Guid? BranchId => FindGuidClaim("branchId", "branch_id");

        public string? UserCode => FindClaimValue("UserCode") ?? Username;

        public string? Username => FindClaimValue(ClaimTypes.Name, "name", "username", ClaimTypes.Email, "email");

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
                string? userType = FindClaimValue(ClaimTypesEx.UserType, "type", "role", ClaimTypes.Role);
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
