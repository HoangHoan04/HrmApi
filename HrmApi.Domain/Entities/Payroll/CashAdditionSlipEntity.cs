using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Phiếu thu nhập thêm / thưởng / hỗ trợ đột xuất áp vào kỳ lương.
    /// </summary>
    public class CashAdditionSlipEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime AdditionDate { get; set; }

        /// <summary>
        /// Loại thu nhập thêm — <see cref="AdditionSlipType"/>.
        /// </summary>
        public string? AdditionType { get; set; }

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
