using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Performance
{
    public class KpiGoalEntity : BaseEntity
    {
        public Guid CycleId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public string? Unit { get; set; }
        public decimal Weight { get; set; } = 1;

        public PerformanceReviewCycleEntity? Cycle { get; set; }
        public EmployeeEntity? Employee { get; set; }
        public List<KpiResultEntity> Results { get; set; } = [];
    }
}
