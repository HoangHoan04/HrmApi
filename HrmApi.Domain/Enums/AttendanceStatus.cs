namespace HrmApi.Domain.Enums
{
    /// <summary>
    /// Trạng thái chấm công trong ngày
    /// </summary>
    public enum AttendanceStatus
    {
        ON_TIME = 1,
        LATE = 2,
        EARLY = 3,
        LEAVE = 4,
        ABSENT = 5,
        INCOMPLETE = 6
    }
}
