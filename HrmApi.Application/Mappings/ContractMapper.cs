using HrmApi.Application.DTOs.Contract;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.Mappings
{
    internal class ContractMapper
    {
        public static ContractDto ToDto(
            ContractEntity entity,
            string? employeeCode = null,
            string? employeeName = null,
            string? contractTypeCode = null,
            string? contractTypeName = null,
            string? companyName = null,
            string? branchName = null,
            string? departmentName = null,
            string? positionName = null,
            string? previousContractCode = null,
            int? notifyBeforeExpiryDays = null)
        {
            DateTime today = DateTime.UtcNow.Date;
            int? daysUntilExpiry = entity.EndDate.HasValue
                ? (int?)(entity.EndDate.Value.Date - today).TotalDays
                : null;

            bool isExpiringSoon = false;
            if (entity.Status == ContractStatus.Active
                && daysUntilExpiry.HasValue
                && notifyBeforeExpiryDays.HasValue
                && daysUntilExpiry.Value >= 0
                && daysUntilExpiry.Value <= notifyBeforeExpiryDays.Value)
            {
                isExpiringSoon = true;
            }
            else if (entity.Status == ContractStatus.ExpiringSoon)
            {
                isExpiringSoon = true;
            }

            return new ContractDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = employeeCode,
                EmployeeName = employeeName,
                ContractTypeId = entity.ContractTypeId,
                ContractTypeCode = contractTypeCode,
                ContractTypeName = contractTypeName,
                Code = entity.Code,
                SignDate = entity.SignDate,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                JobTitle = entity.JobTitle,
                WorkingLocation = entity.WorkingLocation,
                BasicSalary = entity.BasicSalary,
                Allowance = entity.Allowance,
                InsuranceSalary = entity.InsuranceSalary,
                PaymentMethod = entity.PaymentMethod,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                DepartmentId = entity.DepartmentId,
                DepartmentName = departmentName,
                PositionId = entity.PositionId,
                PositionName = positionName,
                SignedByCompanyRepresentative = entity.SignedByCompanyRepresentative,
                SignedByEmployeeName = entity.SignedByEmployeeName,
                IsAutoRenew = entity.IsAutoRenew,
                PreviousContractId = entity.PreviousContractId,
                PreviousContractCode = previousContractCode,
                RenewalTimes = entity.RenewalTimes,
                TerminationDate = entity.TerminationDate,
                TerminationReason = entity.TerminationReason,
                Status = entity.Status,
                FileUrl = entity.FileUrl,
                Note = entity.Note,
                DaysUntilExpiry = daysUntilExpiry,
                IsExpiringSoon = isExpiringSoon,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version
            };
        }

        public static void ApplyCommandFields(ContractEntity entity, ContractCommandFields fields)
        {
            if (!string.IsNullOrWhiteSpace(fields.Code))
            {
                entity.Code = fields.Code.Trim();
            }
            if (fields.ContractTypeId.HasValue)
            {
                entity.ContractTypeId = fields.ContractTypeId;
            }
            if (fields.SignDate.HasValue)
            {
                entity.SignDate = fields.SignDate;
            }
            if (fields.StartDate.HasValue)
            {
                entity.StartDate = fields.StartDate.Value;
            }
            if (fields.EndDate.HasValue || fields.ClearEndDate == true)
            {
                entity.EndDate = fields.ClearEndDate == true ? null : fields.EndDate;
            }
            if (fields.JobTitle != null)
            {
                entity.JobTitle = string.IsNullOrWhiteSpace(fields.JobTitle) ? null : fields.JobTitle.Trim();
            }
            if (fields.WorkingLocation != null)
            {
                entity.WorkingLocation = string.IsNullOrWhiteSpace(fields.WorkingLocation) ? null : fields.WorkingLocation.Trim();
            }
            if (fields.BasicSalary.HasValue)
            {
                entity.BasicSalary = fields.BasicSalary;
            }
            if (fields.Allowance.HasValue)
            {
                entity.Allowance = fields.Allowance;
            }
            if (fields.InsuranceSalary.HasValue)
            {
                entity.InsuranceSalary = fields.InsuranceSalary;
            }
            if (fields.PaymentMethod != null)
            {
                entity.PaymentMethod = string.IsNullOrWhiteSpace(fields.PaymentMethod) ? null : fields.PaymentMethod.Trim();
            }
            if (fields.SignedByCompanyRepresentative != null)
            {
                entity.SignedByCompanyRepresentative = string.IsNullOrWhiteSpace(fields.SignedByCompanyRepresentative)
                    ? null
                    : fields.SignedByCompanyRepresentative.Trim();
            }
            if (fields.SignedByEmployeeName != null)
            {
                entity.SignedByEmployeeName = string.IsNullOrWhiteSpace(fields.SignedByEmployeeName)
                    ? null
                    : fields.SignedByEmployeeName.Trim();
            }
            if (fields.IsAutoRenew.HasValue)
            {
                entity.IsAutoRenew = fields.IsAutoRenew.Value;
            }
            if (fields.FileUrl != null)
            {
                entity.FileUrl = string.IsNullOrWhiteSpace(fields.FileUrl) ? null : fields.FileUrl.Trim();
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

        public static object ToLogObject(ContractEntity entity)
        {
            return new
            {
                entity.Id,
                entity.EmployeeId,
                entity.ContractTypeId,
                entity.Code,
                entity.SignDate,
                entity.StartDate,
                entity.EndDate,
                entity.BasicSalary,
                entity.Status,
                entity.RenewalTimes,
                entity.TerminationDate,
                entity.PreviousContractId
            };
        }
    }

    public class ContractCommandFields
    {
        public Guid? EmployeeId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public string? Code { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? ClearEndDate { get; set; }
        public string? JobTitle { get; set; }
        public string? WorkingLocation { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? Allowance { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public string? PaymentMethod { get; set; }
        public string? SignedByCompanyRepresentative { get; set; }
        public string? SignedByEmployeeName { get; set; }
        public bool? IsAutoRenew { get; set; }
        public string? FileUrl { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }
}
