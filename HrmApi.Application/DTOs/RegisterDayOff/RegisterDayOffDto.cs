using HrmApi.Domain.Enums;

namespace HrmApi.Application.DTOs.RegisterDayOff
{
    public class RegisterDayOffDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? DayOffConfigId { get; set; }
        public string? DayOffConfigName { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public LeaveSession Session { get; set; } = LeaveSession.FULL;
        public decimal TotalDays { get; set; }
        public string? Reason { get; set; }
        public string? AttachmentUrl { get; set; }
        public DayOffStatus Status { get; set; } = DayOffStatus.PENDING;
        public Guid? RequestedApproverId { get; set; }
        public string? RequestedApproverName { get; set; }
        public Guid? ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApproverNote { get; set; }
        public string? CancelReason { get; set; }
    }

    public class PreviewLeaveDaysRequest
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public LeaveSession Session { get; set; } = LeaveSession.FULL;
    }

    public class PreviewLeaveDaysDto
    {
        public decimal TotalDays { get; set; }
        public SaturdayPolicy SaturdayPolicy { get; set; }
        public LeaveSession Session { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
    }

    public class MobileLeaveBalanceDto
    {
        public int Year { get; set; }
        public decimal AnnualTotal { get; set; }
        public decimal AnnualUsed { get; set; }
        public decimal AnnualPending { get; set; }
        public decimal AnnualRemaining { get; set; }
        public decimal SickUsed { get; set; }
        public decimal UnpaidUsed { get; set; }
        public List<MobileLeaveConfigDto> Configs { get; set; } = [];
    }

    public class MobileLeaveConfigDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal DefaultDaysPerYear { get; set; }
        public bool IsPaid { get; set; }
        public bool DeductBalance { get; set; }
        public bool RequireAttachment { get; set; }
        public decimal? MaxDaysPerRequest { get; set; }
        public int MinNoticeDays { get; set; }
    }
}
