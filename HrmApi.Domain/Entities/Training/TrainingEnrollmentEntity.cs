using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Training
{
    public class TrainingEnrollmentEntity : BaseEntity
    {
        public Guid CourseId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = TrainingEnrollmentStatus.Enrolled;
        public string? Note { get; set; }

        public TrainingCourseEntity? Course { get; set; }
        public EmployeeEntity? Employee { get; set; }
        public TrainingResultEntity? Result { get; set; }
    }
}
