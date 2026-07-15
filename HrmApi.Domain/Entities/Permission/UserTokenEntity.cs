using HrmApi.Domain.Common;
using System;

namespace HrmApi.Domain.Entities.Permission
{
    /* Lưu refresh token / phiên đăng nhập, phục vụ JWT refresh + đăng xuất từ xa + quản lý thiết bị */
    public class UserTokenEntity : BaseEntity
    {
        public Guid UserId { get; set; }

        /* Refresh token dạng hash, không lưu plain text */
        public string RefreshTokenHash { get; set; } = string.Empty;

        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }

        /* WEB / MOBILE_IOS / MOBILE_ANDROID */
        public string Platform { get; set; } = string.Empty;

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }

        /* Token thay thế token này khi refresh (token rotation) - chống replay attack */
        public Guid? ReplacedByTokenId { get; set; }

        public UserEntity User { get; set; } = null!;
    }
}