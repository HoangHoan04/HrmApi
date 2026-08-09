using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Ca làm việc theo ngày / chi nhánh (instance của ShiftMaster)
    /// </summary>
    public class ShiftEntity : BaseEntity
    {
        /// <summary>
        /// Id mẫu ca
        /// </summary>
        public Guid ShiftMasterId { get; set; }

        /// <summary>
        /// Id chi nhánh áp dụng
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Ngày làm việc (null nếu ca recurring theo master)
        /// </summary>
        public DateOnly? WorkDate { get; set; }

        /// <summary>
        /// Giờ bắt đầu (có thể override master)
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }
    }
}
