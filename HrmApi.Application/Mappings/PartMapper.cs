using HrmApi.Application.DTOs.Part;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Mappings
{
    internal static class PartMapper
    {
        public static PartDto ToDto(
            PartEntity entity,
            string? companyName = null,
            string? branchName = null,
            string? departmentName = null,
            string? partMasterName = null)
        {
            return new PartDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                PartMasterId = entity.PartMasterId,
                PartMasterName = partMasterName,
                DepartmentId = entity.DepartmentId,
                DepartmentName = departmentName,
                ManagerId = entity.ManagerId,
                Limit = entity.Limit,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void ApplyCommandFields(PartEntity entity, PartCommandFields fields)
        {
            entity.Code = TrimOrNull(fields.Code);
            entity.Name = TrimOrNull(fields.Name);
            entity.Description = TrimOrNull(fields.Description);
            entity.CompanyId = fields.CompanyId;
            entity.BranchId = fields.BranchId;
            entity.PartMasterId = fields.PartMasterId;
            entity.DepartmentId = fields.DepartmentId;
            entity.ManagerId = fields.ManagerId;
            entity.Limit = fields.Limit;
            entity.IsActive = fields.IsActive;
            entity.DisplayOrder = fields.DisplayOrder;
        }

        public static object ToLogObject(PartEntity entity)
        {
            return new
            {
                entity.Code,
                entity.Name,
                entity.Description,
                entity.CompanyId,
                entity.BranchId,
                entity.PartMasterId,
                entity.DepartmentId,
                entity.ManagerId,
                entity.Limit,
                entity.IsActive,
                entity.DisplayOrder
            };
        }

        private static string? TrimOrNull(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public class PartCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? PartMasterId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
        public int? Limit { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}