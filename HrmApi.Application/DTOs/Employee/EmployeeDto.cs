using System;
using System.Collections.Generic;

namespace HrmApi.Application.DTOs
{
    public class EmployeeDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? SecondaryPhone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? CompanyEmail { get; set; }
        public DateTime DayOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }
        public string IdentityCard { get; set; } = string.Empty;
        public string PlaceOfIsssuance { get; set; } = string.Empty;
        public DateTime IssuanceDate { get; set; }
        public string? PermanentAddress { get; set; }
        public string? NowAddress { get; set; }
        public string? CurrentCity { get; set; }
        public string? CurrentWard { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? Bankname { get; set; }
        public string? BankBranchName { get; set; }
        public string? BankAccountHolder { get; set; }
        public string? TaxCode { get; set; }
        public string? SocialInsuranceNumber { get; set; }
        public string? HealthInsuranceNumber { get; set; }
        public string? Level { get; set; }
        public string? WorkingMode { get; set; }
        public string? ContractType { get; set; }
        public string? Status { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? ResignationDate { get; set; }
        public string? ResignationReason { get; set; }

        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }

        public List<EmployeeDependentDto> Dependents { get; set; } = [];
        public List<EmployeeEducationDto> Educations { get; set; } = [];
        public List<EmployeeCertificateDto> Certificates { get; set; } = [];
        public List<EmployeeFileDto> Files { get; set; } = [];
        public List<EmployeeSalaryHistoryDto> SalaryHistories { get; set; } = [];
    }

    public class EmployeeDependentDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public DateTime? DayOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IdentityNumber { get; set; }
        public string? TaxCode { get; set; }
        public DateTime? DependentFromDate { get; set; }
        public DateTime? DependentToDate { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class EmployeeEducationDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public string? Major { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Gpa { get; set; }
    }

    public class EmployeeCertificateDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IssuingOrganization { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CredentialId { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class EmployeeFileDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string FileCategory { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public string? Description { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class EmployeeSalaryHistoryDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal? OldBasicSalary { get; set; }
        public decimal NewBasicSalary { get; set; }
        public decimal? Allowance { get; set; }
        public string? ChangeType { get; set; }
        public string? Reason { get; set; }
        public string? DecisionNumber { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Note { get; set; }
    }
}
