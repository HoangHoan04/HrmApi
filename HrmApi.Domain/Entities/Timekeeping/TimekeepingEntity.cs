using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Bản ghi chấm công theo ngày / nhân viên
    /// </summary>
    public class TimekeepingEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Ngày công
        /// </summary>
        public DateOnly WorkDate { get; set; }

        public Guid? ShiftId { get; set; }
        public Guid? ShiftMasterId { get; set; }

        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }

        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }

        public double? CheckInDistanceM { get; set; }
        public double? CheckOutDistanceM { get; set; }

        /// <summary>
        /// ON_TIME | LATE | EARLY | ABSENT | LEAVE | INCOMPLETE
        /// </summary>
        public string Status { get; set; } = "INCOMPLETE";

        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public int WorkedMinutes { get; set; }

        public string? Note { get; set; }

        /// <summary>
        /// Đã được Admin chỉnh tay
        /// </summary>
        public bool IsManualAdjusted { get; set; }
    }
}
