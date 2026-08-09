using System;

namespace HrmApi.Application.DTOs.ShiftMaster
{
    public class ShiftMasterDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakMinutes { get; set; }
        public int WorkingMinutes { get; set; }
        public bool IsOvernight { get; set; }
        public bool IsActive { get; set; }
    }

    public class ShiftMasterSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
