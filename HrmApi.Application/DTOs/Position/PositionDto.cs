using System;

namespace HrmApi.Application.DTOs.Position
{
    public class PositionDto : BaseDto
    {
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? PositionMasterId { get; set; }
        public string? PositionMasterName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PartId { get; set; }
        public string? PartName { get; set; }
        public int? QuantityStandard { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
