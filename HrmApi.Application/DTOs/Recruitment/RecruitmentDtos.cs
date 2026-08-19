using HrmApi.Application.Common.Models;

namespace HrmApi.Application.DTOs.Recruitment
{
    public class HeadcountNodeDto
    {
        public string NodeType { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? PlannedLimit { get; set; }
        public int ActualCount { get; set; }
        public int? Vacancy { get; set; }
        public int Level { get; set; }
        public int Depth { get; set; }
        public bool IsEditable { get; set; } = true;
        public bool IsAggregated { get; set; }
        public int ChildBranchCount { get; set; }
    }

    public class HeadcountTreeQuery
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class UpsertHeadcountRowFields
    {
        public string NodeType { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public int? PlannedLimit { get; set; }
    }

    public class JobDescriptionDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Responsibilities { get; set; }
        public string? Requirements { get; set; }
        public string? Benefits { get; set; }
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PartId { get; set; }
        public string? PartName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? PositionMasterId { get; set; }
        public string? PositionMasterName { get; set; }
        public bool IsActive { get; set; }
    }

    public class JobDescriptionCommandFields
    {
        public string? Code { get; set; }
        public string? Title { get; set; }
        public string? Responsibilities { get; set; }
        public string? Requirements { get; set; }
        public string? Benefits { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? PositionMasterId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class JobDescriptionPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class EvaluationCriteriaDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal DefaultWeight { get; set; }
        public decimal MaxScore { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
    }

    public class EvaluationCriteriaCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal? DefaultWeight { get; set; }
        public decimal? MaxScore { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class EvaluationCriteriaPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public string? Category { get; set; }
        public bool? IsActive { get; set; }
    }

    public class HiringSourceDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ChannelType { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
    }

    public class HiringSourceCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ChannelType { get; set; }
        public string? ContactEmail { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RecruitmentRequestDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string RequestLevel { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PartId { get; set; }
        public string? PartName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? JobDescriptionId { get; set; }
        public string? JobDescriptionTitle { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public DateOnly? ExpectedStartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? RequestedByEmployeeId { get; set; }
        public string? RequestedByEmployeeName { get; set; }
        public Guid? ApprovedByEmployeeId { get; set; }
        public string? ApprovedByEmployeeName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovalNote { get; set; }
    }

    public class RecruitmentRequestCommandFields
    {
        public string? Code { get; set; }
        public string? Title { get; set; }
        public string? RequestLevel { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? JobDescriptionId { get; set; }
        public int? Quantity { get; set; }
        public string? Reason { get; set; }
        public DateOnly? ExpectedStartDate { get; set; }
        public Guid? RequestedByEmployeeId { get; set; }
    }

    public class RecruitmentRequestPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public string? Status { get; set; }
        public string? RequestLevel { get; set; }
    }

    public class RecruitmentRequestDecisionFields
    {
        public Guid Id { get; set; }
        public string? ApprovalNote { get; set; }
    }

    public class PlanCriteriaDto : BaseDto
    {
        public Guid HiringPlanId { get; set; }
        public Guid EvaluationCriteriaId { get; set; }
        public string? EvaluationCriteriaCode { get; set; }
        public string? EvaluationCriteriaName { get; set; }
        public decimal Weight { get; set; }
        public decimal MaxScore { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class PlanCriteriaInputDto
    {
        public Guid EvaluationCriteriaId { get; set; }
        public decimal? Weight { get; set; }
        public decimal? MaxScore { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class HiringPlanDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? RecruitmentRequestId { get; set; }
        public string? RecruitmentRequestCode { get; set; }
        public Guid JobDescriptionId { get; set; }
        public string? JobDescriptionTitle { get; set; }
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PartId { get; set; }
        public string? PartName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public int TargetQuantity { get; set; }
        public DateOnly? OpenFrom { get; set; }
        public DateOnly? OpenTo { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public List<PlanCriteriaDto> Criteria { get; set; } = [];
    }

    public class HiringPlanCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? RecruitmentRequestId { get; set; }
        public Guid? JobDescriptionId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public int? TargetQuantity { get; set; }
        public DateOnly? OpenFrom { get; set; }
        public DateOnly? OpenTo { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class HiringPlanPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RecruitmentRequestId { get; set; }
        public Guid? JobDescriptionId { get; set; }
        public string? Status { get; set; }
    }

    public class SetHiringPlanCriteriaFields
    {
        public Guid HiringPlanId { get; set; }
        public List<PlanCriteriaInputDto> Criteria { get; set; } = [];
    }

    public class CandidateDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? CvUrl { get; set; }
        public Guid? HiringPlanId { get; set; }
        public string? HiringPlanName { get; set; }
        public Guid? RecruitmentRequestId { get; set; }
        public string? RecruitmentRequestCode { get; set; }
        public Guid? HiringSourceId { get; set; }
        public string? HiringSourceName { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CandidateCommandFields
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? CvUrl { get; set; }
        public Guid? HiringPlanId { get; set; }
        public Guid? RecruitmentRequestId { get; set; }
        public Guid? HiringSourceId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Status { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CandidatePagedQuery : PagedRequest
    {
        public Guid? HiringPlanId { get; set; }
        public Guid? RecruitmentRequestId { get; set; }
        public Guid? HiringSourceId { get; set; }
        public string? Status { get; set; }
        public List<string>? Statuses { get; set; }
    }

    public class ChangeCandidateStatusFields
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CandidateStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CandidateStatusSummaryQuery
    {
        public Guid? HiringPlanId { get; set; }
        public Guid? RecruitmentRequestId { get; set; }
    }

    public class CandidateHirePrefillDto
    {
        public Guid CandidateId { get; set; }
        public string CandidateCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? CvUrl { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? HiringPlanId { get; set; }
        public string? HiringPlanName { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public string SuggestedEmployeeCode { get; set; } = string.Empty;
    }

    public class LinkCandidateEmployeeFields
    {
        public Guid CandidateId { get; set; }
        public Guid EmployeeId { get; set; }
        public bool SetStatusHired { get; set; } = true;
    }

    public class InterviewerDto : BaseDto
    {
        public Guid InterviewScheduleId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class InterviewerInputDto
    {
        public Guid EmployeeId { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class EvaluationDto : BaseDto
    {
        public Guid InterviewScheduleId { get; set; }
        public Guid InterviewerEmployeeId { get; set; }
        public string? InterviewerEmployeeName { get; set; }
        public Guid EvaluationCriteriaId { get; set; }
        public string? EvaluationCriteriaCode { get; set; }
        public string? EvaluationCriteriaName { get; set; }
        public decimal Score { get; set; }
        public string? Comment { get; set; }
    }

    public class EvaluationInputDto
    {
        public Guid? Id { get; set; }
        public Guid EvaluationCriteriaId { get; set; }
        public decimal Score { get; set; }
        public string? Comment { get; set; }
    }

    public class InterviewScheduleDto : BaseDto
    {
        public Guid CandidateId { get; set; }
        public string? CandidateCode { get; set; }
        public string? CandidateName { get; set; }
        public Guid? HiringPlanId { get; set; }
        public string? HiringPlanName { get; set; }
        public int Round { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<InterviewerDto> Interviewers { get; set; } = [];
        public List<EvaluationDto> Evaluations { get; set; } = [];
    }

    public class InterviewScheduleCommandFields
    {
        public Guid? CandidateId { get; set; }
        public Guid? HiringPlanId { get; set; }
        public int? Round { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class InterviewSchedulePagedQuery : PagedRequest
    {
        public Guid? CandidateId { get; set; }
        public Guid? HiringPlanId { get; set; }
        public string? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class InterviewCalendarRangeQuery
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid? HiringPlanId { get; set; }
        public Guid? CandidateId { get; set; }
    }

    public class SetInterviewersFields
    {
        public Guid InterviewScheduleId { get; set; }
        public List<InterviewerInputDto> Interviewers { get; set; } = [];
    }

    public class UpsertInterviewEvaluationsFields
    {
        public Guid InterviewScheduleId { get; set; }
        public Guid InterviewerEmployeeId { get; set; }
        public List<EvaluationInputDto> Evaluations { get; set; } = [];
    }

    public class IdRequest
    {
        public Guid Id { get; set; }
    }
}
