using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>
    /// API client key (chỉ lưu hash; plaintext trả về một lần khi tạo).
    /// </summary>
    public class ApiClientKeyEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string KeyHash { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public string? Note { get; set; }

        public virtual CompanyEntity? Company { get; set; }
    }
}
