namespace HrmApi.Application.DTOs.PublicHoliday
{
    public class PublicHolidayDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public DateOnly HolidayDate { get; set; }
        public bool IsRecurringYearly { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
