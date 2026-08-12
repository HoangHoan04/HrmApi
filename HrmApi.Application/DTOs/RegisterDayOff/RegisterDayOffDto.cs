using System;
using System.Collections.Generic;
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
        public DayOffType DayOffType { get; set; } = DayOffType.ANNUAL;
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public decimal TotalDays { get; set; }
        public string? Reason { get; set; }
        public DayOffStatus Status { get; set; } = DayOffStatus.PENDING;
        public Guid? ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApproverNote { get; set; }
    }

    public class CreateRegisterDayOffRequest
    {
        public Guid? DayOffConfigId { get; set; }
        public DayOffType? DayOffType { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string? Reason { get; set; }
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
        public DayOffType DayOffType { get; set; }
        public decimal DefaultDaysPerYear { get; set; }
        public bool IsPaid { get; set; }
    }
}
