namespace HrmApi.Domain.Enums
{
    /// <summary>
    /// Trạng thái đơn nghỉ phép
    /// </summary>
    public static class DayOffStatus
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Cancelled = "CANCELLED";
    }
}
