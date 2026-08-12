using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Đơn đăng ký nghỉ phép
    /// </summary>
    public class RegisterDayOffEntity : BaseEntity
    {
        /// <summary>
        /// Id nhân viên (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }
        /// <summary>
        /// Id công ty (khóa ngoại tới CompanyEntity)
        /// </summary>
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// Id chi nhánh (khóa ngoại tới BranchEntity)
        /// </summary>
        public Guid? BranchId { get; set; }
        /// <summary>
        /// Id cấu hình ngày phép (khóa ngoại tới DayOffConfigEntity)
        /// </summary>
        public Guid? DayOffConfigId { get; set; }

        /// <summary>
        /// Loại nghỉ phép
        /// </summary>
        public HrmApi.Domain.Enums.DayOffType DayOffType { get; set; } = HrmApi.Domain.Enums.DayOffType.ANNUAL;
        /// <summary>
        /// Ngày bắt đầu nghỉ phép
        /// </summary>
        public DateOnly FromDate { get; set; }
        /// <summary>
        /// Ngày kết thúc nghỉ phép
        /// </summary>
        public DateOnly ToDate { get; set; }

        /// <summary>
        /// Buổi nghỉ: cả ngày / sáng / chiều
        /// </summary>
        public HrmApi.Domain.Enums.LeaveSession Session { get; set; } = HrmApi.Domain.Enums.LeaveSession.FULL;

        /// <summary>
        /// Tổng số ngày nghỉ phép (có thể là số nguyên hoặc số thập phân)
        /// </summary>
        public decimal TotalDays { get; set; }
        /// <summary>
        /// Lý do nghỉ phép
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// URL file đính kèm (đơn giấy / giấy tờ liên quan)
        /// </summary>
        public string? AttachmentUrl { get; set; }

        /// <summary>
        /// Trạng thái đơn đăng ký nghỉ phép
        /// </summary>
        public HrmApi.Domain.Enums.DayOffStatus Status { get; set; } = HrmApi.Domain.Enums.DayOffStatus.PENDING;

        /// <summary>
        /// Người được đề xuất duyệt (Part/Dept/Branch Manager) tại thời điểm tạo đơn
        /// </summary>
        public Guid? RequestedApproverId { get; set; }

        /// <summary>
        /// Id người phê duyệt đơn đăng ký nghỉ phép (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid? ApproverId { get; set; }
        /// <summary>
        /// Ngày phê duyệt đơn đăng ký nghỉ phép
        /// </summary>
        public DateTime? ApprovedAt { get; set; }
        /// <summary>
        /// Ghi chú của người phê duyệt (nếu có)
        /// </summary>
        public string? ApproverNote { get; set; }

        /// <summary>
        /// Lý do hủy đơn (nếu có)
        /// </summary>
        public string? CancelReason { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên sở hữu đơn đăng ký nghỉ phép
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }
        /// <summary>
        /// Navigation property tới công ty mà nhân viên đang làm việc
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh mà nhân viên đang làm việc
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }
        /// <summary>
        /// Navigation property tới cấu hình loại nghỉ phép
        /// </summary>
        public virtual DayOffConfigEntity? DayOffConfig { get; set; }
        /// <summary>
        /// Navigation property tới người được đề xuất duyệt
        /// </summary>
        public virtual EmployeeEntity? RequestedApprover { get; set; }
        /// <summary>
        /// Navigation property tới người phê duyệt (Employee)
        /// </summary>
        public virtual EmployeeEntity? Approver { get; set; }


    }
}
