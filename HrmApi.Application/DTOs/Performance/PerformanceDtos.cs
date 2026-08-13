using HrmApi.Application.Common.Models;

namespace HrmApi.Application.DTOs.Performance
{
    public class PerformanceReviewCycleDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateOnly PeriodFrom { get; set; }
        public DateOnly PeriodTo { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class PerformanceReviewCycleCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public DateOnly? PeriodFrom { get; set; }
        public DateOnly? PeriodTo { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class PerformanceReviewCyclePagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Status { get; set; }
    }

    public class KpiGoalDto : BaseDto
    {
        public Guid CycleId { get; set; }
        public string? CycleName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public string? Unit { get; set; }
        public decimal Weight { get; set; }
    }

    public class KpiGoalCommandFields
    {
        public Guid? CycleId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Title { get; set; }
        public decimal? TargetValue { get; set; }
        public string? Unit { get; set; }
        public decimal? Weight { get; set; }
    }

    public class KpiGoalPagedQuery : PagedRequest
    {
        public Guid? CycleId { get; set; }
        public Guid? EmployeeId { get; set; }
    }

    public class KpiResultDto : BaseDto
    {
        public Guid GoalId { get; set; }
        public string? GoalTitle { get; set; }
        public decimal ActualValue { get; set; }
        public decimal Score { get; set; }
        public string? Comment { get; set; }
        public Guid? RatedByEmployeeId { get; set; }
        public string? RatedByEmployeeName { get; set; }
        public DateTime? RatedAt { get; set; }
    }

    public class KpiResultCommandFields
    {
        public Guid? GoalId { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal? Score { get; set; }
        public string? Comment { get; set; }
        public Guid? RatedByEmployeeId { get; set; }
        public DateTime? RatedAt { get; set; }
    }

    public class KpiResultPagedQuery : PagedRequest
    {
        public Guid? GoalId { get; set; }
        public Guid? CycleId { get; set; }
    }

    public class CompetencyFrameworkDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CompetencyFrameworkCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CompetencyFrameworkPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ScoreBandDto
    {
        public string Band { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DeptScoreDto
    {
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public decimal AvgScore { get; set; }
        public int Count { get; set; }
    }

    public class PerformanceDashboardDto
    {
        public Guid? CycleId { get; set; }
        public int GoalCount { get; set; }
        public int ResultCount { get; set; }
        public decimal AvgScore { get; set; }
        public decimal MetTargetPercent { get; set; }
        public List<ScoreBandDto> ScoreBands { get; set; } = [];
        public List<DeptScoreDto> DeptScores { get; set; } = [];
    }

    public class Performance360ReviewDto : BaseDto
    {
        public Guid CycleId { get; set; }
        public string? CycleName { get; set; }
        public Guid SubjectEmployeeId { get; set; }
        public string? SubjectEmployeeCode { get; set; }
        public string? SubjectEmployeeName { get; set; }
        public Guid ReviewerEmployeeId { get; set; }
        public string? ReviewerEmployeeCode { get; set; }
        public string? ReviewerEmployeeName { get; set; }
        public string ReviewerType { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string? Comment { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class Performance360ReviewCommandFields
    {
        public Guid? CycleId { get; set; }
        public Guid? SubjectEmployeeId { get; set; }
        public Guid? ReviewerEmployeeId { get; set; }
        public string? ReviewerType { get; set; }
        public decimal? Score { get; set; }
        public string? Comment { get; set; }
        public string? Status { get; set; }
    }

    public class Performance360ReviewPagedQuery : PagedRequest
    {
        public Guid? CycleId { get; set; }
        public Guid? SubjectEmployeeId { get; set; }
        public string? ReviewerType { get; set; }
    }
}
