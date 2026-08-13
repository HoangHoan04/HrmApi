using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>
    /// Mẫu thông báo EMAIL/SMS dùng trong hệ thống.
    /// </summary>
    public class NotificationTemplateEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;

        /// <summary>EMAIL | SMS</summary>
        public string Channel { get; set; } = NotificationChannel.Email;

        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }
    }
}
