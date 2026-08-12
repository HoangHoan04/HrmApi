using HrmApi.Application.DTOs;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.DTOs.AttendanceComplaint
{
    public class AttendanceComplaintDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateOnly WorkDate { get; set; }
        public Guid? TimekeepingId { get; set; }
        public AttendanceComplaintType ComplaintType { get; set; }
        public string? ComplaintTypeLabel { get; set; }
        public TimeSpan? RequestedCheckInTime { get; set; }
        public TimeSpan? RequestedCheckOutTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public AttendanceComplaintStatus Status { get; set; }
        public Guid? ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ApproverNote { get; set; }

        public DateTime? CurrentCheckInAt { get; set; }
        public DateTime? CurrentCheckOutAt { get; set; }
        public AttendanceStatus? CurrentStatus { get; set; }
    }
}
