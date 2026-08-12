using HrmApi.Application.DTOs.Salary;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.Mappings
{
    internal static class SalaryMapper
    {
        public static SalaryDto ToDto(
            SalaryEntity entity,
            string? employeeCode = null,
            string? employeeName = null,
            string? salaryConfigName = null,
            string? companyName = null,
            string? branchName = null,
            string? departmentName = null,
            string? positionName = null)
        {
            List<SalaryLineItemDto> lines = (entity.LineItems ?? [])
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ItemName)
                .Select(ToLineDto)
                .ToList();

            return new SalaryDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = employeeCode,
                EmployeeName = employeeName,
                SalaryConfigId = entity.SalaryConfigId,
                SalaryConfigName = salaryConfigName,
                Year = entity.Year,
                Month = entity.Month,
                PeriodCode = entity.PeriodCode,
                PayDate = entity.PayDate,
                Status = entity.Status,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                DepartmentId = entity.DepartmentId,
                DepartmentName = departmentName,
                PositionId = entity.PositionId,
                PositionName = positionName,
                StandardWorkingDays = entity.StandardWorkingDays,
                ActualWorkingDays = entity.ActualWorkingDays,
                BasicSalary = entity.BasicSalary,
                GrossSalary = entity.GrossSalary,
                TotalDeduction = entity.TotalDeduction,
                NetSalary = entity.NetSalary,
                InsuranceSalary = entity.InsuranceSalary,
                Currency = entity.Currency,
                PayslipFileUrl = entity.PayslipFileUrl,
                ApprovedDate = entity.ApprovedDate,
                ApprovedBy = entity.ApprovedBy,
                PaidDate = entity.PaidDate,
                Note = entity.Note,
                LineItems = lines,
                IncomeItems = lines.Where(x => x.ItemType == SalaryItemType.Income).ToList(),
                DeductionItems = lines.Where(x => x.ItemType == SalaryItemType.Deduction).ToList(),
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version
            };
        }

        public static SalaryLineItemDto ToLineDto(SalaryLineItemEntity entity)
        {
            return new SalaryLineItemDto
            {
                Id = entity.Id,
                ItemType = entity.ItemType,
                ItemCode = entity.ItemCode,
                ItemName = entity.ItemName,
                Amount = entity.Amount,
                DisplayOrder = entity.DisplayOrder,
                Note = entity.Note
            };
        }

        public static void RecalculateTotals(SalaryEntity entity)
        {
            List<SalaryLineItemEntity> lines = (entity.LineItems ?? [])
                .Where(x => !x.IsDeleted)
                .ToList();
            entity.GrossSalary = lines
                .Where(x => x.ItemType == SalaryItemType.Income)
                .Sum(x => x.Amount);
            entity.TotalDeduction = lines
                .Where(x => x.ItemType == SalaryItemType.Deduction)
                .Sum(x => x.Amount);
            entity.NetSalary = entity.GrossSalary - entity.TotalDeduction;
            SalaryLineItemEntity? basic = lines.FirstOrDefault(x => x.ItemCode == SalaryItemCode.Basic);
            if (basic != null)
            {
                entity.BasicSalary = basic.Amount;
            }
        }

        public static object ToLogObject(SalaryEntity entity)
        {
            return new
            {
                entity.Id,
                entity.EmployeeId,
                entity.Year,
                entity.Month,
                entity.PeriodCode,
                entity.Status,
                entity.GrossSalary,
                entity.TotalDeduction,
                entity.NetSalary,
                entity.PayDate
            };
        }
    }

    public class SalaryLineItemCommandFields
    {
        public string? ItemType { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal? Amount { get; set; }
        public int? DisplayOrder { get; set; }
        public string? Note { get; set; }
    }

    public class SalaryCommandFields
    {
        public Guid? EmployeeId { get; set; }
        public Guid? SalaryConfigId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime? PayDate { get; set; }
        public string? Status { get; set; }
        public decimal? StandardWorkingDays { get; set; }
        public decimal? ActualWorkingDays { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public string? Currency { get; set; }
        public string? PayslipFileUrl { get; set; }
        public string? Note { get; set; }
        public List<SalaryLineItemCommandFields>? LineItems { get; set; }
        public bool? AutoGenerateInsuranceLines { get; set; }
    }

    internal static class SalaryConfigMapper
    {
        public static SalaryConfigDto ToDto(SalaryConfigEntity entity, string? companyName = null)
        {
            return new SalaryConfigDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                StandardWorkingDays = entity.StandardWorkingDays,
                BhxhEmployeeRate = entity.BhxhEmployeeRate,
                BhytEmployeeRate = entity.BhytEmployeeRate,
                BhtnEmployeeRate = entity.BhtnEmployeeRate,
                DefaultPayDay = entity.DefaultPayDay,
                IsComputePrevMonth = entity.IsComputePrevMonth,
                Currency = entity.Currency,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version
            };
        }

        public static SalaryConfigSelectBoxDto ToSelectBox(SalaryConfigEntity entity)
        {
            return new SalaryConfigSelectBoxDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                CompanyId = entity.CompanyId,
                StandardWorkingDays = entity.StandardWorkingDays,
                DefaultPayDay = entity.DefaultPayDay,
                Currency = entity.Currency
            };
        }

        public static void ApplyCommandFields(SalaryConfigEntity entity, SalaryConfigCommandFields fields)
        {
            if (!string.IsNullOrWhiteSpace(fields.Code))
            {
                entity.Code = fields.Code.Trim();
            }
            if (!string.IsNullOrWhiteSpace(fields.Name))
            {
                entity.Name = fields.Name.Trim();
            }
            if (fields.Description != null)
            {
                entity.Description = string.IsNullOrWhiteSpace(fields.Description) ? null : fields.Description.Trim();
            }
            if (fields.CompanyId.HasValue)
            {
                entity.CompanyId = fields.CompanyId == Guid.Empty ? null : fields.CompanyId;
            }
            if (fields.StandardWorkingDays.HasValue)
            {
                entity.StandardWorkingDays = fields.StandardWorkingDays.Value;
            }
            if (fields.BhxhEmployeeRate.HasValue)
            {
                entity.BhxhEmployeeRate = fields.BhxhEmployeeRate.Value;
            }
            if (fields.BhytEmployeeRate.HasValue)
            {
                entity.BhytEmployeeRate = fields.BhytEmployeeRate.Value;
            }
            if (fields.BhtnEmployeeRate.HasValue)
            {
                entity.BhtnEmployeeRate = fields.BhtnEmployeeRate.Value;
            }
            if (fields.DefaultPayDay.HasValue)
            {
                entity.DefaultPayDay = fields.DefaultPayDay;
            }
            if (fields.IsComputePrevMonth.HasValue)
            {
                entity.IsComputePrevMonth = fields.IsComputePrevMonth.Value;
            }
            if (!string.IsNullOrWhiteSpace(fields.Currency))
            {
                entity.Currency = fields.Currency.Trim().ToUpperInvariant();
            }
            if (fields.IsActive.HasValue)
            {
                entity.IsActive = fields.IsActive.Value;
            }
            if (fields.DisplayOrder.HasValue)
            {
                entity.DisplayOrder = fields.DisplayOrder.Value;
            }
        }

        public static object ToLogObject(SalaryConfigEntity entity)
        {
            return new
            {
                entity.Id,
                entity.Code,
                entity.Name,
                entity.CompanyId,
                entity.IsActive,
                entity.StandardWorkingDays
            };
        }
    }

    public class SalaryConfigCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public int? StandardWorkingDays { get; set; }
        public decimal? BhxhEmployeeRate { get; set; }
        public decimal? BhytEmployeeRate { get; set; }
        public decimal? BhtnEmployeeRate { get; set; }
        public int? DefaultPayDay { get; set; }
        public bool? IsComputePrevMonth { get; set; }
        public string? Currency { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
