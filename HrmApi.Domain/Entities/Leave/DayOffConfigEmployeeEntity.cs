using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Số ngày phép còn lại / được cấp cho từng nhân viên
    /// </summary>
    public class DayOffConfigEmployeeEntity : BaseEntity
    {
        public Guid DayOffConfigId { get; set; }
        public Guid EmployeeId { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public string? Note { get; set; }
    }
}
