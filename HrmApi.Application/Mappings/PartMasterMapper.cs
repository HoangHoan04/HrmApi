using HrmApi.Application.DTOs.PartMaster;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Mappings
{
    internal static class PartMasterMapper
    {
        public static PartMasterDto ToDto(PartMasterEntity entity, string? companyName = null, string? branchName = null)
        {
            return new PartMasterDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                Type = entity.Type,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void ApplyCommandFields(PartMasterEntity entity, PartMasterCommandFields fields)
        {
            entity.Code = fields.Code.Trim();
            entity.Name = fields.Name.Trim();
            entity.Description = TrimOrNull(fields.Description);
            entity.CompanyId = fields.CompanyId;
            entity.BranchId = fields.BranchId;
            entity.Type = TrimOrNull(fields.Type);
            entity.IsActive = fields.IsActive;
            entity.DisplayOrder = fields.DisplayOrder;
        }

        public static object ToLogObject(PartMasterEntity entity)
        {
            return new
            {
                entity.Code,
                entity.Name,
                entity.Description,
                entity.CompanyId,
                entity.BranchId,
                entity.Type,
                entity.IsActive,
                entity.DisplayOrder
            };
        }

        private static string? TrimOrNull(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public class PartMasterCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Type { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}