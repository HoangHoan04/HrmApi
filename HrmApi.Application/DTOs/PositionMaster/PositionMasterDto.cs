namespace HrmApi.Application.DTOs.PositionMaster
{
    public class PositionMasterDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public bool IsLimitHoursWorking { get; set; }
        public string? Limit { get; set; }
        public int? WorkingHour { get; set; }
        public int? MinimumWorkingHour { get; set; }
        public TimeSpan? HourWorkingStart { get; set; }
        public TimeSpan? HourWorkingEnd { get; set; }
        public bool IsTimeKeeping { get; set; }
        public TimeSpan? HourSnapShotStart { get; set; }
        public TimeSpan? HourSnapShotEnd { get; set; }
        public bool IsAllowOverTimekeepingStandard { get; set; }
        public bool IsSwapPosition { get; set; }
        public string? TargetChangePositionIds { get; set; }
        public bool IsApprovedWhenHiringCandidate { get; set; }
        public bool IsHadASecondInterview { get; set; }
        public bool IsApprovedDayOff { get; set; }
        public int? QuantityStandard { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
    public class PositionMasterSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

}
