namespace HrmApi.Domain.Enums
{
    /// <summary>
    /// Trạng thái chấm công trong ngày
    /// </summary>
    public static class AttendanceStatus
    {
        public const string OnTime = "ON_TIME";
        public const string Late = "LATE";
        public const string Early = "EARLY";
        public const string Leave = "LEAVE";
        public const string Absent = "ABSENT";
        public const string Incomplete = "INCOMPLETE";
    }
}
