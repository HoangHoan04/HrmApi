using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Training
{
    public class TrainingQuizEntity : BaseEntity
    {
        public Guid CourseId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string CorrectOption { get; set; } = "A";

        public TrainingCourseEntity? Course { get; set; }
    }
}
