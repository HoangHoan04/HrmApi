using HrmApi.Application.DTOs.ReviewRenewal;
using HrmApi.Domain.Entities.Contract;

namespace HrmApi.Application.Mappings
{
    internal class ReviewRenewalMapper
    {
        public static ReviewRenewalDto ToDto(
            ReviewRenewalEntity entity,
            string? contractCode = null,
            string? employeeCode = null,
            string? employeeName = null,
            string? proposedContractTypeName = null,
            string? newContractCode = null,
            DateTime? contractEndDate = null)
        {
            return new ReviewRenewalDto
            {
                Id = entity.Id,
                ContractId = entity.ContractId,
                ContractCode = contractCode,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = employeeCode,
                EmployeeName = employeeName,
                ReviewDate = entity.ReviewDate,
                ReviewedBy = entity.ReviewedBy,
                PerformanceScore = entity.PerformanceScore,
                ReviewResult = entity.ReviewResult,
                ReviewComment = entity.ReviewComment,
                Recommendation = entity.Recommendation,
                ProposedContractTypeId = entity.ProposedContractTypeId,
                ProposedContractTypeName = proposedContractTypeName,
                ProposedStartDate = entity.ProposedStartDate,
                ProposedEndDate = entity.ProposedEndDate,
                ProposedBasicSalary = entity.ProposedBasicSalary,
                ApprovedBy = entity.ApprovedBy,
                ApprovedDate = entity.ApprovedDate,
                Status = entity.Status,
                NewContractId = entity.NewContractId,
                NewContractCode = newContractCode,
                Note = entity.Note,
                ContractEndDate = contractEndDate,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version
            };
        }

        public static void ApplyCommandFields(ReviewRenewalEntity entity, ReviewRenewalCommandFields fields)
        {
            if (fields.ReviewDate.HasValue)
            {
                entity.ReviewDate = fields.ReviewDate;
            }
            if (fields.ReviewedBy != null)
            {
                entity.ReviewedBy = string.IsNullOrWhiteSpace(fields.ReviewedBy) ? null : fields.ReviewedBy.Trim();
            }
            if (fields.PerformanceScore.HasValue)
            {
                entity.PerformanceScore = fields.PerformanceScore;
            }
            if (fields.ReviewResult != null)
            {
                entity.ReviewResult = string.IsNullOrWhiteSpace(fields.ReviewResult) ? null : fields.ReviewResult.Trim();
            }
            if (fields.ReviewComment != null)
            {
                entity.ReviewComment = string.IsNullOrWhiteSpace(fields.ReviewComment) ? null : fields.ReviewComment.Trim();
            }
            if (fields.Recommendation != null)
            {
                entity.Recommendation = string.IsNullOrWhiteSpace(fields.Recommendation) ? null : fields.Recommendation.Trim();
            }
            if (fields.ProposedContractTypeId.HasValue)
            {
                entity.ProposedContractTypeId = fields.ProposedContractTypeId == Guid.Empty ? null : fields.ProposedContractTypeId;
            }
            if (fields.ProposedStartDate.HasValue)
            {
                entity.ProposedStartDate = fields.ProposedStartDate;
            }
            if (fields.ProposedEndDate.HasValue || fields.ClearProposedEndDate == true)
            {
                entity.ProposedEndDate = fields.ClearProposedEndDate == true ? null : fields.ProposedEndDate;
            }
            if (fields.ProposedBasicSalary.HasValue)
            {
                entity.ProposedBasicSalary = fields.ProposedBasicSalary;
            }
            if (fields.Note != null)
            {
                entity.Note = string.IsNullOrWhiteSpace(fields.Note) ? null : fields.Note.Trim();
            }
            if (!string.IsNullOrWhiteSpace(fields.Status))
            {
                entity.Status = fields.Status.Trim();
            }
        }

        public static object ToLogObject(ReviewRenewalEntity entity)
        {
            return new
            {
                entity.Id,
                entity.ContractId,
                entity.EmployeeId,
                entity.Recommendation,
                entity.Status,
                entity.ProposedContractTypeId,
                entity.NewContractId,
                entity.ApprovedBy,
                entity.ApprovedDate
            };
        }
    }

    public class ReviewRenewalCommandFields
    {
        public Guid? ContractId { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ReviewedBy { get; set; }
        public decimal? PerformanceScore { get; set; }
        public string? ReviewResult { get; set; }
        public string? ReviewComment { get; set; }
        public string? Recommendation { get; set; }
        public Guid? ProposedContractTypeId { get; set; }
        public DateTime? ProposedStartDate { get; set; }
        public DateTime? ProposedEndDate { get; set; }
        public bool? ClearProposedEndDate { get; set; }
        public decimal? ProposedBasicSalary { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }
}
