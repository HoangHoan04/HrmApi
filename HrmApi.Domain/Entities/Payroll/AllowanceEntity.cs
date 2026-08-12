using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Danh mục phụ cấp (ăn trưa, xăng xe, điện thoại,...).
    /// </summary>
    public class AllowanceEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Số tiền mặc định của phụ cấp
        /// </summary>
        public decimal? DefaultAmount { get; set; }

        /// <summary>
        /// Có tính thuế TNCN hay không
        /// </summary>
        public bool IsTaxable { get; set; } = true;

        /// <summary>
        /// Có tính vào lương đóng BH hay không
        /// </summary>
        public bool IsInsurable { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public virtual CompanyEntity? Company { get; set; }
    }
}
