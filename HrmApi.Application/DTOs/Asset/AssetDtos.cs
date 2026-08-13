using HrmApi.Application.Common.Models;

namespace HrmApi.Application.DTOs.Asset
{
    public class AssetTypeDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssetTypeCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AssetTypePagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AssetDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid AssetTypeId { get; set; }
        public string? AssetTypeName { get; set; }
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? SerialNumber { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class AssetCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? AssetTypeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? SerialNumber { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class AssetPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? AssetTypeId { get; set; }
        public string? Status { get; set; }
    }

    public class AssetTicketDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid AssetId { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string TicketType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TicketAt { get; set; }
        public string? Note { get; set; }
    }

    public class AssetTicketCommandFields
    {
        public string? Code { get; set; }
        public Guid? AssetId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string? TicketType { get; set; }
        public string? Status { get; set; }
        public DateTime? TicketAt { get; set; }
        public string? Note { get; set; }
    }

    public class AssetTicketPagedQuery : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? AssetId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? TicketType { get; set; }
        public string? Status { get; set; }
    }
}
