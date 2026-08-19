using HrmApi.Application.DTOs.Department;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Mappings
{
    internal static class DepartmentMapper
    {
        public static DepartmentDto ToDto(
            DepartmentEntity entity,
            string? companyName = null,
            string? branchName = null,
            string? parentDepartmentName = null,
            string? managerName = null,
            string? deputyManagerName = null)
        {
            return new DepartmentDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                ShortName = entity.ShortName,
                Description = entity.Description,
                Type = entity.Type,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                ParentDepartmentId = entity.ParentDepartmentId,
                ParentDepartmentName = parentDepartmentName,
                Level = entity.Level,
                Limit = entity.Limit,
                CurrentHeadCount = entity.CurrentHeadCount,
                ManagerId = entity.ManagerId,
                ManagerName = managerName,
                DeputyManagerId = entity.DeputyManagerId,
                DeputyManagerName = deputyManagerName,
                Email = entity.Email,
                PhoneExtension = entity.PhoneExtension,
                CostCenterCode = entity.CostCenterCode,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                EstablishedDate = entity.EstablishedDate,
                DissolvedDate = entity.DissolvedDate,
                IsNotifyMarketing = entity.IsNotifyMarketing,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void ApplyCommandFields(DepartmentEntity entity, DepartmentCommandFields fields)
        {
            entity.Code = fields.Code.Trim();
            entity.Name = fields.Name.Trim();
            entity.ShortName = TrimOrNull(fields.ShortName);
            entity.Description = TrimOrNull(fields.Description);
            entity.Type = TrimOrNull(fields.Type);
            entity.CompanyId = fields.CompanyId;
            entity.BranchId = fields.BranchId;
            entity.ParentDepartmentId = fields.ParentDepartmentId;
            entity.Level = fields.Level;
            entity.Limit = fields.Limit ?? 0;
            entity.CurrentHeadCount = fields.CurrentHeadCount;
            entity.ManagerId = fields.ManagerId;
            entity.DeputyManagerId = fields.DeputyManagerId;
            entity.Email = TrimOrNull(fields.Email);
            entity.PhoneExtension = TrimOrNull(fields.PhoneExtension);
            entity.CostCenterCode = TrimOrNull(fields.CostCenterCode);
            entity.IsActive = fields.IsActive;
            entity.DisplayOrder = fields.DisplayOrder;
            entity.EstablishedDate = fields.EstablishedDate;
            entity.DissolvedDate = fields.DissolvedDate;
            entity.IsNotifyMarketing = fields.IsNotifyMarketing;
        }

        public static object ToLogObject(DepartmentEntity entity)
        {
            return new
            {
                entity.Code,
                entity.Name,
                entity.ShortName,
                entity.Description,
                entity.Type,
                entity.CompanyId,
                entity.BranchId,
                entity.ParentDepartmentId,
                entity.Level,
                entity.Limit,
                entity.CurrentHeadCount,
                entity.ManagerId,
                entity.DeputyManagerId,
                entity.Email,
                entity.PhoneExtension,
                entity.CostCenterCode,
                entity.IsActive,
                entity.DisplayOrder,
                entity.EstablishedDate,
                entity.DissolvedDate,
                entity.IsNotifyMarketing
            };
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public class DepartmentCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? ParentDepartmentId { get; set; }
        public int Level { get; set; } = 1;
        public int? Limit { get; set; }
        public int? CurrentHeadCount { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? DeputyManagerId { get; set; }
        public string? Email { get; set; }
        public string? PhoneExtension { get; set; }
        public string? CostCenterCode { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public DateTime? DissolvedDate { get; set; }
        public bool IsNotifyMarketing { get; set; }
    }
}
