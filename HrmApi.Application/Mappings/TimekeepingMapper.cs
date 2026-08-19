using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;

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
                Status = entity.Status,
                LateMinutes = entity.LateMinutes,
                EarlyMinutes = entity.EarlyMinutes,
                WorkedMinutes = entity.WorkedMinutes,
                OtMinutes = entity.OtMinutes,
                NightMinutes = entity.NightMinutes,
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
                TotalOtMinutes = entity.TotalOtMinutes,
                TotalNightMinutes = entity.TotalNightMinutes,
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
            WorkWindowResult window,
            string? branchName,
            int allowedRadiusMeters)
        {
            bool hasIn = entity?.CheckInAt != null;
            bool hasOut = entity?.CheckOutAt != null;
            return new MobileTodayDto
            {
                WorkDate = workDate,
                Status = onLeave
                    ? AttendanceStatus.LEAVE
                    : (entity?.Status ?? AttendanceStatus.INCOMPLETE),
                CheckInAt = entity?.CheckInAt,
                CheckOutAt = entity?.CheckOutAt,
                LateMinutes = entity?.LateMinutes ?? 0,
                EarlyMinutes = entity?.EarlyMinutes ?? 0,
                WorkedMinutes = entity?.WorkedMinutes ?? 0,
                Note = entity?.Note,
                OnLeave = onLeave,
                CanCheckIn = !onLeave && !hasIn && window.IsScheduledWorkDay,
                CanCheckOut = !onLeave && hasIn && !hasOut,
                ExpectedStart = window.StartTime,
                ExpectedEnd = window.EndTime,
                ExpectedBreakStart = window.BreakStartTime,
                ExpectedBreakEnd = window.BreakEndTime,
                BreakMinutes = window.BreakMinutes,
                IsScheduledWorkDay = window.IsScheduledWorkDay,
                ScheduleSource = window.Source,
                BranchName = branchName,
                AllowedRadiusMeters = allowedRadiusMeters
            };
        }

        public static MobileMonthDayDto ToMonthDayDto(TimekeepingEntity entity)
        {
            return new()
            {
                WorkDate = entity.WorkDate,
                Status = entity.Status,
                CheckInAt = entity.CheckInAt,
                CheckOutAt = entity.CheckOutAt,
                WorkedMinutes = entity.WorkedMinutes,
                LateMinutes = entity.LateMinutes,
                EarlyMinutes = entity.EarlyMinutes,
                Note = entity.Note
            };
        }

        public static object ToLogObject(TimekeepingEntity entity)
        {
            return new
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
                entity.OtMinutes,
                entity.NightMinutes,
                entity.Note,
                entity.IsManualAdjusted
            };
        }
    }

    public class ManualAdjustTimekeepingFields
    {
        public Guid Id { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public string? Note { get; set; }
        public AttendanceStatus? Status { get; set; }
    }
}
