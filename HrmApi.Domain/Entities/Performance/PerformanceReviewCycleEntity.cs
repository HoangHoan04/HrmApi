using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Performance
{
    public class PerformanceReviewCycleEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public DateOnly PeriodFrom { get; set; }
        public DateOnly PeriodTo { get; set; }
        public string Status { get; set; } = ReviewCycleStatus.Draft;
        public string? Note { get; set; }

        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public List<KpiGoalEntity> Goals { get; set; } = [];
    }
}
