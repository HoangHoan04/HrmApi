using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Training
{
    public class TrainingCourseEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Provider { get; set; }
        public decimal Hours { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string Status { get; set; } = TrainingCourseStatus.Draft;
        public string? Description { get; set; }

        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public List<TrainingEnrollmentEntity> Enrollments { get; set; } = [];
        public List<TrainingCourseMaterialEntity> Materials { get; set; } = [];
        public List<TrainingQuizEntity> Quizzes { get; set; } = [];
    }
}
