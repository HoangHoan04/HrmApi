using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Training
{
    public class TrainingCourseMaterialEntity : BaseEntity
    {
        public Guid CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        public TrainingCourseEntity? Course { get; set; }
    }
}
