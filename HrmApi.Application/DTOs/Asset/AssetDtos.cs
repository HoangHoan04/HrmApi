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
        public bool IsSerialRequired { get; set; } = true;
        public int? MaxPerEmployee { get; set; }
    }

    public class AssetTypeCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSerialRequired { get; set; }
        public int? MaxPerEmployee { get; set; }
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
        public DateOnly? WarrantyExpiryDate { get; set; }
        public string? Vendor { get; set; }
        public string? Model { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }

        public Guid? CurrentHolderEmployeeId { get; set; }
        public string? CurrentHolderEmployeeCode { get; set; }
        public string? CurrentHolderEmployeeName { get; set; }
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
        public DateOnly? WarrantyExpiryDate { get; set; }
        public string? Vendor { get; set; }
        public string? Model { get; set; }
        public string? Location { get; set; }
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
        public Guid? ToEmployeeId { get; set; }
        public string? ToEmployeeCode { get; set; }
        public string? ToEmployeeName { get; set; }
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string TicketType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TicketAt { get; set; }
        public DateOnly? ReturnExpectedDate { get; set; }
        public string? Condition { get; set; }
        public string? Note { get; set; }
    }

    public class AssetTicketCommandFields
    {
        public string? Code { get; set; }
        public Guid? AssetId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ToEmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? TicketType { get; set; }
        public string? Status { get; set; }
        public DateTime? TicketAt { get; set; }
        public DateOnly? ReturnExpectedDate { get; set; }
        public string? Condition { get; set; }
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

    public class AssetAssignmentDto : BaseDto
    {
        public Guid AssetId { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public string? SerialNumber { get; set; }
        public string? AssetTypeName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string? ConditionOnIssue { get; set; }
        public string? ConditionOnReturn { get; set; }
        public string? Note { get; set; }
        public bool IsHolding => !ReturnedAt.HasValue;
    }

    public class EmployeeAssetSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public List<AssetAssignmentDto> CurrentHoldingAssets { get; set; } = [];
        public List<AssetAssignmentDto> PastAssetHistories { get; set; } = [];
    }

    public class EmployeeAssetClearanceDto
    {
        public Guid EmployeeId { get; set; }
        public bool HasUnreturnedAssets { get; set; }
        public int UnreturnedCount { get; set; }
        public List<AssetAssignmentDto> UnreturnedAssets { get; set; } = [];
    }
}
