using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Timekeeping;

namespace HrmApi.Application.DTOs.Mobile
{
    public class MobileTeamMonthQuery
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class MobileTeamMemberMonthDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public List<MobileMonthDayDto> Days { get; set; } = [];
        public int OnTimeDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyDays { get; set; }
        public int LeaveDays { get; set; }
        public int AbsentDays { get; set; }
        public int IncompleteDays { get; set; }
        public int TotalWorkedMinutes { get; set; }
    }

    public class MobileTeamMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<MobileTeamMemberMonthDto> Members { get; set; } = [];
    }

    public class MobileTeamCalendarQuery
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
    }

    public class MobileDirectoryQuery : PagedRequest
    {
        public Guid? DepartmentId { get; set; }
    }

    public class MobileDirectoryEmployeeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
    }

    public class MobileManagerSummaryDto
    {
        public int PendingLeaveApprovals { get; set; }
        public int TeamLateThisMonth { get; set; }
        public int? ExpiringContractsCount { get; set; }
    }

    public class MobilePayslipHtmlRequest
    {
        public Guid? Id { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class MobilePayslipHtmlDto
    {
        public Guid SalaryId { get; set; }
        public string Html { get; set; } = string.Empty;
    }

    public class MobileAnnouncementDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMobileAnnouncementRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? CompanyId { get; set; }
    }

    public class MobileMyGoalsQuery
    {
        public Guid? CycleId { get; set; }
    }

    public class MobileQuizzesQuery
    {
        public Guid CourseId { get; set; }
    }

    public class MobileQuizQuestionDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
    }

    public class MobileSubmitQuizRequest
    {
        public Guid CourseId { get; set; }
        public List<MobileQuizAnswerDto> Answers { get; set; } = [];
        public string? Note { get; set; }
    }

    public class MobileQuizAnswerDto
    {
        public Guid QuizId { get; set; }
        public string SelectedOption { get; set; } = string.Empty;
    }

    public class MobileSubmitQuizResultDto
    {
        public Guid CourseId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public decimal ScorePercent { get; set; }
        public string? Note { get; set; }
    }

    public class MobileUpsert360Request
    {
        public Guid? Id { get; set; }
        public Guid? CycleId { get; set; }
        public Guid? SubjectEmployeeId { get; set; }
        public string? ReviewerType { get; set; }
        public decimal? Score { get; set; }
        public string? Comment { get; set; }
        public string? Status { get; set; }
    }
}
