using HrmApi.Application.Common.Interfaces;
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
                string? idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(idStr, out Guid id) ? id : null;
            }
        }

        public Guid? EmployeeId
        {
            get
            {
                string? idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("EmployeeId");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public Guid? CompanyId
        {
            get
            {
                string? idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("CompanyId");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public Guid? BranchId
        {
            get
            {
                string? idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("BranchId");
                return Guid.TryParse(idStr, out Guid id) && id != Guid.Empty ? id : null;
            }
        }

        public string? UserCode => _httpContextAccessor.HttpContext?.User?.FindFirstValue("UserCode") ?? Username;

        public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

        public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent => _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}
