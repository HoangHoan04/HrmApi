using HrmApi.Application.DTOs;

namespace HrmApi.Application.DTOs.OvertimeRequest
{
    public class OvertimeRequestDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateOnly WorkDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public int RequestedMinutes { get; set; }
        public int? ApprovedMinutes { get; set; }
        public string OtType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ApproverNote { get; set; }
    }
}
