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
            string? partName = null,
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
                DecisionNumber = entity.DecisionNumber,
                SignDate = entity.SignDate,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                ProbationEndDate = entity.ProbationEndDate,
                JobTitle = entity.JobTitle,
                JobDescription = entity.JobDescription,
                WorkingLocation = entity.WorkingLocation,
                WorkingMode = entity.WorkingMode,
                WorkingHoursPerWeek = entity.WorkingHoursPerWeek,
                AnnualLeaveDays = entity.AnnualLeaveDays,
                BasicSalary = entity.BasicSalary,
                SalaryCoefficient = entity.SalaryCoefficient,
                Allowance = entity.Allowance,
                InsuranceSalary = entity.InsuranceSalary,
                Currency = entity.Currency,
                PaymentMethod = entity.PaymentMethod,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                DepartmentId = entity.DepartmentId,
                DepartmentName = departmentName,
                PartId = entity.PartId,
                PartName = partName,
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
                entity.ContractTypeId = fields.ContractTypeId == Guid.Empty ? null : fields.ContractTypeId;
            }
            if (fields.DecisionNumber != null)
            {
                entity.DecisionNumber = string.IsNullOrWhiteSpace(fields.DecisionNumber)
                    ? null
                    : fields.DecisionNumber.Trim();
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
            if (fields.ProbationEndDate.HasValue || fields.ClearProbationEndDate == true)
            {
                entity.ProbationEndDate = fields.ClearProbationEndDate == true ? null : fields.ProbationEndDate;
            }
            if (fields.JobTitle != null)
            {
                entity.JobTitle = string.IsNullOrWhiteSpace(fields.JobTitle) ? null : fields.JobTitle.Trim();
            }
            if (fields.JobDescription != null)
            {
                entity.JobDescription = string.IsNullOrWhiteSpace(fields.JobDescription)
                    ? null
                    : fields.JobDescription.Trim();
            }
            if (fields.WorkingLocation != null)
            {
                entity.WorkingLocation = string.IsNullOrWhiteSpace(fields.WorkingLocation)
                    ? null
                    : fields.WorkingLocation.Trim();
            }
            if (fields.WorkingMode != null)
            {
                entity.WorkingMode = string.IsNullOrWhiteSpace(fields.WorkingMode)
                    ? null
                    : fields.WorkingMode.Trim();
            }
            if (fields.WorkingHoursPerWeek.HasValue)
            {
                entity.WorkingHoursPerWeek = fields.WorkingHoursPerWeek;
            }
            if (fields.AnnualLeaveDays.HasValue)
            {
                entity.AnnualLeaveDays = fields.AnnualLeaveDays;
            }
            if (fields.BasicSalary.HasValue)
            {
                entity.BasicSalary = fields.BasicSalary;
            }
            if (fields.SalaryCoefficient.HasValue)
            {
                entity.SalaryCoefficient = fields.SalaryCoefficient;
            }
            if (fields.Allowance.HasValue)
            {
                entity.Allowance = fields.Allowance;
            }
            if (fields.InsuranceSalary.HasValue)
            {
                entity.InsuranceSalary = fields.InsuranceSalary;
            }
            if (fields.Currency != null)
            {
                entity.Currency = string.IsNullOrWhiteSpace(fields.Currency)
                    ? "VND"
                    : fields.Currency.Trim().ToUpperInvariant();
            }
            if (fields.PaymentMethod != null)
            {
                entity.PaymentMethod = string.IsNullOrWhiteSpace(fields.PaymentMethod)
                    ? null
                    : fields.PaymentMethod.Trim();
            }
            if (fields.CompanyId.HasValue)
            {
                entity.CompanyId = fields.CompanyId == Guid.Empty ? null : fields.CompanyId;
            }
            if (fields.BranchId.HasValue)
            {
                entity.BranchId = fields.BranchId == Guid.Empty ? null : fields.BranchId;
            }
            if (fields.DepartmentId.HasValue)
            {
                entity.DepartmentId = fields.DepartmentId == Guid.Empty ? null : fields.DepartmentId;
            }
            if (fields.PartId.HasValue)
            {
                entity.PartId = fields.PartId == Guid.Empty ? null : fields.PartId;
            }
            if (fields.PositionId.HasValue)
            {
                entity.PositionId = fields.PositionId == Guid.Empty ? null : fields.PositionId;
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
                entity.DecisionNumber,
                entity.SignDate,
                entity.StartDate,
                entity.EndDate,
                entity.ProbationEndDate,
                entity.BasicSalary,
                entity.SalaryCoefficient,
                entity.WorkingMode,
                entity.CompanyId,
                entity.BranchId,
                entity.DepartmentId,
                entity.PartId,
                entity.PositionId,
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
        public string? DecisionNumber { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? ClearEndDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool? ClearProbationEndDate { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        public string? WorkingLocation { get; set; }
        public string? WorkingMode { get; set; }
        public decimal? WorkingHoursPerWeek { get; set; }
        public int? AnnualLeaveDays { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? SalaryCoefficient { get; set; }
        public decimal? Allowance { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public string? Currency { get; set; }
        public string? PaymentMethod { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public string? SignedByCompanyRepresentative { get; set; }
        public string? SignedByEmployeeName { get; set; }
        public bool? IsAutoRenew { get; set; }
        public string? FileUrl { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }
}
