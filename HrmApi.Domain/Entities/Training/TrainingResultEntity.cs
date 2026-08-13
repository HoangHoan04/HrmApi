using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Training
{
    public class TrainingResultEntity : BaseEntity
    {
        public Guid EnrollmentId { get; set; }
        public decimal? Score { get; set; }
        public string? Grade { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CertificateUrl { get; set; }
        public string? Note { get; set; }

        public TrainingEnrollmentEntity? Enrollment { get; set; }
    }
}
