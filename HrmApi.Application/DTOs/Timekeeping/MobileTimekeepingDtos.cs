using HrmApi.Domain.Enums;

namespace HrmApi.Application.DTOs.Timekeeping
{
    public class MobilePunchRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class MobileTodayDto
    {
        public DateOnly WorkDate { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.INCOMPLETE;
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public int WorkedMinutes { get; set; }
        public string? Note { get; set; }
        public bool CanCheckIn { get; set; }
        public bool CanCheckOut { get; set; }
        public bool OnLeave { get; set; }
        public TimeSpan? ExpectedStart { get; set; }
        public TimeSpan? ExpectedEnd { get; set; }
        public TimeSpan? ExpectedBreakStart { get; set; }
        public TimeSpan? ExpectedBreakEnd { get; set; }
        public int BreakMinutes { get; set; }
        public bool IsScheduledWorkDay { get; set; } = true;
        public string? ScheduleSource { get; set; }
        public string? BranchName { get; set; }
        public int AllowedRadiusMeters { get; set; }
    }

    public class MobileMonthDayDto
    {
        public DateOnly WorkDate { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.INCOMPLETE;
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public int WorkedMinutes { get; set; }
        public double? WorkedHours => WorkedMinutes > 0 ? Math.Round(WorkedMinutes / 60.0, 2) : null;
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public string? Note { get; set; }
    }

    public class MobileMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<MobileMonthDayDto> Days { get; set; } = [];
        public int OnTimeDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyDays { get; set; }
        public int LeaveDays { get; set; }
        public int AbsentDays { get; set; }
        public int IncompleteDays { get; set; }
        public int TotalWorkedMinutes { get; set; }
        public int ExpectedWorkingDays { get; set; }
        public int DailyExpectedMinutes { get; set; }
        public int ExpectedWorkedMinutes { get; set; }
    }
}
