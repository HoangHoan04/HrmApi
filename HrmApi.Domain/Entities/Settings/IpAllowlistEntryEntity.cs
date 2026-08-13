using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>
    /// IP/CIDR được phép truy cập API khi allowlist đang có entry active.
    /// </summary>
    public class IpAllowlistEntryEntity : BaseEntity
    {
        /// <summary>
        /// Địa chỉ IP hoặc CIDR (ví dụ 192.168.1.10 hoặc 10.0.0.0/24)
        /// </summary>
        public string CidrOrIp { get; set; } = string.Empty;

        public string? Note { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
