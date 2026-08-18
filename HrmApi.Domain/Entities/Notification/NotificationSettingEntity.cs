using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Notification
{
    /// <summary>
    /// Cài đặt tùy chọn nhận thông báo của người dùng.
    /// </summary>
    public class NotificationSettingEntity : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? EmployeeId { get; set; }

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
}
