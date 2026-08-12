using HrmApi.Application.DTOs.TransferEmployee;
using HrmApi.Domain.Entities.EmployeeMovement;

namespace HrmApi.Application.Mappings
{
    internal class TransferEmployeeMapper
    {
        public static TransferEmployeeDto ToDto(
            TransferEmployeeEntity entity,
            string? employeeCode = null,
            string? employeeName = null,
            List<TransferEmployeePositionDto>? details = null)
        {
            return new TransferEmployeeDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = employeeCode,
                EmployeeName = employeeName,
                Code = entity.Code,
                TransferType = entity.TransferType,
                RequestDate = entity.RequestDate,
                EffectiveDate = entity.EffectiveDate,
                ExpectedEndDate = entity.ExpectedEndDate,
                ActualEndDate = entity.ActualEndDate,
                Reason = entity.Reason,
                DecisionNumber = entity.DecisionNumber,
                DecisionDate = entity.DecisionDate,
                DecisionFileUrl = entity.DecisionFileUrl,
                ApprovedBy = entity.ApprovedBy,
                ApprovedDate = entity.ApprovedDate,
                Status = entity.Status,
                Note = entity.Note,
                Details = details ?? [],
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version
            };
        }

        public static TransferEmployeePositionDto ToDetailDto(
            TransferEmployeePositionEntity entity,
            string? oldCompanyName = null,
            string? newCompanyName = null,
            string? oldBranchName = null,
            string? newBranchName = null,
            string? oldDepartmentName = null,
            string? newDepartmentName = null,
            string? oldPartName = null,
            string? newPartName = null,
            string? oldPositionName = null,
            string? newPositionName = null)
        {
            return new TransferEmployeePositionDto
            {
                Id = entity.Id,
                TransferEmployeeId = entity.TransferEmployeeId,
                EmployeeId = entity.EmployeeId,
                EffectiveDate = entity.EffectiveDate,
                OldCompanyId = entity.OldCompanyId,
                OldCompanyName = oldCompanyName,
                NewCompanyId = entity.NewCompanyId,
                NewCompanyName = newCompanyName,
                OldBranchId = entity.OldBranchId,
                OldBranchName = oldBranchName,
                NewBranchId = entity.NewBranchId,
                NewBranchName = newBranchName,
                OldDepartmentId = entity.OldDepartmentId,
                OldDepartmentName = oldDepartmentName,
                NewDepartmentId = entity.NewDepartmentId,
                NewDepartmentName = newDepartmentName,
                OldPartId = entity.OldPartId,
                OldPartName = oldPartName,
                NewPartId = entity.NewPartId,
                NewPartName = newPartName,
                OldPositionId = entity.OldPositionId,
                OldPositionName = oldPositionName,
                NewPositionId = entity.NewPositionId,
                NewPositionName = newPositionName,
                ChangeType = entity.ChangeType,
                Note = entity.Note,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version
            };
        }

        public static void ApplyHeaderFields(TransferEmployeeEntity entity, TransferEmployeeCommandFields fields)
        {
            if (!string.IsNullOrWhiteSpace(fields.Code))
            {
                entity.Code = fields.Code.Trim();
            }
            if (!string.IsNullOrWhiteSpace(fields.TransferType))
            {
                entity.TransferType = fields.TransferType.Trim();
            }
            if (fields.RequestDate.HasValue)
            {
                entity.RequestDate = fields.RequestDate;
            }
            if (fields.EffectiveDate.HasValue)
            {
                entity.EffectiveDate = fields.EffectiveDate.Value;
            }
            if (fields.ExpectedEndDate.HasValue || fields.ClearExpectedEndDate == true)
            {
                entity.ExpectedEndDate = fields.ClearExpectedEndDate == true ? null : fields.ExpectedEndDate;
            }
            if (fields.Reason != null)
            {
                entity.Reason = string.IsNullOrWhiteSpace(fields.Reason) ? null : fields.Reason.Trim();
            }
            if (fields.DecisionNumber != null)
            {
                entity.DecisionNumber = string.IsNullOrWhiteSpace(fields.DecisionNumber) ? null : fields.DecisionNumber.Trim();
            }
            if (fields.DecisionDate.HasValue || fields.ClearDecisionDate == true)
            {
                entity.DecisionDate = fields.ClearDecisionDate == true ? null : fields.DecisionDate;
            }
            if (fields.DecisionFileUrl != null)
            {
                entity.DecisionFileUrl = string.IsNullOrWhiteSpace(fields.DecisionFileUrl) ? null : fields.DecisionFileUrl.Trim();
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

        public static object ToLogObject(TransferEmployeeEntity entity)
        {
            return new
            {
                entity.Id,
                entity.EmployeeId,
                entity.Code,
                entity.TransferType,
                entity.EffectiveDate,
                entity.ExpectedEndDate,
                entity.Status,
                entity.ApprovedBy,
                entity.ApprovedDate,
                entity.ActualEndDate
            };
        }
    }

    public class TransferEmployeeCommandFields
    {
        public Guid? EmployeeId { get; set; }
        public string? Code { get; set; }
        public string? TransferType { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public bool? ClearExpectedEndDate { get; set; }
        public string? Reason { get; set; }
        public string? DecisionNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public bool? ClearDecisionDate { get; set; }
        public string? DecisionFileUrl { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public List<TransferEmployeePositionInputDto>? Details { get; set; }
    }
}
