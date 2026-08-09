using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Chuẩn chấm công (bán kính GPS, phút ân hạn đi trễ/về sớm)
    /// </summary>
    public class TimeKeepingStandardEntity : BaseEntity
    {
        /// <summary>
        /// Mã chuẩn chấm công
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên chuẩn chấm công
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty sở hữu
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Bán kính cho phép chấm công (mét), mặc định 200
        /// </summary>
        public int AllowedRadiusMeters { get; set; } = 200;

        /// <summary>
        /// Số phút ân hạn đi trễ
        /// </summary>
        public int LateGraceMinutes { get; set; } = 0;

        /// <summary>
        /// Số phút ân hạn về sớm
        /// </summary>
        public int EarlyLeaveGraceMinutes { get; set; } = 0;

        /// <summary>
        /// Đang kích hoạt
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
