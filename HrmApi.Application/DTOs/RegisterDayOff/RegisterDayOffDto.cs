using System;

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
        public string DayOffType { get; set; } = "ANNUAL";
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public decimal TotalDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "PENDING";
        public Guid? ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApproverNote { get; set; }
    }

    public class CreateRegisterDayOffRequest
    {
        public Guid? DayOffConfigId { get; set; }
        public string? DayOffType { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string? Reason { get; set; }
    }
}
