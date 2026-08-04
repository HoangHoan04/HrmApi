using HrmApi.Application.DTOs.Position;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Mappings
{
    internal static class PositionMapper
    {
        public static PositionDto ToDto(
            PositionEntity entity,
            string? companyName = null,
            string? branchName = null,
            string? departmentName = null,
            string? partName = null,
            string? positionMasterName = null)
        {
            return new PositionDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                PositionMasterId = entity.PositionMasterId,
                PositionMasterName = positionMasterName,
                DepartmentId = entity.DepartmentId,
                DepartmentName = departmentName,
                PartId = entity.PartId,
                PartName = partName,
                QuantityStandard = entity.QuantityStandard,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void ApplyCommandFields(PositionEntity entity, PositionCommandFields fields)
        {
            entity.CompanyId = fields.CompanyId;
            entity.BranchId = fields.BranchId;
            entity.PositionMasterId = fields.PositionMasterId;
            entity.DepartmentId = fields.DepartmentId;
            entity.PartId = fields.PartId;
            entity.QuantityStandard = fields.QuantityStandard;
            entity.IsActive = fields.IsActive;
            entity.DisplayOrder = fields.DisplayOrder;
        }

        public static object ToLogObject(PositionEntity entity)
        {
            return new
            {
                entity.CompanyId,
                entity.BranchId,
                entity.PositionMasterId,
                entity.DepartmentId,
                entity.PartId,
                entity.QuantityStandard,
                entity.IsActive,
                entity.DisplayOrder
            };
        }
    }

    public class PositionCommandFields
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? PositionMasterId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public int? QuantityStandard { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
