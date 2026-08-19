namespace HrmApi.Application.DTOs.TimeKeepingStandard
{
    public class TimeKeepingStandardDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int AllowedRadiusMeters { get; set; }
        public int LateGraceMinutes { get; set; }
        public int EarlyLeaveGraceMinutes { get; set; }
        public TimeSpan NightStartTime { get; set; }
        public TimeSpan NightEndTime { get; set; }
        public bool IsActive { get; set; }
    }

    public class TimeKeepingStandardSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
    }
}
