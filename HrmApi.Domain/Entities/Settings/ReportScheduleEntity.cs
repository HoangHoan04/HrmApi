using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>
    /// Lịch gửi báo cáo định kỳ (hết hạn HĐ, phép tồn, kỳ lương).
    /// </summary>
    public class ReportScheduleEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>CONTRACT_EXPIRY | LEAVE_BALANCE | PAYROLL_PERIOD</summary>
        public string ReportType { get; set; } = ReportScheduleType.ContractExpiry;

        /// <summary>Gợi ý chu kỳ: DAILY / WEEKLY / MONTHLY</summary>
        public string CronHint { get; set; } = ReportCronHint.Daily;

        public string EmailTo { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastRunAt { get; set; }
        public string? Note { get; set; }
    }
}
