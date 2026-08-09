using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Ngày nghỉ lễ
    /// </summary>
    public class PublicHolidayEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public DateOnly HolidayDate { get; set; }
        public bool IsRecurringYearly { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
