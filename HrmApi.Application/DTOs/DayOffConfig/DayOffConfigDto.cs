namespace HrmApi.Application.DTOs.DayOffConfig
{
    public class DayOffConfigDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public decimal DefaultDaysPerYear { get; set; }
        public bool IsPaid { get; set; }
        public bool DeductBalance { get; set; } = true;
        public bool RequireAttachment { get; set; }
        public decimal? MaxDaysPerRequest { get; set; }
        public int MinNoticeDays { get; set; }
        public bool IsActive { get; set; }
    }

    public class DayOffConfigSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public bool RequireAttachment { get; set; }
        public bool DeductBalance { get; set; }
        public decimal DefaultDaysPerYear { get; set; }
    }
}
