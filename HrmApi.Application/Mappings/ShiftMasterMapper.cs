using HrmApi.Application.DTOs.ShiftMaster;
using HrmApi.Domain.Entities.Timekeeping;

namespace HrmApi.Application.Mappings
{
    internal class ShiftMasterMapper
    {
        public static ShiftMasterDto ToDto(ShiftMasterEntity entity, string? companyName = null)
        {
            return new ShiftMasterDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                BreakStartTime = entity.BreakStartTime,
                BreakEndTime = entity.BreakEndTime,
                BreakMinutes = entity.BreakMinutes,
                WorkingMinutes = entity.WorkingMinutes,
                IsOvernight = entity.IsOvernight,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static void ApplyCommandFields(ShiftMasterEntity entity, ShiftMasterCommandFields fields)
        {
            entity.Code = fields.Code?.Trim() ?? entity.Code;
            entity.Name = fields.Name?.Trim() ?? entity.Name;
            entity.Description = string.IsNullOrWhiteSpace(fields.Description) ? entity.Description : fields.Description.Trim();
            if (fields.CompanyId.HasValue) entity.CompanyId = fields.CompanyId;
            if (fields.StartTime.HasValue) entity.StartTime = fields.StartTime.Value;
            if (fields.EndTime.HasValue) entity.EndTime = fields.EndTime.Value;
            entity.BreakStartTime = fields.BreakStartTime;
            entity.BreakEndTime = fields.BreakEndTime;
            if (fields.BreakMinutes.HasValue) entity.BreakMinutes = fields.BreakMinutes.Value;
            if (fields.WorkingMinutes.HasValue) entity.WorkingMinutes = fields.WorkingMinutes.Value;
            if (fields.IsOvernight.HasValue) entity.IsOvernight = fields.IsOvernight.Value;
            if (fields.IsActive.HasValue) entity.IsActive = fields.IsActive.Value;

            if (entity.BreakStartTime.HasValue && entity.BreakEndTime.HasValue)
            {
                int mins = (int)(entity.BreakEndTime.Value - entity.BreakStartTime.Value).TotalMinutes;
                if (mins < 0) mins += 24 * 60;
                entity.BreakMinutes = Math.Max(0, mins);
            }
        }

        public static object ToLogObject(ShiftMasterEntity entity) => new
        {
            entity.Id,
            entity.Code,
            entity.Name,
            entity.CompanyId,
            entity.StartTime,
            entity.EndTime,
            entity.BreakStartTime,
            entity.BreakEndTime,
            entity.BreakMinutes,
            entity.WorkingMinutes,
            entity.IsOvernight,
            entity.IsActive
        };
    }

    public class ShiftMasterCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public int? BreakMinutes { get; set; }
        public int? WorkingMinutes { get; set; }
        public bool? IsOvernight { get; set; }
        public bool? IsActive { get; set; }
    }
}
