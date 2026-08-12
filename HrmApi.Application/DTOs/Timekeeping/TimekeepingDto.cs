using System;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.DTOs.Timekeeping
{
    public class TimekeepingDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateOnly WorkDate { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public string? ShiftMasterName { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
        public double? CheckInDistanceM { get; set; }
        public double? CheckOutDistanceM { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.INCOMPLETE;
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public int WorkedMinutes { get; set; }
        public string? Note { get; set; }
        public bool IsManualAdjusted { get; set; }
    }
}
