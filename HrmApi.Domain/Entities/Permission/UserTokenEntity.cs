using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Lưu refresh token / phiên đăng nhập, phục vụ JWT refresh + đăng xuất từ xa + quản lý thiết bị
    /// </summary>
    public class UserTokenEntity : BaseEntity
    {
        /// <summary>
        /// Id người dùng (khóa ngoại tới UserEntity)
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Hash của refresh token, để xác thực khi refresh token được gửi lên (không lưu trữ refresh token gốc để bảo mật)
        /// </summary>
        public string RefreshTokenHash { get; set; } = string.Empty;
        /// <summary>
        /// Id thiết bị (device) mà người dùng đang sử dụng, để quản lý đăng xuất từ xa và hạn chế số lượng thiết bị đăng nhập
        /// </summary>
        public string? DeviceId { get; set; }
        /// <summary>
        /// Tên thiết bị (device) mà người dùng đang sử dụng, để hiển thị trong danh sách thiết bị đăng nhập
        /// </summary>
        public string? DeviceName { get; set; }
        /// <summary>
        /// Hệ điều hành của thiết bị (device) mà người dùng đang sử dụng, để hiển thị trong danh sách thiết bị đăng nhập
        /// </summary>
        public string Platform { get; set; } = string.Empty;
        /// <summary>
        /// Địa chỉ IP của thiết bị (device) mà người dùng đang sử dụng, để hiển thị trong danh sách thiết bị đăng nhập
        /// </summary>
        public string? IpAddress { get; set; }
        /// <summary>
        /// User agent của thiết bị (device) mà người dùng đang sử dụng, để hiển thị trong danh sách thiết bị đăng nhập
        /// </summary>
        public string? UserAgent { get; set; }
        /// <summary>
        /// Ngày hết hạn của refresh token, để xác định khi nào cần yêu cầu người dùng đăng nhập lại
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Ngày refresh token bị thu hồi (nếu có), để xác định khi nào cần yêu cầu người dùng đăng nhập lại
        /// </summary>
        public DateTime? RevokedAt { get; set; }
        /// <summary>
        /// Lý do refresh token bị thu hồi (nếu có), để hiển thị trong danh sách thiết bị đăng nhập
        /// </summary>
        public string? RevokedReason { get; set; }
        /// <summary>
        /// Id của refresh token thay thế cho refresh token này (nếu có), để xác định khi nào cần yêu cầu người dùng đăng nhập lại
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }
        /// <summary>
        /// Navigation property tới người dùng sở hữu refresh token này
        /// </summary>
        public UserEntity User { get; set; } = null!;
    }
}