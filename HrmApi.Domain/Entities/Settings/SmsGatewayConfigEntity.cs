using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>Cấu hình cổng SMS (adapter tích hợp).</summary>
    public class SmsGatewayConfigEntity : BaseEntity
    {
        public string Provider { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string? SenderId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }
    }
}
