using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class EvaluationCriteriaEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal DefaultWeight { get; set; } = 1;
        public decimal MaxScore { get; set; } = 10;
        public Guid? CompanyId { get; set; }
        public bool IsActive { get; set; } = true;

        public CompanyEntity? Company { get; set; }
        public List<HiringPlanCriteriaEntity> PlanCriteria { get; set; } = [];
        public List<InterviewEvaluationEntity> Evaluations { get; set; } = [];
    }
}
