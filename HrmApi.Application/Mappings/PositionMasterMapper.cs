using HrmApi.Application.DTOs.PositionMaster;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Mappings
{
    internal static class PositionMasterMapper
    {
        public static PositionMasterDto ToDto(
            PositionMasterEntity entity,
            string? companyName = null,
            string? branchName = null)
        {
            return new PositionMasterDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                BranchId = entity.BranchId,
                BranchName = branchName,
                IsLimitHoursWorking = entity.IsLimitHoursWorking,
                Limit = entity.Limit,
                WorkingHour = entity.WorkingHour,
                MinimumWorkingHour = entity.MinimumWorkingHour,
                HourWorkingStart = entity.HourWorkingStart,
                HourWorkingEnd = entity.HourWorkingEnd,
                IsTimeKeeping = entity.IsTimeKeeping,
                HourSnapShotStart = entity.HourSnapShotStart,
                HourSnapShotEnd = entity.HourSnapShotEnd,
                IsAllowOverTimekeepingStandard = entity.IsAllowOverTimekeepingStandard,
                IsSwapPosition = entity.IsSwapPosition,
                TargetChangePositionIds = entity.TargetChangePositionIds,
                IsApprovedWhenHiringCandidate = entity.IsApprovedWhenHiringCandidate,
                IsHadASecondInterview = entity.IsHadASecondInterview,
                IsApprovedDayOff = entity.IsApprovedDayOff,
                QuantityStandard = entity.QuantityStandard,
                GradeCode = entity.GradeCode,
                GradeName = entity.GradeName,
                SalaryMin = entity.SalaryMin,
                SalaryMax = entity.SalaryMax,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void ApplyCommandFields(PositionMasterEntity entity, PositionMasterCommandFields fields)
        {
            entity.Code = fields.Code.Trim();
            entity.Name = fields.Name.Trim();
            entity.Description = TrimOrNull(fields.Description);
            entity.CompanyId = fields.CompanyId;
            entity.BranchId = fields.BranchId;
            entity.IsLimitHoursWorking = fields.IsLimitHoursWorking;
            entity.Limit = TrimOrNull(fields.Limit);
            entity.WorkingHour = fields.WorkingHour;
            entity.MinimumWorkingHour = fields.MinimumWorkingHour;
            entity.HourWorkingStart = fields.HourWorkingStart;
            entity.HourWorkingEnd = fields.HourWorkingEnd;
            entity.IsTimeKeeping = fields.IsTimeKeeping;
            entity.HourSnapShotStart = fields.HourSnapShotStart;
            entity.HourSnapShotEnd = fields.HourSnapShotEnd;
            entity.IsAllowOverTimekeepingStandard = fields.IsAllowOverTimekeepingStandard;
            entity.IsSwapPosition = fields.IsSwapPosition;
            entity.TargetChangePositionIds = TrimOrNull(fields.TargetChangePositionIds);
            entity.IsApprovedWhenHiringCandidate = fields.IsApprovedWhenHiringCandidate;
            entity.IsHadASecondInterview = fields.IsHadASecondInterview;
            entity.IsApprovedDayOff = fields.IsApprovedDayOff;
            entity.QuantityStandard = fields.QuantityStandard;
            entity.GradeCode = TrimOrNull(fields.GradeCode);
            entity.GradeName = TrimOrNull(fields.GradeName);
            entity.SalaryMin = fields.SalaryMin;
            entity.SalaryMax = fields.SalaryMax;
            if (entity.SalaryMin.HasValue && entity.SalaryMax.HasValue
                && entity.SalaryMin.Value > entity.SalaryMax.Value)
            {
                throw new InvalidOperationException("Lương tối thiểu không được lớn hơn lương tối đa.");
            }
            entity.IsActive = fields.IsActive;
            entity.DisplayOrder = fields.DisplayOrder;
        }

        public static object ToLogObject(PositionMasterEntity entity)
        {
            return new
            {
                entity.Code,
                entity.Name,
                entity.Description,
                entity.CompanyId,
                entity.BranchId,
                entity.IsLimitHoursWorking,
                entity.Limit,
                entity.WorkingHour,
                entity.MinimumWorkingHour,
                entity.HourWorkingStart,
                entity.HourWorkingEnd,
                entity.IsTimeKeeping,
                entity.HourSnapShotStart,
                entity.HourSnapShotEnd,
                entity.IsAllowOverTimekeepingStandard,
                entity.IsSwapPosition,
                entity.TargetChangePositionIds,
                entity.IsApprovedWhenHiringCandidate,
                entity.IsHadASecondInterview,
                entity.IsApprovedDayOff,
                entity.QuantityStandard,
                entity.GradeCode,
                entity.GradeName,
                entity.SalaryMin,
                entity.SalaryMax,
                entity.IsActive,
                entity.DisplayOrder
            };
        }

        private static string? TrimOrNull(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public class PositionMasterCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsLimitHoursWorking { get; set; }
        public string? Limit { get; set; }
        public int? WorkingHour { get; set; }
        public int? MinimumWorkingHour { get; set; }
        public TimeSpan? HourWorkingStart { get; set; }
        public TimeSpan? HourWorkingEnd { get; set; }
        public bool IsTimeKeeping { get; set; }
        public TimeSpan? HourSnapShotStart { get; set; }
        public TimeSpan? HourSnapShotEnd { get; set; }
        public bool IsAllowOverTimekeepingStandard { get; set; }
        public bool IsSwapPosition { get; set; }
        public string? TargetChangePositionIds { get; set; }
        public bool IsApprovedWhenHiringCandidate { get; set; }
        public bool IsHadASecondInterview { get; set; }
        public bool IsApprovedDayOff { get; set; }
        public int? QuantityStandard { get; set; }
        public string? GradeCode { get; set; }
        public string? GradeName { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
