using HrmApi.Application.DTOs.TimeKeepingStandard;
using HrmApi.Domain.Entities.Timekeeping;

namespace HrmApi.Application.Mappings
{
    internal class TimeKeepingStandardMapper
    {
        public static TimeKeepingStandardDto ToDto(TimeKeepingStandardEntity entity, string? companyName = null)
        {
            return new TimeKeepingStandardDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                AllowedRadiusMeters = entity.AllowedRadiusMeters,
                LateGraceMinutes = entity.LateGraceMinutes,
                EarlyLeaveGraceMinutes = entity.EarlyLeaveGraceMinutes,
                NightStartTime = entity.NightStartTime,
                NightEndTime = entity.NightEndTime,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static void ApplyCommandFields(TimeKeepingStandardEntity entity, TimeKeepingStandardCommandFields fields)
        {
            entity.Code = fields.Code?.Trim() ?? entity.Code;
            entity.Name = fields.Name?.Trim() ?? entity.Name;
            entity.Description = string.IsNullOrWhiteSpace(fields.Description) ? entity.Description : fields.Description.Trim();
            if (fields.CompanyId.HasValue) entity.CompanyId = fields.CompanyId;
            if (fields.AllowedRadiusMeters.HasValue) entity.AllowedRadiusMeters = fields.AllowedRadiusMeters.Value;
            if (fields.LateGraceMinutes.HasValue) entity.LateGraceMinutes = fields.LateGraceMinutes.Value;
            if (fields.EarlyLeaveGraceMinutes.HasValue) entity.EarlyLeaveGraceMinutes = fields.EarlyLeaveGraceMinutes.Value;
            if (fields.NightStartTime.HasValue) entity.NightStartTime = fields.NightStartTime.Value;
            if (fields.NightEndTime.HasValue) entity.NightEndTime = fields.NightEndTime.Value;
            if (fields.IsActive.HasValue) entity.IsActive = fields.IsActive.Value;
        }

        public static object ToLogObject(TimeKeepingStandardEntity entity) => new
        {
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.CompanyId,
            entity.AllowedRadiusMeters,
            entity.LateGraceMinutes,
            entity.EarlyLeaveGraceMinutes,
            entity.NightStartTime,
            entity.NightEndTime,
            entity.IsActive
        };
    }

    public class TimeKeepingStandardCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public int? AllowedRadiusMeters { get; set; }
        public int? LateGraceMinutes { get; set; }
        public int? EarlyLeaveGraceMinutes { get; set; }
        public TimeSpan? NightStartTime { get; set; }
        public TimeSpan? NightEndTime { get; set; }
        public bool? IsActive { get; set; }
    }
}
