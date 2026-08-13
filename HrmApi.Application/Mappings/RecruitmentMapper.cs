using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Recruitment;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.Mappings
{
    internal static class RecruitmentMapper
    {
        public static JobDescriptionDto ToDto(
            JobDescriptionEntity e,
            string? companyName = null,
            string? branchName = null,
            string? departmentName = null,
            string? partName = null,
            string? positionName = null,
            string? positionMasterName = null)
            => new()
            {
                Id = e.Id,
                Code = e.Code,
                Title = e.Title,
                Responsibilities = e.Responsibilities,
                Requirements = e.Requirements,
                Benefits = e.Benefits,
                CompanyId = e.CompanyId,
                CompanyName = companyName,
                BranchId = e.BranchId,
                BranchName = branchName,
                DepartmentId = e.DepartmentId,
                DepartmentName = departmentName,
                PartId = e.PartId,
                PartName = partName,
                PositionId = e.PositionId,
                PositionName = positionName,
                PositionMasterId = e.PositionMasterId,
                PositionMasterName = positionMasterName,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static void Apply(JobDescriptionEntity e, JobDescriptionCommandFields f)
        {
            if (!string.IsNullOrWhiteSpace(f.Code)) e.Code = f.Code.Trim();
            if (!string.IsNullOrWhiteSpace(f.Title)) e.Title = f.Title.Trim();
            if (f.Responsibilities != null) e.Responsibilities = string.IsNullOrWhiteSpace(f.Responsibilities) ? null : f.Responsibilities.Trim();
            if (f.Requirements != null) e.Requirements = string.IsNullOrWhiteSpace(f.Requirements) ? null : f.Requirements.Trim();
            if (f.Benefits != null) e.Benefits = string.IsNullOrWhiteSpace(f.Benefits) ? null : f.Benefits.Trim();
            if (f.CompanyId.HasValue && f.CompanyId != Guid.Empty) e.CompanyId = f.CompanyId.Value;
            if (f.BranchId.HasValue) e.BranchId = NullIfEmpty(f.BranchId);
            if (f.DepartmentId.HasValue) e.DepartmentId = NullIfEmpty(f.DepartmentId);
            if (f.PartId.HasValue) e.PartId = NullIfEmpty(f.PartId);
            if (f.PositionId.HasValue) e.PositionId = NullIfEmpty(f.PositionId);
            if (f.PositionMasterId.HasValue) e.PositionMasterId = NullIfEmpty(f.PositionMasterId);
            if (f.IsActive.HasValue) e.IsActive = f.IsActive.Value;
        }

        public static EvaluationCriteriaDto ToDto(EvaluationCriteriaEntity e, string? companyName = null)
            => new()
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                Category = e.Category,
                DefaultWeight = e.DefaultWeight,
                MaxScore = e.MaxScore,
                CompanyId = e.CompanyId,
                CompanyName = companyName,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static void Apply(EvaluationCriteriaEntity e, EvaluationCriteriaCommandFields f)
        {
            if (!string.IsNullOrWhiteSpace(f.Code)) e.Code = f.Code.Trim();
            if (!string.IsNullOrWhiteSpace(f.Name)) e.Name = f.Name.Trim();
            if (f.Description != null) e.Description = string.IsNullOrWhiteSpace(f.Description) ? null : f.Description.Trim();
            if (f.Category != null) e.Category = string.IsNullOrWhiteSpace(f.Category) ? null : f.Category.Trim();
            if (f.DefaultWeight.HasValue) e.DefaultWeight = f.DefaultWeight.Value;
            if (f.MaxScore.HasValue) e.MaxScore = f.MaxScore.Value;
            if (f.CompanyId.HasValue) e.CompanyId = NullIfEmpty(f.CompanyId);
            if (f.IsActive.HasValue) e.IsActive = f.IsActive.Value;
        }

        public static HiringSourceDto ToDto(HiringSourceEntity e)
            => new()
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                ChannelType = e.ChannelType,
                ContactEmail = e.ContactEmail,
                DisplayOrder = e.DisplayOrder,
                IsSystem = e.IsSystem,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static void Apply(HiringSourceEntity e, HiringSourceCommandFields f)
        {
            if (!string.IsNullOrWhiteSpace(f.Code) && !e.IsSystem) e.Code = f.Code.Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(f.Name)) e.Name = f.Name.Trim();
            if (f.Description != null) e.Description = string.IsNullOrWhiteSpace(f.Description) ? null : f.Description.Trim();
            if (!string.IsNullOrWhiteSpace(f.ChannelType)) e.ChannelType = f.ChannelType.Trim().ToUpperInvariant();
            if (f.ContactEmail != null) e.ContactEmail = string.IsNullOrWhiteSpace(f.ContactEmail) ? null : f.ContactEmail.Trim();
            if (f.DisplayOrder.HasValue) e.DisplayOrder = f.DisplayOrder.Value;
            if (f.IsActive.HasValue) e.IsActive = f.IsActive.Value;
        }

        public static RecruitmentRequestDto ToDto(
            RecruitmentRequestEntity e,
            string? companyName = null,
            string? branchName = null,
            string? departmentName = null,
            string? partName = null,
            string? positionName = null,
            string? jdTitle = null,
            string? requestedByName = null,
            string? approvedByName = null)
            => new()
            {
                Id = e.Id,
                Code = e.Code,
                Title = e.Title,
                RequestLevel = e.RequestLevel,
                CompanyId = e.CompanyId,
                CompanyName = companyName,
                BranchId = e.BranchId,
                BranchName = branchName,
                DepartmentId = e.DepartmentId,
                DepartmentName = departmentName,
                PartId = e.PartId,
                PartName = partName,
                PositionId = e.PositionId,
                PositionName = positionName,
                JobDescriptionId = e.JobDescriptionId,
                JobDescriptionTitle = jdTitle,
                Quantity = e.Quantity,
                Reason = e.Reason,
                ExpectedStartDate = e.ExpectedStartDate,
                Status = e.Status,
                RequestedByEmployeeId = e.RequestedByEmployeeId,
                RequestedByEmployeeName = requestedByName,
                ApprovedByEmployeeId = e.ApprovedByEmployeeId,
                ApprovedByEmployeeName = approvedByName,
                ApprovedAt = e.ApprovedAt,
                ApprovalNote = e.ApprovalNote,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static void Apply(RecruitmentRequestEntity e, RecruitmentRequestCommandFields f)
        {
            if (!string.IsNullOrWhiteSpace(f.Code)) e.Code = f.Code.Trim();
            if (!string.IsNullOrWhiteSpace(f.Title)) e.Title = f.Title.Trim();
            if (!string.IsNullOrWhiteSpace(f.RequestLevel)) e.RequestLevel = f.RequestLevel.Trim().ToUpperInvariant();
            if (f.CompanyId.HasValue && f.CompanyId != Guid.Empty) e.CompanyId = f.CompanyId.Value;
            if (f.BranchId.HasValue) e.BranchId = NullIfEmpty(f.BranchId);
            if (f.DepartmentId.HasValue) e.DepartmentId = NullIfEmpty(f.DepartmentId);
            if (f.PartId.HasValue) e.PartId = NullIfEmpty(f.PartId);
            if (f.PositionId.HasValue) e.PositionId = NullIfEmpty(f.PositionId);
            if (f.JobDescriptionId.HasValue) e.JobDescriptionId = NullIfEmpty(f.JobDescriptionId);
            if (f.Quantity.HasValue) e.Quantity = f.Quantity.Value;
            if (f.Reason != null) e.Reason = string.IsNullOrWhiteSpace(f.Reason) ? null : f.Reason.Trim();
            if (f.ExpectedStartDate.HasValue) e.ExpectedStartDate = f.ExpectedStartDate;
            if (f.RequestedByEmployeeId.HasValue) e.RequestedByEmployeeId = NullIfEmpty(f.RequestedByEmployeeId);
        }

        public static HiringPlanDto ToDto(
            HiringPlanEntity e,
            string? requestCode = null,
            string? jdTitle = null,
            string? companyName = null,
            string? branchName = null,
            string? departmentName = null,
            string? partName = null,
            string? positionName = null,
            List<PlanCriteriaDto>? criteria = null)
            => new()
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                RecruitmentRequestId = e.RecruitmentRequestId,
                RecruitmentRequestCode = requestCode,
                JobDescriptionId = e.JobDescriptionId,
                JobDescriptionTitle = jdTitle,
                CompanyId = e.CompanyId,
                CompanyName = companyName,
                BranchId = e.BranchId,
                BranchName = branchName,
                DepartmentId = e.DepartmentId,
                DepartmentName = departmentName,
                PartId = e.PartId,
                PartName = partName,
                PositionId = e.PositionId,
                PositionName = positionName,
                TargetQuantity = e.TargetQuantity,
                OpenFrom = e.OpenFrom,
                OpenTo = e.OpenTo,
                Status = e.Status,
                Note = e.Note,
                Criteria = criteria ?? [],
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static void Apply(HiringPlanEntity e, HiringPlanCommandFields f)
        {
            if (!string.IsNullOrWhiteSpace(f.Code)) e.Code = f.Code.Trim();
            if (!string.IsNullOrWhiteSpace(f.Name)) e.Name = f.Name.Trim();
            if (f.RecruitmentRequestId.HasValue) e.RecruitmentRequestId = NullIfEmpty(f.RecruitmentRequestId);
            if (f.JobDescriptionId.HasValue && f.JobDescriptionId != Guid.Empty) e.JobDescriptionId = f.JobDescriptionId.Value;
            if (f.CompanyId.HasValue && f.CompanyId != Guid.Empty) e.CompanyId = f.CompanyId.Value;
            if (f.BranchId.HasValue) e.BranchId = NullIfEmpty(f.BranchId);
            if (f.DepartmentId.HasValue) e.DepartmentId = NullIfEmpty(f.DepartmentId);
            if (f.PartId.HasValue) e.PartId = NullIfEmpty(f.PartId);
            if (f.PositionId.HasValue) e.PositionId = NullIfEmpty(f.PositionId);
            if (f.TargetQuantity.HasValue) e.TargetQuantity = f.TargetQuantity.Value;
            if (f.OpenFrom.HasValue) e.OpenFrom = f.OpenFrom;
            if (f.OpenTo.HasValue) e.OpenTo = f.OpenTo;
            if (!string.IsNullOrWhiteSpace(f.Status)) e.Status = f.Status.Trim().ToUpperInvariant();
            if (f.Note != null) e.Note = string.IsNullOrWhiteSpace(f.Note) ? null : f.Note.Trim();
        }

        public static PlanCriteriaDto ToDto(HiringPlanCriteriaEntity e, string? code = null, string? name = null)
            => new()
            {
                Id = e.Id,
                HiringPlanId = e.HiringPlanId,
                EvaluationCriteriaId = e.EvaluationCriteriaId,
                EvaluationCriteriaCode = code,
                EvaluationCriteriaName = name,
                Weight = e.Weight,
                MaxScore = e.MaxScore,
                DisplayOrder = e.DisplayOrder,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static CandidateDto ToDto(
            CandidateEntity e,
            string? planName = null,
            string? requestCode = null,
            string? sourceName = null,
            string? employeeName = null)
            => new()
            {
                Id = e.Id,
                Code = e.Code,
                FullName = e.FullName,
                Email = e.Email,
                Phone = e.Phone,
                Gender = e.Gender,
                DateOfBirth = e.DateOfBirth,
                CvUrl = e.CvUrl,
                HiringPlanId = e.HiringPlanId,
                HiringPlanName = planName,
                RecruitmentRequestId = e.RecruitmentRequestId,
                RecruitmentRequestCode = requestCode,
                HiringSourceId = e.HiringSourceId,
                HiringSourceName = sourceName,
                EmployeeId = e.EmployeeId,
                EmployeeName = employeeName,
                Status = e.Status,
                AppliedAt = e.AppliedAt,
                Notes = e.Notes,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static void Apply(CandidateEntity e, CandidateCommandFields f)
        {
            if (!string.IsNullOrWhiteSpace(f.Code)) e.Code = f.Code.Trim();
            if (!string.IsNullOrWhiteSpace(f.FullName)) e.FullName = f.FullName.Trim();
            if (f.Email != null) e.Email = string.IsNullOrWhiteSpace(f.Email) ? null : f.Email.Trim();
            if (f.Phone != null) e.Phone = string.IsNullOrWhiteSpace(f.Phone) ? null : f.Phone.Trim();
            if (f.Gender != null) e.Gender = string.IsNullOrWhiteSpace(f.Gender) ? null : f.Gender.Trim();
            if (f.DateOfBirth.HasValue) e.DateOfBirth = f.DateOfBirth;
            if (f.CvUrl != null) e.CvUrl = string.IsNullOrWhiteSpace(f.CvUrl) ? null : f.CvUrl.Trim();
            if (f.HiringPlanId.HasValue) e.HiringPlanId = NullIfEmpty(f.HiringPlanId);
            if (f.RecruitmentRequestId.HasValue) e.RecruitmentRequestId = NullIfEmpty(f.RecruitmentRequestId);
            if (f.HiringSourceId.HasValue) e.HiringSourceId = NullIfEmpty(f.HiringSourceId);
            if (f.EmployeeId.HasValue) e.EmployeeId = NullIfEmpty(f.EmployeeId);
            if (!string.IsNullOrWhiteSpace(f.Status)) e.Status = f.Status.Trim().ToUpperInvariant();
            if (f.AppliedAt.HasValue) e.AppliedAt = f.AppliedAt.Value;
            if (f.Notes != null) e.Notes = string.IsNullOrWhiteSpace(f.Notes) ? null : f.Notes.Trim();
        }

        public static InterviewScheduleDto ToDto(
            InterviewScheduleEntity e,
            string? candidateCode = null,
            string? candidateName = null,
            string? planName = null,
            List<InterviewerDto>? interviewers = null,
            List<EvaluationDto>? evaluations = null)
            => new()
            {
                Id = e.Id,
                CandidateId = e.CandidateId,
                CandidateCode = candidateCode,
                CandidateName = candidateName,
                HiringPlanId = e.HiringPlanId,
                HiringPlanName = planName,
                Round = e.Round,
                StartAt = e.StartAt,
                EndAt = e.EndAt,
                Location = e.Location,
                MeetingUrl = e.MeetingUrl,
                Status = e.Status,
                Notes = e.Notes,
                Interviewers = interviewers ?? [],
                Evaluations = evaluations ?? [],
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static void Apply(InterviewScheduleEntity e, InterviewScheduleCommandFields f)
        {
            if (f.CandidateId.HasValue && f.CandidateId != Guid.Empty) e.CandidateId = f.CandidateId.Value;
            if (f.HiringPlanId.HasValue) e.HiringPlanId = NullIfEmpty(f.HiringPlanId);
            if (f.Round.HasValue) e.Round = f.Round.Value;
            if (f.StartAt.HasValue) e.StartAt = f.StartAt.Value;
            if (f.EndAt.HasValue) e.EndAt = f.EndAt.Value;
            if (f.Location != null) e.Location = string.IsNullOrWhiteSpace(f.Location) ? null : f.Location.Trim();
            if (f.MeetingUrl != null) e.MeetingUrl = string.IsNullOrWhiteSpace(f.MeetingUrl) ? null : f.MeetingUrl.Trim();
            if (!string.IsNullOrWhiteSpace(f.Status)) e.Status = f.Status.Trim().ToUpperInvariant();
            if (f.Notes != null) e.Notes = string.IsNullOrWhiteSpace(f.Notes) ? null : f.Notes.Trim();
        }

        public static InterviewerDto ToDto(InterviewInterviewerEntity e, string? code = null, string? name = null)
            => new()
            {
                Id = e.Id,
                InterviewScheduleId = e.InterviewScheduleId,
                EmployeeId = e.EmployeeId,
                EmployeeCode = code,
                EmployeeName = name,
                IsPrimary = e.IsPrimary,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static EvaluationDto ToDto(
            InterviewEvaluationEntity e,
            string? interviewerName = null,
            string? criteriaCode = null,
            string? criteriaName = null)
            => new()
            {
                Id = e.Id,
                InterviewScheduleId = e.InterviewScheduleId,
                InterviewerEmployeeId = e.InterviewerEmployeeId,
                InterviewerEmployeeName = interviewerName,
                EvaluationCriteriaId = e.EvaluationCriteriaId,
                EvaluationCriteriaCode = criteriaCode,
                EvaluationCriteriaName = criteriaName,
                Score = e.Score,
                Comment = e.Comment,
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Version = e.Version,
            };

        public static Guid? NullIfEmpty(Guid? value)
            => value.HasValue && value.Value != Guid.Empty ? value : null;

        public static bool IsActiveEmployeeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return true;
            string s = status.ToUpperInvariant();
            return !s.Contains("INACTIVE") && !s.Contains("RESIGNED");
        }

        public static bool IsValidCandidateStatus(string status)
            => status is CandidateStatus.New or CandidateStatus.Screening or CandidateStatus.Interview
                or CandidateStatus.Waitlist or CandidateStatus.Offer or CandidateStatus.Hired
                or CandidateStatus.Rejected or CandidateStatus.Withdrawn;
    }
}
