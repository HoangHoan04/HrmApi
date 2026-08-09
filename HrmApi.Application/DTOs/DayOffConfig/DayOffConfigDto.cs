using System;

namespace HrmApi.Application.DTOs.DayOffConfig
{
    public class DayOffConfigDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string DayOffType { get; set; } = "ANNUAL";
        public decimal DefaultDaysPerYear { get; set; }
        public bool IsPaid { get; set; }
        public bool IsActive { get; set; }
    }

    public class DayOffConfigSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DayOffType { get; set; } = "ANNUAL";
        public Guid? CompanyId { get; set; }
    }
}
