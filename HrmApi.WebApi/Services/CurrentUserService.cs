using HrmApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace HrmApi.WebApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(idStr, out var id))
                {
                    return id;
                }
                return null;
            }
        }

        public Guid? EmployeeId
        {
            get
            {
                var idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("EmployeeId");
                if (Guid.TryParse(idStr, out var id) && id != Guid.Empty)
                {
                    return id;
                }
                return null;
            }
        }

        public Guid? CompanyId
        {
            get
            {
                var idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("CompanyId");
                if (Guid.TryParse(idStr, out var id) && id != Guid.Empty)
                {
                    return id;
                }
                return null;
            }
        }

        public Guid? BranchId
        {
            get
            {
                var idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("BranchId");
                if (Guid.TryParse(idStr, out var id) && id != Guid.Empty)
                {
                    return id;
                }
                return null;
            }
        }

        public string? UserCode => _httpContextAccessor.HttpContext?.User?.FindFirstValue("UserCode") ?? Username;

        public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

        public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent => _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}
