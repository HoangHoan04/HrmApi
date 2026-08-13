using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class InterviewScheduleEntity : BaseEntity
    {
        public Guid CandidateId { get; set; }
        public Guid? HiringPlanId { get; set; }
        public int Round { get; set; } = 1;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }
        public string Status { get; set; } = InterviewStatus.Scheduled;
        public string? Notes { get; set; }

        public CandidateEntity? Candidate { get; set; }
        public HiringPlanEntity? HiringPlan { get; set; }
        public List<InterviewInterviewerEntity> Interviewers { get; set; } = [];
        public List<InterviewEvaluationEntity> Evaluations { get; set; } = [];
    }
}
