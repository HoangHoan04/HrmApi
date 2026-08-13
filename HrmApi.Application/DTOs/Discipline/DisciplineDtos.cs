using HrmApi.Application.Common.Models;

namespace HrmApi.Application.DTOs.Discipline
{
    public class ViolationTypeDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ViolationTypeCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class ViolationTypePagedQuery : PagedRequest
    {
        public bool? IsActive { get; set; }
        public string? Severity { get; set; }
    }

    public class ViolationDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid ViolationTypeId { get; set; }
        public string? ViolationTypeName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Description { get; set; }
        public string? Decision { get; set; }
        public string PenaltyType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class ViolationCommandFields
    {
        public string? Code { get; set; }
        public Guid? ViolationTypeId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public DateTime? OccurredAt { get; set; }
        public string? Description { get; set; }
        public string? Decision { get; set; }
        public string? PenaltyType { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class ViolationPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? ViolationTypeId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Status { get; set; }
    }
}
