using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class InterviewInterviewerEntity : BaseEntity
    {
        public Guid InterviewScheduleId { get; set; }
        public Guid EmployeeId { get; set; }
        public bool IsPrimary { get; set; }

        public InterviewScheduleEntity? InterviewSchedule { get; set; }
        public EmployeeEntity? Employee { get; set; }
    }
}
