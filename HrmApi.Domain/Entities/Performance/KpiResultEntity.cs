using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Performance
{
    public class KpiResultEntity : BaseEntity
    {
        public Guid GoalId { get; set; }
        public decimal ActualValue { get; set; }
        public decimal Score { get; set; }
        public string? Comment { get; set; }
        public Guid? RatedByEmployeeId { get; set; }
        public DateTime? RatedAt { get; set; }

        public KpiGoalEntity? Goal { get; set; }
        public EmployeeEntity? RatedByEmployee { get; set; }
    }
}
