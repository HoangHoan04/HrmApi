using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Phiếu tạm ứng lương của nhân viên.
    /// </summary>
    public class AdvanceEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Tháng sẽ khấu trừ tạm ứng trên phiếu lương
        /// </summary>
        public int? DeductMonth { get; set; }

        /// <summary>
        /// Năm sẽ khấu trừ tạm ứng trên phiếu lương
        /// </summary>
        public int? DeductYear { get; set; }

        /// <summary>
        /// Trạng thái (DRAFT, PENDING, APPROVED, APPLIED, REJECTED, CANCELLED)
        /// </summary>
        public string Status { get; set; } = "PENDING";

        public string? Reason { get; set; }
        public string? Note { get; set; }

        public virtual EmployeeEntity? Employee { get; set; }
    }
}
