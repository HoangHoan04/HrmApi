namespace HrmApi.Domain.Enums
{
    public static class ReportScheduleType
    {
        public const string ContractExpiry = "CONTRACT_EXPIRY";
        public const string LeaveBalance = "LEAVE_BALANCE";
        public const string PayrollPeriod = "PAYROLL_PERIOD";
    }

    public static class ReportCronHint
    {
        public const string Daily = "DAILY";
        public const string Weekly = "WEEKLY";
        public const string Monthly = "MONTHLY";
    }

    public static class NotificationChannel
    {
        public const string Email = "EMAIL";
        public const string Sms = "SMS";
    }
}
