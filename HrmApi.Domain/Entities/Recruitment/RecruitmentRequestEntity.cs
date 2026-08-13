using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class RecruitmentRequestEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string RequestLevel { get; set; } = RecruitmentRequestLevel.Department;

        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? JobDescriptionId { get; set; }

        public int Quantity { get; set; } = 1;
        public string? Reason { get; set; }
        public DateOnly? ExpectedStartDate { get; set; }

        public string Status { get; set; } = RecruitmentRequestStatus.Draft;
        public Guid? RequestedByEmployeeId { get; set; }
        public Guid? ApprovedByEmployeeId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovalNote { get; set; }

        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public DepartmentEntity? Department { get; set; }
        public PartEntity? Part { get; set; }
        public PositionEntity? Position { get; set; }
        public JobDescriptionEntity? JobDescription { get; set; }
        public EmployeeEntity? RequestedByEmployee { get; set; }
        public EmployeeEntity? ApprovedByEmployee { get; set; }

        public List<HiringPlanEntity> HiringPlans { get; set; } = [];
        public List<CandidateEntity> Candidates { get; set; } = [];
    }
}
