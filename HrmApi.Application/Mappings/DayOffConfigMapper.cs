using HrmApi.Application.DTOs.DayOffConfig;
using HrmApi.Domain.Entities.Leave;

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
                DefaultDaysPerYear = entity.DefaultDaysPerYear,
                IsPaid = entity.IsPaid,
                DeductBalance = entity.DeductBalance,
                RequireAttachment = entity.RequireAttachment,
                MaxDaysPerRequest = entity.MaxDaysPerRequest,
                MinNoticeDays = entity.MinNoticeDays,
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

            if (fields.DefaultDaysPerYear.HasValue)
            {
                entity.DefaultDaysPerYear = fields.DefaultDaysPerYear.Value;
            }

            if (fields.IsPaid.HasValue)
            {
                entity.IsPaid = fields.IsPaid.Value;
            }

            if (fields.DeductBalance.HasValue)
            {
                entity.DeductBalance = fields.DeductBalance.Value;
            }

            if (fields.RequireAttachment.HasValue)
            {
                entity.RequireAttachment = fields.RequireAttachment.Value;
            }

            if (fields.MaxDaysPerRequest.HasValue)
            {
                entity.MaxDaysPerRequest = fields.MaxDaysPerRequest;
            }

            if (fields.MinNoticeDays.HasValue)
            {
                entity.MinNoticeDays = fields.MinNoticeDays.Value;
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
                entity.DefaultDaysPerYear,
                entity.IsPaid,
                entity.DeductBalance,
                entity.RequireAttachment,
                entity.MaxDaysPerRequest,
                entity.MinNoticeDays,
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
        public decimal? DefaultDaysPerYear { get; set; }
        public bool? IsPaid { get; set; }
        public bool? DeductBalance { get; set; }
        public bool? RequireAttachment { get; set; }
        public decimal? MaxDaysPerRequest { get; set; }
        public int? MinNoticeDays { get; set; }
        public bool? IsActive { get; set; }
    }
}
