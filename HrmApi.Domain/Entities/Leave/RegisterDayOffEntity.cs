using System;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Đơn đăng ký nghỉ phép
    /// </summary>
    public class RegisterDayOffEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DayOffConfigId { get; set; }

        /// <summary>
        /// ANNUAL | SICK | UNPAID | OTHER
        /// </summary>
        public string DayOffType { get; set; } = "ANNUAL";

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public decimal TotalDays { get; set; }
        public string? Reason { get; set; }

        /// <summary>
        /// PENDING | APPROVED | REJECTED | CANCELLED
        /// </summary>
        public string Status { get; set; } = "PENDING";

        public Guid? ApproverId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApproverNote { get; set; }
    }
}
