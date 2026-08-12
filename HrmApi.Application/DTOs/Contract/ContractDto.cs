namespace HrmApi.Application.DTOs.Contract
{
    public class ContractDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? ContractTypeId { get; set; }
        public string? ContractTypeCode { get; set; }
        public string? ContractTypeName { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime? SignDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? JobTitle { get; set; }
        public string? WorkingLocation { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? Allowance { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public string? PaymentMethod { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? SignedByCompanyRepresentative { get; set; }
        public string? SignedByEmployeeName { get; set; }
        public bool IsAutoRenew { get; set; }
        public Guid? PreviousContractId { get; set; }
        public string? PreviousContractCode { get; set; }
        public int RenewalTimes { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string? TerminationReason { get; set; }
        public string? Status { get; set; }
        public string? FileUrl { get; set; }
        public string? Note { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public bool IsExpiringSoon { get; set; }
    }
}
