namespace HrmApi.Domain.Enums
{
    public static class NotificationType
    {
        public const string Leave = "LEAVE";
        public const string Overtime = "OVERTIME";
        public const string Attendance = "ATTENDANCE";
        public const string Payslip = "PAYSLIP";
        public const string Contract = "CONTRACT";
        public const string Recruitment = "RECRUITMENT";
        public const string Performance = "PERFORMANCE";
        public const string System = "SYSTEM";
        public const string Announcement = "ANNOUNCEMENT";
    }

    public static class NotificationSeverity
    {
        public const string Info = "INFO";
        public const string Success = "SUCCESS";
        public const string Warning = "WARNING";
        public const string Danger = "DANGER";
    }

    public static class DevicePlatform
    {
        public const string Ios = "IOS";
        public const string Android = "ANDROID";
        public const string Web = "WEB";
    }
}
