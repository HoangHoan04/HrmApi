using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class JobDescriptionEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Responsibilities { get; set; }
        public string? Requirements { get; set; }
        public string? Benefits { get; set; }

        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? PositionMasterId { get; set; }

        public bool IsActive { get; set; } = true;

        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public DepartmentEntity? Department { get; set; }
        public PartEntity? Part { get; set; }
        public PositionEntity? Position { get; set; }
        public PositionMasterEntity? PositionMaster { get; set; }

        public List<RecruitmentRequestEntity> RecruitmentRequests { get; set; } = [];
        public List<HiringPlanEntity> HiringPlans { get; set; } = [];
    }
}
