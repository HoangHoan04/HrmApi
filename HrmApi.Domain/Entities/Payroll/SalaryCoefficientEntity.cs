using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Hệ số lương theo bậc/ngạch (thang bảng lương).
    /// </summary>
    public class SalaryCoefficientEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Bậc / cấp bậc áp dụng
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// Hệ số lương
        /// </summary>
        public decimal Coefficient { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public virtual CompanyEntity? Company { get; set; }
    }
}
