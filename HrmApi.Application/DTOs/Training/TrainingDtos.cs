using HrmApi.Application.Common.Models;

namespace HrmApi.Application.DTOs.Training
{
    public class TrainingCourseDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Provider { get; set; }
        public decimal Hours { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class TrainingCourseCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Provider { get; set; }
        public decimal? Hours { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
    }

    public class TrainingCoursePagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Status { get; set; }
    }

    public class TrainingEnrollmentDto : BaseDto
    {
        public Guid CourseId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class TrainingEnrollmentCommandFields
    {
        public Guid? CourseId { get; set; }
        public Guid? EmployeeId { get; set; }
        public DateTime? EnrolledAt { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class TrainingEnrollmentPagedQuery : PagedRequest
    {
        public Guid? CourseId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Status { get; set; }
    }

    public class TrainingResultDto : BaseDto
    {
        public Guid EnrollmentId { get; set; }
        public string? CourseName { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? Score { get; set; }
        public string? Grade { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CertificateUrl { get; set; }
        public string? Note { get; set; }
    }

    public class TrainingResultCommandFields
    {
        public Guid? EnrollmentId { get; set; }
        public decimal? Score { get; set; }
        public string? Grade { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CertificateUrl { get; set; }
        public string? Note { get; set; }
    }

    public class TrainingResultPagedQuery : PagedRequest
    {
        public Guid? CourseId { get; set; }
        public Guid? EnrollmentId { get; set; }
    }

    public class CourseMaterialDto : BaseDto
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class CourseMaterialCommandFields
    {
        public Guid? CourseId { get; set; }
        public string? Name { get; set; }
        public string? FileUrl { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class CourseMaterialPagedQuery : PagedRequest
    {
        public Guid? CourseId { get; set; }
    }

    public class TrainingQuizDto : BaseDto
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public string Question { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string CorrectOption { get; set; } = string.Empty;
    }

    public class TrainingQuizCommandFields
    {
        public Guid? CourseId { get; set; }
        public string? Question { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectOption { get; set; }
    }

    public class TrainingQuizPagedQuery : PagedRequest
    {
        public Guid? CourseId { get; set; }
    }

    public class TrainingProgressDto
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int EnrolledCount { get; set; }
        public int CompletedCount { get; set; }
        public int DroppedCount { get; set; }
        public decimal CompletionPercent { get; set; }
    }

    public class TrainingProgressQuery
    {
        public Guid? CompanyId { get; set; }
        public Guid? CourseId { get; set; }
    }
}
