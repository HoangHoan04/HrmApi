namespace HrmApi.Application.DTOs.TransferEmployee
{
    public class TransferEmployeeDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public DateTime? RequestDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string? Reason { get; set; }
        public string? DecisionNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? DecisionFileUrl { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public List<TransferEmployeePositionDto> Details { get; set; } = [];
    }

    public class TransferEmployeePositionDto : BaseDto
    {
        public Guid TransferEmployeeId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Guid? OldCompanyId { get; set; }
        public string? OldCompanyName { get; set; }
        public Guid? NewCompanyId { get; set; }
        public string? NewCompanyName { get; set; }
        public Guid? OldBranchId { get; set; }
        public string? OldBranchName { get; set; }
        public Guid? NewBranchId { get; set; }
        public string? NewBranchName { get; set; }
        public Guid? OldDepartmentId { get; set; }
        public string? OldDepartmentName { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public string? NewDepartmentName { get; set; }
        public Guid? OldPartId { get; set; }
        public string? OldPartName { get; set; }
        public Guid? NewPartId { get; set; }
        public string? NewPartName { get; set; }
        public Guid? OldPositionId { get; set; }
        public string? OldPositionName { get; set; }
        public Guid? NewPositionId { get; set; }
        public string? NewPositionName { get; set; }
        public string? ChangeType { get; set; }
        public string? Note { get; set; }
    }

    public class TransferEmployeePositionInputDto
    {
        public Guid? Id { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public Guid? OldCompanyId { get; set; }
        public Guid? NewCompanyId { get; set; }
        public Guid? OldBranchId { get; set; }
        public Guid? NewBranchId { get; set; }
        public Guid? OldDepartmentId { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? OldPartId { get; set; }
        public Guid? NewPartId { get; set; }
        public Guid? OldPositionId { get; set; }
        public Guid? NewPositionId { get; set; }
        public string? ChangeType { get; set; }
        public string? Note { get; set; }
    }
}
