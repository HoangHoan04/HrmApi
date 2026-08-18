using System;
using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Notification
{
    /// <summary>
    /// Bản ghi thông báo trong hệ thống (In-App Notification) cho người dùng / nhân viên.
    /// </summary>
    public class NotificationEntity : BaseEntity
    {
        /// <summary>ID tài khoản nhận thông báo</summary>
        public Guid UserId { get; set; }

        /// <summary>ID nhân viên nhận thông báo (nếu có)</summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>Tiêu đề thông báo</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Nội dung chi tiết thông báo</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Phân loại: LEAVE | OVERTIME | ATTENDANCE | PAYSLIP | CONTRACT | RECRUITMENT | SYSTEM | ANNOUNCEMENT</summary>
        public string Type { get; set; } = NotificationType.System;

        /// <summary>Mức độ: INFO | SUCCESS | WARNING | DANGER</summary>
        public string Severity { get; set; } = NotificationSeverity.Info;

        /// <summary>Đường dẫn deep-link trên Web Admin (ví dụ: /operate-manager/leave-manager)</summary>
        public string? TargetUrl { get; set; }

        /// <summary>Loại đối tượng nghiệp vụ (ví dụ: LEAVE_REQUEST, OVERTIME_REQUEST, PAYSLIP)</summary>
        public string? TargetType { get; set; }

        /// <summary>ID của đối tượng nghiệp vụ</summary>
        public Guid? TargetId { get; set; }

        /// <summary>Dữ liệu JSON metadata mở rộng</summary>
        public string? DataJson { get; set; }

        /// <summary>Trạng thái đã đọc</summary>
        public bool IsRead { get; set; } = false;

        /// <summary>Thời điểm đọc thông báo</summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>Đánh dấu thông báo phát sóng (broadcast)</summary>
        public bool IsBroadcast { get; set; } = false;

        /// <summary>Người gửi thông báo (null = do hệ thống tự sinh)</summary>
        public Guid? SenderId { get; set; }
    }
}
