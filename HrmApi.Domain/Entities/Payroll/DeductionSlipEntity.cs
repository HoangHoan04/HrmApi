using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Phiếu khấu trừ (phạt, bồi thường, hoàn ứng,...) áp vào kỳ lương.
    /// </summary>
    public class DeductionSlipEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DeductionDate { get; set; }

        /// <summary>
        /// Loại khấu trừ — <see cref="DeductionSlipType"/>.
        /// </summary>
        public string? DeductionType { get; set; }

        public int? ApplyMonth { get; set; }
        public int? ApplyYear { get; set; }
        public string Status { get; set; } = SlipStatus.Pending;
        public string? Reason { get; set; }
        public string? Note { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public virtual EmployeeEntity? Employee { get; set; }
    }
}
