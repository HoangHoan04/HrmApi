using HrmApi.Application.DTOs.PublicHoliday;
using HrmApi.Domain.Entities.Leave;

namespace HrmApi.Application.Mappings
{
    internal class PublicHolidayMapper
    {
        public static PublicHolidayDto ToDto(PublicHolidayEntity entity, string? companyName = null)
        {
            return new PublicHolidayDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                HolidayDate = entity.HolidayDate,
                IsRecurringYearly = entity.IsRecurringYearly,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static void ApplyCommandFields(PublicHolidayEntity entity, PublicHolidayCommandFields fields)
        {
            entity.Code = fields.Code?.Trim() ?? entity.Code;
            entity.Name = fields.Name?.Trim() ?? entity.Name;
            if (fields.CompanyId.HasValue)
            {
                entity.CompanyId = fields.CompanyId;
            }

            if (fields.HolidayDate != default)
            {
                entity.HolidayDate = fields.HolidayDate;
            }

            if (fields.IsRecurringYearly.HasValue)
            {
                entity.IsRecurringYearly = fields.IsRecurringYearly.Value;
            }

            if (fields.Description != null)
            {
                entity.Description = string.IsNullOrWhiteSpace(fields.Description) ? null : fields.Description.Trim();
            }

            if (fields.IsActive.HasValue)
            {
                entity.IsActive = fields.IsActive.Value;
            }
        }

        public static object ToLogObject(PublicHolidayEntity entity)
        {
            return new
            {
                entity.Id,
                entity.Code,
                entity.Name,
                entity.CompanyId,
                entity.HolidayDate,
                entity.IsRecurringYearly,
                entity.IsActive
            };
        }
    }

    public class PublicHolidayCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public DateOnly HolidayDate { get; set; }
        public bool? IsRecurringYearly { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
