using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Domain.Entities.Timekeeping;

namespace HrmApi.Application.Mappings
{
    internal class TimekeepingMapper
    {
        public static TimekeepingDto ToDto(
            TimekeepingEntity entity,
            string? employeeName = null,
            string? employeeCode = null,
            string? branchName = null,
            string? shiftMasterName = null)
        {
            return new TimekeepingDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName,
                EmployeeCode = employeeCode,
                CompanyId = entity.CompanyId,
                BranchId = entity.BranchId,
                BranchName = branchName,
                WorkDate = entity.WorkDate,
                ShiftId = entity.ShiftId,
                ShiftMasterId = entity.ShiftMasterId,
                ShiftMasterName = shiftMasterName,
                CheckInAt = entity.CheckInAt,
                CheckOutAt = entity.CheckOutAt,
                CheckInLatitude = entity.CheckInLatitude,
                CheckInLongitude = entity.CheckInLongitude,
                CheckOutLatitude = entity.CheckOutLatitude,
                CheckOutLongitude = entity.CheckOutLongitude,
                CheckInDistanceM = entity.CheckInDistanceM,
                CheckOutDistanceM = entity.CheckOutDistanceM,
                Status = entity.Status.ToString(),
                LateMinutes = entity.LateMinutes,
                EarlyMinutes = entity.EarlyMinutes,
                WorkedMinutes = entity.WorkedMinutes,
                Note = entity.Note,
                IsManualAdjusted = entity.IsManualAdjusted,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static TimekeepingSummaryDto ToSummaryDto(
            TimekeepingSummaryEntity entity,
            string? employeeName = null,
            string? employeeCode = null,
            string? branchName = null)
        {
            return new TimekeepingSummaryDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName,
                EmployeeCode = employeeCode,
                CompanyId = entity.CompanyId,
                BranchId = entity.BranchId,
                BranchName = branchName,
                Year = entity.Year,
                Month = entity.Month,
                OnTimeDays = entity.OnTimeDays,
                LateDays = entity.LateDays,
                EarlyDays = entity.EarlyDays,
                LeaveDays = entity.LeaveDays,
                AbsentDays = entity.AbsentDays,
                IncompleteDays = entity.IncompleteDays,
                WorkingDays = entity.WorkingDays,
                TotalWorkedMinutes = entity.TotalWorkedMinutes,
                TotalLateMinutes = entity.TotalLateMinutes,
                TotalEarlyMinutes = entity.TotalEarlyMinutes,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static MobileTodayDto ToTodayDto(
            TimekeepingEntity? entity,
            DateOnly workDate,
            bool onLeave,
            TimeSpan? expectedStart,
            TimeSpan? expectedEnd,
            string? branchName,
            int allowedRadiusMeters)
        {
            var hasIn = entity?.CheckInAt != null;
            var hasOut = entity?.CheckOutAt != null;
            return new MobileTodayDto
            {
                WorkDate = workDate,
                Status = (onLeave ? Domain.Enums.AttendanceStatus.LEAVE : (entity?.Status ?? Domain.Enums.AttendanceStatus.INCOMPLETE)).ToString(),
                CheckInAt = entity?.CheckInAt,
                CheckOutAt = entity?.CheckOutAt,
                LateMinutes = entity?.LateMinutes ?? 0,
                EarlyMinutes = entity?.EarlyMinutes ?? 0,
                WorkedMinutes = entity?.WorkedMinutes ?? 0,
                Note = entity?.Note,
                OnLeave = onLeave,
                CanCheckIn = !onLeave && !hasIn,
                CanCheckOut = !onLeave && hasIn && !hasOut,
                ExpectedStart = expectedStart,
                ExpectedEnd = expectedEnd,
                BranchName = branchName,
                AllowedRadiusMeters = allowedRadiusMeters
            };
        }

        public static MobileMonthDayDto ToMonthDayDto(TimekeepingEntity entity) => new()
        {
            WorkDate = entity.WorkDate,
            Status = entity.Status.ToString(),
            CheckInAt = entity.CheckInAt,
            CheckOutAt = entity.CheckOutAt,
            WorkedMinutes = entity.WorkedMinutes,
            LateMinutes = entity.LateMinutes,
            EarlyMinutes = entity.EarlyMinutes,
            Note = entity.Note
        };

        public static object ToLogObject(TimekeepingEntity entity) => new
        {
            entity.Id,
            entity.EmployeeId,
            entity.WorkDate,
            entity.CheckInAt,
            entity.CheckOutAt,
            entity.Status,
            entity.LateMinutes,
            entity.EarlyMinutes,
            entity.WorkedMinutes,
            entity.Note,
            entity.IsManualAdjusted
        };
    }

    public class ManualAdjustTimekeepingFields
    {
        public Guid Id { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }
}
