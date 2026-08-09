using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Xếp lịch làm việc cho nhân viên theo ngày
    /// </summary>
    public class WorkScheduledEmployeeEntity : BaseEntity
    {
        /// <summary>
        /// Id nhân viên
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Id ca cụ thể (nếu có)
        /// </summary>
        public Guid? ShiftId { get; set; }

        /// <summary>
        /// Id mẫu ca (dùng khi không tạo Shift instance)
        /// </summary>
        public Guid? ShiftMasterId { get; set; }

        /// <summary>
        /// Ngày làm việc
        /// </summary>
        public DateOnly WorkDate { get; set; }

        /// <summary>
        /// Id chi nhánh làm việc
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }
    }
}
