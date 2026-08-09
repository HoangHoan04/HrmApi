using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Cấu hình loại nghỉ phép / số ngày phép theo năm
    /// </summary>
    public class DayOffConfigEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Loại nghỉ: ANNUAL, SICK, UNPAID, OTHER
        /// </summary>
        public string DayOffType { get; set; } = "ANNUAL";

        /// <summary>
        /// Số ngày phép mặc định trong năm
        /// </summary>
        public decimal DefaultDaysPerYear { get; set; }

        /// <summary>
        /// Có tính lương hay không
        /// </summary>
        public bool IsPaid { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}
