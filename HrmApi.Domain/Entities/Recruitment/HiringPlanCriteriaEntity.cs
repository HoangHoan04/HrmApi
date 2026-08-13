using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Recruitment
{
    public class HiringPlanCriteriaEntity : BaseEntity
    {
        public Guid HiringPlanId { get; set; }
        public Guid EvaluationCriteriaId { get; set; }
        public decimal Weight { get; set; } = 1;
        public decimal MaxScore { get; set; } = 10;
        public int DisplayOrder { get; set; }

        public HiringPlanEntity? HiringPlan { get; set; }
        public EvaluationCriteriaEntity? EvaluationCriteria { get; set; }
    }
}
