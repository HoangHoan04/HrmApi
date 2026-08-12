using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Quyết định tăng lương (khác với lịch sử lương nhân viên — đây là phiếu đề xuất/duyệt tăng lương).
    /// </summary>
    public class SalaryIncreaseEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public decimal? OldBasicSalary { get; set; }
        public decimal NewBasicSalary { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string? DecisionNumber { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "PENDING";
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? Note { get; set; }

        public virtual EmployeeEntity? Employee { get; set; }
    }
}
