using HrmApi.Application.DTOs.ContractType;
using HrmApi.Domain.Entities.Contract;

namespace HrmApi.Application.Mappings
{
    internal class ContractTypeMapper
    {
        public static ContractTypeDto ToDto(ContractTypeEntity entity, string? companyName = null)
        {
            return new ContractTypeDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                IsProbation = entity.IsProbation,
                IsUnlimited = entity.IsUnlimited,
                DefaultDurationMonths = entity.DefaultDurationMonths,
                MaxRenewalTimes = entity.MaxRenewalTimes,
                NotifyBeforeExpiryDays = entity.NotifyBeforeExpiryDays,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version
            };
        }

        public static void ApplyCommandFields(ContractTypeEntity entity, ContractTypeCommandFields fields)
        {
            entity.Code = fields.Code?.Trim() ?? entity.Code;
            entity.Name = fields.Name?.Trim() ?? entity.Name;
            entity.Description = string.IsNullOrWhiteSpace(fields.Description) ? null : fields.Description.Trim();
            entity.CompanyId = fields.CompanyId;
            if (fields.IsProbation.HasValue)
            {
                entity.IsProbation = fields.IsProbation.Value;
            }
            if (fields.IsUnlimited.HasValue)
            {
                entity.IsUnlimited = fields.IsUnlimited.Value;
            }
            entity.DefaultDurationMonths = fields.DefaultDurationMonths;
            entity.MaxRenewalTimes = fields.MaxRenewalTimes;
            entity.NotifyBeforeExpiryDays = fields.NotifyBeforeExpiryDays;
            if (fields.IsActive.HasValue)
            {
                entity.IsActive = fields.IsActive.Value;
            }
            if (fields.DisplayOrder.HasValue)
            {
                entity.DisplayOrder = fields.DisplayOrder.Value;
            }
        }

        public static object ToLogObject(ContractTypeEntity entity)
        {
            return new
            {
                entity.Id,
                entity.Code,
                entity.Name,
                entity.Description,
                entity.CompanyId,
                entity.IsProbation,
                entity.IsUnlimited,
                entity.DefaultDurationMonths,
                entity.MaxRenewalTimes,
                entity.NotifyBeforeExpiryDays,
                entity.IsActive,
                entity.DisplayOrder
            };
        }
    }

    public class ContractTypeCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsProbation { get; set; }
        public bool? IsUnlimited { get; set; }
        public int? DefaultDurationMonths { get; set; }
        public int? MaxRenewalTimes { get; set; }
        public int? NotifyBeforeExpiryDays { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
