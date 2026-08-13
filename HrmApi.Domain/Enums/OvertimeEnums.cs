namespace HrmApi.Domain.Enums
{
    public static class OvertimeRequestStatus
    {
        public const string Draft = "DRAFT";
        public const string Submitted = "SUBMITTED";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Cancelled = "CANCELLED";
    }

    public static class OvertimeType
    {
        public const string AfterShift = "AFTER_SHIFT";
        public const string DayOff = "DAY_OFF";
        public const string Holiday = "HOLIDAY";
    }
}
