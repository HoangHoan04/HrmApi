using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Chi tiết dòng thu nhập / khấu trừ trên phiếu lương.
    /// </summary>
    public class SalaryLineItemEntity : BaseEntity
    {
        /// <summary>
        /// Id phiếu lương (Require)
        /// </summary>
        public Guid SalaryId { get; set; }

        /// <summary>
        /// Loại dòng: INCOME | DEDUCTION
        /// </summary>
        public string ItemType { get; set; } = string.Empty;

        /// <summary>
        /// Mã dòng (BASIC, LUNCH, BHXH, PIT,...)
        /// </summary>
        public string ItemCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên hiển thị dòng
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền (thu nhập > 0; khấu trừ lưu số dương, UI hiển thị dấu trừ)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Ghi chú dòng
        /// </summary>
        public string? Note { get; set; }

        public virtual SalaryEntity? Salary { get; set; }
    }
}
