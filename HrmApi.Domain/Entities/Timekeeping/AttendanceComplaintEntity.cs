using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Khiếu nại / yêu cầu chỉnh sửa chấm công (quên punch, sai giờ...).
    /// Admin/HR duyệt → cập nhật TimekeepingEntity.
    /// </summary>
    public class AttendanceComplaintEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }

        public DateOnly WorkDate { get; set; }

        public Guid? TimekeepingId { get; set; }

        public AttendanceComplaintType ComplaintType { get; set; } =
            AttendanceComplaintType.FORGOT_BOTH;

        /// <summary>Giờ vào ca đề xuất (local business time).</summary>
        public TimeSpan? RequestedCheckInTime { get; set; }

        /// <summary>Giờ ra ca đề xuất (local business time).</summary>
        public TimeSpan? RequestedCheckOutTime { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; }

        public AttendanceComplaintStatus Status { get; set; } =
            AttendanceComplaintStatus.PENDING;

        public Guid? ApproverId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ApproverNote { get; set; }

        public virtual EmployeeEntity? Employee { get; set; }
        public virtual TimekeepingEntity? Timekeeping { get; set; }
        public virtual EmployeeEntity? Approver { get; set; }
    }
}
