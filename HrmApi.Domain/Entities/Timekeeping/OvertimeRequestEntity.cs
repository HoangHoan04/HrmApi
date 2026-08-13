using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Đơn đăng ký / duyệt tăng ca (OT).
    /// </summary>
    public class OvertimeRequestEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }

        public DateOnly WorkDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        /// <summary>Số phút OT đăng ký (tính từ From/To hoặc nhập tay).</summary>
        public int RequestedMinutes { get; set; }

        /// <summary>Số phút OT được duyệt (null = dùng RequestedMinutes).</summary>
        public int? ApprovedMinutes { get; set; }

        public string OtType { get; set; } = OvertimeType.AfterShift;
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }

        public string Status { get; set; } = OvertimeRequestStatus.Draft;

        public Guid? ApproverId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ApproverNote { get; set; }

        public virtual EmployeeEntity? Employee { get; set; }
        public virtual EmployeeEntity? Approver { get; set; }
    }
}
