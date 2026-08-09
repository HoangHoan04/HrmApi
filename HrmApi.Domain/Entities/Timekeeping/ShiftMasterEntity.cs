using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Mẫu ca làm việc
    /// </summary>
    public class ShiftMasterEntity : BaseEntity
    {
        /// <summary>
        /// Mã ca
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên ca
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Giờ bắt đầu ca
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc ca
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Số phút nghỉ giữa ca
        /// </summary>
        public int BreakMinutes { get; set; }

        /// <summary>
        /// Số phút làm việc chuẩn
        /// </summary>
        public int WorkingMinutes { get; set; }

        /// <summary>
        /// Ca qua đêm (end &lt; start)
        /// </summary>
        public bool IsOvernight { get; set; }

        /// <summary>
        /// Đang kích hoạt
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
