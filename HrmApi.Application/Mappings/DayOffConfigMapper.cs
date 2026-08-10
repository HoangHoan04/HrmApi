using HrmApi.Application.DTOs.DayOffConfig;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.Mappings
{
    internal class DayOffConfigMapper
    {
        public static DayOffConfigDto ToDto(DayOffConfigEntity entity, string? companyName = null)
        {
            return new DayOffConfigDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                DayOffType = entity.DayOffType.ToString(),
                DefaultDaysPerYear = entity.DefaultDaysPerYear,
                IsPaid = entity.IsPaid,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static void ApplyCommandFields(DayOffConfigEntity entity, DayOffConfigCommandFields fields)
        {
            entity.Code = fields.Code?.Trim() ?? entity.Code;
            entity.Name = fields.Name?.Trim() ?? entity.Name;
            entity.Description = string.IsNullOrWhiteSpace(fields.Description) ? entity.Description : fields.Description.Trim();
            if (fields.CompanyId.HasValue)
            {
                entity.CompanyId = fields.CompanyId;
            }

            if (!string.IsNullOrWhiteSpace(fields.DayOffType))
            {
                if (System.Enum.TryParse<HrmApi.Domain.Enums.DayOffType>(fields.DayOffType, true, out DayOffType parsedType))
                {
                    entity.DayOffType = parsedType;
                }
            }
            if (fields.DefaultDaysPerYear.HasValue)
            {
                entity.DefaultDaysPerYear = fields.DefaultDaysPerYear.Value;
            }

            if (fields.IsPaid.HasValue)
            {
                entity.IsPaid = fields.IsPaid.Value;
            }

            if (fields.IsActive.HasValue)
            {
                entity.IsActive = fields.IsActive.Value;
            }
        }

        public static object ToLogObject(DayOffConfigEntity entity)
        {
            return new
            {
                entity.Id,
                entity.Code,
                entity.Name,
                entity.CompanyId,
                entity.DayOffType,
                entity.DefaultDaysPerYear,
                entity.IsPaid,
                entity.IsActive
            };
        }
    }

    public class DayOffConfigCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? DayOffType { get; set; }
        public decimal? DefaultDaysPerYear { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsActive { get; set; }
    }
}
