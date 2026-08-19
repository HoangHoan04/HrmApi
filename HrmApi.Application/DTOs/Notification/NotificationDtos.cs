using HrmApi.Domain.Enums;

namespace HrmApi.Application.DTOs.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = NotificationType.System;
        public string Severity { get; set; } = NotificationSeverity.Info;
        public string? TargetUrl { get; set; }
        public string? TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public string? DataJson { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsBroadcast { get; set; }
        public Guid? SenderId { get; set; }
        public string? SenderName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = NotificationType.System;
        public string Severity { get; set; } = NotificationSeverity.Info;
        public string? TargetUrl { get; set; }
        public string? TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public string? DataJson { get; set; }
        public Guid? SenderId { get; set; }
    }

    public class BroadcastNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = NotificationType.Announcement;
        public string Severity { get; set; } = NotificationSeverity.Info;
        public string? TargetUrl { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public List<Guid>? TargetUserIds { get; set; }
    }

    public class NotificationSettingDto
    {
        public Guid UserId { get; set; }
        public bool EmailEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = true;
        public bool InAppEnabled { get; set; } = true;
        public bool NotifyOnLeave { get; set; } = true;
        public bool NotifyOnOvertime { get; set; } = true;
        public bool NotifyOnAttendance { get; set; } = true;
        public bool NotifyOnPayslip { get; set; } = true;
        public bool NotifyOnContract { get; set; } = true;
        public bool NotifyOnRecruitment { get; set; } = true;
    }

    public class RegisterDeviceTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = DevicePlatform.Android;
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
    }
}
