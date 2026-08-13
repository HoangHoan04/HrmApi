using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class CandidateEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? CvUrl { get; set; }

        public Guid? HiringPlanId { get; set; }
        public Guid? RecruitmentRequestId { get; set; }
        public Guid? HiringSourceId { get; set; }
        public Guid? EmployeeId { get; set; }

        public string Status { get; set; } = CandidateStatus.New;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public HiringPlanEntity? HiringPlan { get; set; }
        public RecruitmentRequestEntity? RecruitmentRequest { get; set; }
        public HiringSourceEntity? HiringSource { get; set; }
        public EmployeeEntity? Employee { get; set; }

        public List<InterviewScheduleEntity> Interviews { get; set; } = [];
    }
}
