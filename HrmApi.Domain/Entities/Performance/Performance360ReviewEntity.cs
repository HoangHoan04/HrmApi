using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Performance
{
    public class Performance360ReviewEntity : BaseEntity
    {
        public Guid CycleId { get; set; }
        public Guid SubjectEmployeeId { get; set; }
        public Guid ReviewerEmployeeId { get; set; }
        public string ReviewerType { get; set; } = Performance360ReviewerType.Self;
        public decimal Score { get; set; }
        public string? Comment { get; set; }
        public string Status { get; set; } = Performance360Status.Draft;

        public PerformanceReviewCycleEntity? Cycle { get; set; }
        public EmployeeEntity? SubjectEmployee { get; set; }
        public EmployeeEntity? ReviewerEmployee { get; set; }
    }
}
