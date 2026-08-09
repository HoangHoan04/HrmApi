using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Tổng hợp công theo tháng / nhân viên
    /// </summary>
    public class TimekeepingSummaryEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public int OnTimeDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyDays { get; set; }
        public int LeaveDays { get; set; }
        public int AbsentDays { get; set; }
        public int IncompleteDays { get; set; }
        public int WorkingDays { get; set; }
        public int TotalWorkedMinutes { get; set; }
        public int TotalLateMinutes { get; set; }
        public int TotalEarlyMinutes { get; set; }
    }
}
