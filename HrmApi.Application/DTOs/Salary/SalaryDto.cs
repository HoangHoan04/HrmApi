namespace HrmApi.Application.DTOs.Salary
{
    public class SalaryLineItemDto
    {
        public Guid? Id { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int DisplayOrder { get; set; }
        public string? Note { get; set; }
    }

    public class SalaryDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? SalaryConfigId { get; set; }
        public string? SalaryConfigName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public DateTime? PayDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public decimal? StandardWorkingDays { get; set; }
        public decimal? ActualWorkingDays { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetSalary { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public string Currency { get; set; } = "VND";
        public string? PayslipFileUrl { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Note { get; set; }
        public List<SalaryLineItemDto> LineItems { get; set; } = [];
        public List<SalaryLineItemDto> IncomeItems { get; set; } = [];
        public List<SalaryLineItemDto> DeductionItems { get; set; } = [];
    }

    public class SalaryConfigDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int StandardWorkingDays { get; set; }
        public decimal BhxhEmployeeRate { get; set; }
        public decimal BhytEmployeeRate { get; set; }
        public decimal BhtnEmployeeRate { get; set; }
        public int? DefaultPayDay { get; set; }
        public bool IsComputePrevMonth { get; set; }
        public string Currency { get; set; } = "VND";
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class SalaryConfigSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public int StandardWorkingDays { get; set; }
        public int? DefaultPayDay { get; set; }
        public string Currency { get; set; } = "VND";
    }
}
