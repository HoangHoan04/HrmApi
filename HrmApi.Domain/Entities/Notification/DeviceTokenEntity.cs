using System;
using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Notification
{
    /// <summary>
    /// Lưu trữ Token thiết bị Mobile (FCM / Expo Push Token) để gửi push notification.
    /// </summary>
    public class DeviceTokenEntity : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = DevicePlatform.Android;
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
