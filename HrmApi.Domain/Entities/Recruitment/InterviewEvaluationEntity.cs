using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class InterviewEvaluationEntity : BaseEntity
    {
        public Guid InterviewScheduleId { get; set; }
        public Guid InterviewerEmployeeId { get; set; }
        public Guid EvaluationCriteriaId { get; set; }
        public decimal Score { get; set; }
        public string? Comment { get; set; }

        public InterviewScheduleEntity? InterviewSchedule { get; set; }
        public EmployeeEntity? InterviewerEmployee { get; set; }
        public EvaluationCriteriaEntity? EvaluationCriteria { get; set; }
    }
}
