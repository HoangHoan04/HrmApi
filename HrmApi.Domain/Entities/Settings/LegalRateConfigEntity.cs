using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>
    /// Cấu hình tỷ lệ BHXH/BHYT/BHTN và giảm trừ thuế theo năm.
    /// </summary>
    public class LegalRateConfigEntity : BaseEntity
    {
        public int Year { get; set; }
        public decimal SocialInsuranceEmployeeRate { get; set; }
        public decimal SocialInsuranceEmployerRate { get; set; }
        public decimal HealthInsuranceRate { get; set; }
        public decimal UnemploymentRate { get; set; }
        public decimal PersonalDeduction { get; set; }
        public decimal DependentDeduction { get; set; }
        public string? Note { get; set; }
    }
}
