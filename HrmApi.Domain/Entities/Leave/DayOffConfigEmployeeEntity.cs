using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Số ngày phép còn lại / được cấp cho từng nhân viên
    /// </summary>
    public class DayOffConfigEmployeeEntity : BaseEntity
    {
        /// <summary>
        /// Id cấu hình ngày phép (khóa ngoại tới DayOffConfigEntity)
        /// </summary>
        public Guid DayOffConfigId { get; set; }

        /// <summary>
        /// Id nhân viên (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Năm áp dụng số ngày phép
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Số ngày phép được cấp cho nhân viên trong năm (có thể là số nguyên hoặc số thập phân)
        /// </summary>
        public decimal AllocatedDays { get; set; }
        /// <summary>
        /// Số ngày phép đã sử dụng trong năm (có thể là số nguyên hoặc số thập phân)
        /// </summary>
        public decimal UsedDays { get; set; }

        /// <summary>
        /// Số ngày phép còn lại trong năm (có thể là số nguyên hoặc số thập phân)
        /// </summary>
        public decimal RemainingDays { get; set; }
        /// <summary>
        /// Ghi chú thêm về số ngày phép của nhân viên
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Navigation property tới cấu hình ngày phép (DayOffConfigEntity)
        /// </summary>
        public virtual DayOffConfigEntity? DayOffConfig { get; set; }
        /// <summary>
        /// Navigation property tới nhân viên sở hữu số ngày phép này (EmployeeEntity)
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }


    }
}
