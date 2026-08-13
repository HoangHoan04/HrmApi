using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class HiringPlanEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public Guid? RecruitmentRequestId { get; set; }
        public Guid JobDescriptionId { get; set; }

        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }

        public int TargetQuantity { get; set; } = 1;
        public DateOnly? OpenFrom { get; set; }
        public DateOnly? OpenTo { get; set; }
        public string Status { get; set; } = HiringPlanStatus.Draft;
        public string? Note { get; set; }

        public RecruitmentRequestEntity? RecruitmentRequest { get; set; }
        public JobDescriptionEntity? JobDescription { get; set; }
        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public DepartmentEntity? Department { get; set; }
        public PartEntity? Part { get; set; }
        public PositionEntity? Position { get; set; }

        public List<HiringPlanCriteriaEntity> Criteria { get; set; } = [];
        public List<CandidateEntity> Candidates { get; set; } = [];
        public List<InterviewScheduleEntity> Interviews { get; set; } = [];
    }
}
