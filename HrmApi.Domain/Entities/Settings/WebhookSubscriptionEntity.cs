using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>
    /// Đăng ký webhook nhận sự kiện hệ thống.
    /// </summary>
    public class WebhookSubscriptionEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        /// <summary>Danh sách event types phân tách bằng dấu phẩy</summary>
        public string EventTypes { get; set; } = string.Empty;

        public string? Secret { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }
    }
}
