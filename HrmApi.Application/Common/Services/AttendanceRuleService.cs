using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Common.Services
{
    public class AttendanceRuleService : IAttendanceRuleService
    {
        private readonly IApplicationDbContext _context;

        public AttendanceRuleService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeEntity> ResolveEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            EmployeeEntity? employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken);
            return employee ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");
        }

        public async Task<WorkWindowResult> ResolveWorkWindowAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default)
        {
            WorkScheduledEmployeeEntity? schedule = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employee.Id && x.WorkDate == workDate && !x.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (schedule != null)
            {
                WorkWindowResult dayWindow = await ResolveFromDayScheduleAsync(schedule, employee, cancellationToken);
                dayWindow.Source = AttendanceScheduleSource.DayOverride;
                dayWindow.IsScheduledWorkDay = true;
                return dayWindow;
            }

            EmployeeWorkPatternEntity? pattern = await _context.EmployeeWorkPatternEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employee.Id
                    && !x.IsDeleted
                    && x.IsActive
                    && x.PatternType == WorkPatternType.FIXED_WEEKLY
                    && x.EffectiveFrom <= workDate
                    && (x.EffectiveTo == null || x.EffectiveTo >= workDate))
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefaultAsync(cancellationToken);

            if (pattern != null)
            {
                ShiftMasterEntity master = await _context.ShiftMasterEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == pattern.ShiftMasterId && !x.IsDeleted, cancellationToken)
                    ?? throw new InvalidOperationException(
                        "Ca mặc định đang trỏ tới ShiftMaster không tồn tại. Kiểm tra Admin → Ca mặc định.");

                WorkWindowResult patternWindow = WithBreak(new WorkWindowResult
                {
                    StartTime = master.StartTime,
                    EndTime = master.EndTime,
                    ShiftMasterId = master.Id,
                    BranchId = pattern.BranchId ?? employee.BranchId,
                    IsOvernight = master.IsOvernight || master.EndTime < master.StartTime,
                    Source = AttendanceScheduleSource.WorkPattern,
                    IsScheduledWorkDay = WorkPatternHelper.IsWorkDay(pattern, workDate),
                }, master);

                return !patternWindow.IsScheduledWorkDay ? patternWindow : patternWindow;
            }

            if (employee.PositionId.HasValue)
            {
                Guid? positionMasterId = await _context.PositionEntities.AsNoTracking()
                    .Where(x => x.Id == employee.PositionId.Value && !x.IsDeleted)
                    .Select(x => x.PositionMasterId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (positionMasterId.HasValue)
                {
                    PositionMasterEntity? pm = await _context.PositionMasterEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == positionMasterId.Value && !x.IsDeleted, cancellationToken);
                    if (pm != null && pm.HourWorkingStart.HasValue && pm.HourWorkingEnd.HasValue)
                    {
                        DayOfWeek dow = workDate.DayOfWeek;
                        bool weekday = dow is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
                        return new WorkWindowResult
                        {
                            StartTime = pm.HourWorkingStart.Value,
                            EndTime = pm.HourWorkingEnd.Value,
                            BranchId = employee.BranchId,
                            IsOvernight = pm.HourWorkingEnd.Value < pm.HourWorkingStart.Value,
                            Source = AttendanceScheduleSource.Position,
                            IsScheduledWorkDay = weekday,
                        };
                    }
                }
            }

            throw new InvalidOperationException(
                "Chưa gán ca mặc định (Work Pattern) hoặc lịch ngày cho nhân viên. Admin → Ca/lịch → Ca mặc định.");
        }

        private async Task<WorkWindowResult> ResolveFromDayScheduleAsync(
            WorkScheduledEmployeeEntity schedule,
            EmployeeEntity employee,
            CancellationToken cancellationToken)
        {
            if (schedule.ShiftId.HasValue)
            {
                ShiftEntity? shift = await _context.ShiftEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == schedule.ShiftId.Value && !x.IsDeleted, cancellationToken);
                if (shift != null)
                {
                    ShiftMasterEntity? shiftMaster = await _context.ShiftMasterEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == shift.ShiftMasterId && !x.IsDeleted, cancellationToken);
                    return WithBreak(new WorkWindowResult
                    {
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,
                        ShiftId = shift.Id,
                        ShiftMasterId = shift.ShiftMasterId,
                        BranchId = schedule.BranchId ?? shift.BranchId ?? employee.BranchId,
                        IsOvernight = shift.EndTime < shift.StartTime,
                    }, shiftMaster);
                }
            }

            if (schedule.ShiftMasterId.HasValue)
            {
                ShiftMasterEntity? master = await _context.ShiftMasterEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == schedule.ShiftMasterId.Value && !x.IsDeleted, cancellationToken);
                if (master != null)
                {
                    return WithBreak(new WorkWindowResult
                    {
                        StartTime = master.StartTime,
                        EndTime = master.EndTime,
                        ShiftMasterId = master.Id,
                        ShiftId = schedule.ShiftId,
                        BranchId = schedule.BranchId ?? employee.BranchId,
                        IsOvernight = master.IsOvernight || master.EndTime < master.StartTime,
                    }, master);
                }
            }

            throw new InvalidOperationException(
                "Lịch ngày chưa gắn ca (ShiftMaster). Vào Admin → Ca/lịch → Lịch theo ngày.");
        }

        private static WorkWindowResult WithBreak(WorkWindowResult window, ShiftMasterEntity? master)
        {
            if (master == null)
            {
                return window;
            }

            window.BreakStartTime = master.BreakStartTime;
            window.BreakEndTime = master.BreakEndTime;
            window.BreakMinutes = master.BreakMinutes;
            if (window.BreakMinutes <= 0 && master.BreakStartTime.HasValue && master.BreakEndTime.HasValue)
            {
                int mins = (int)(master.BreakEndTime.Value - master.BreakStartTime.Value).TotalMinutes;
                if (mins < 0)
                {
                    mins += 24 * 60;
                }

                window.BreakMinutes = Math.Max(0, mins);
            }

            return window;
        }

        public async Task<AttendanceStandardResult> ResolveStandardAsync(Guid? branchId, Guid? companyId, CancellationToken cancellationToken = default)
        {
            Guid? standardId = null;

            if (branchId.HasValue)
            {
                standardId = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == branchId.Value)
                    .Select(x => x.TimeKeepingStandardId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (!standardId.HasValue && companyId.HasValue)
            {
                standardId = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == companyId.Value)
                    .Select(x => x.TimeKeepingStandardId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (standardId.HasValue)
            {
                TimeKeepingStandardEntity? std = await _context.TimeKeepingStandardEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == standardId.Value && !x.IsDeleted, cancellationToken);
                if (std != null)
                {
                    return ToStandardResult(std);
                }
            }

            if (companyId.HasValue)
            {
                TimeKeepingStandardEntity? companyStd = await _context.TimeKeepingStandardEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive && x.CompanyId == companyId)
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (companyStd != null)
                {
                    return ToStandardResult(companyStd);
                }
            }

            return new AttendanceStandardResult
            {
                AllowedRadiusMeters = 200,
                LateGraceMinutes = 0,
                EarlyLeaveGraceMinutes = 0,
                NightStartTime = new TimeSpan(22, 0, 0),
                NightEndTime = new TimeSpan(6, 0, 0),
            };
        }

        private static AttendanceStandardResult ToStandardResult(TimeKeepingStandardEntity std)
        {
            return new()
            {
                StandardId = std.Id,
                AllowedRadiusMeters = std.AllowedRadiusMeters > 0 ? std.AllowedRadiusMeters : 200,
                LateGraceMinutes = std.LateGraceMinutes,
                EarlyLeaveGraceMinutes = std.EarlyLeaveGraceMinutes,
                NightStartTime = std.NightStartTime,
                NightEndTime = std.NightEndTime,
            };
        }

        public double ValidateGeofence(double? siteLat, double? siteLng, double punchLat, double punchLng, int allowedRadiusMeters)
        {
            if (!siteLat.HasValue || !siteLng.HasValue)
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình tọa độ GPS (chi nhánh hoặc công ty). Vui lòng liên hệ Admin.");
            }

            double distance = GeoHelper.HaversineDistanceMeters(siteLat.Value, siteLng.Value, punchLat, punchLng);
            return distance > allowedRadiusMeters
                ? throw new InvalidOperationException(
                    $"Bạn đang ngoài phạm vi chấm công ({distance:0}m / cho phép {allowedRadiusMeters}m).")
                : distance;
        }

        public async Task<bool> HasApprovedLeaveAsync(Guid employeeId, DateOnly workDate, CancellationToken cancellationToken = default)
        {
            return await _context.RegisterDayOffEntities.AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employeeId
                    && !x.IsDeleted
                    && x.Status == DayOffStatus.APPROVED
                    && x.FromDate <= workDate
                    && x.ToDate >= workDate, cancellationToken);
        }

        public void ComputeStatus(TimekeepingEntity record, WorkWindowResult window, AttendanceStandardResult standard)
        {
            if (record.Status == AttendanceStatus.LEAVE)
            {
                record.OtMinutes = 0;
                record.NightMinutes = 0;
                return;
            }

            if (!record.CheckInAt.HasValue && !record.CheckOutAt.HasValue)
            {
                record.Status = AttendanceStatus.INCOMPLETE;
                record.LateMinutes = 0;
                record.EarlyMinutes = 0;
                record.WorkedMinutes = 0;
                record.OtMinutes = 0;
                record.NightMinutes = 0;
                return;
            }

            DateTime expectedStart = BusinessDateHelper.ToUtc(record.WorkDate, window.StartTime);
            DateTime expectedEnd = BusinessDateHelper.ToUtc(record.WorkDate, window.EndTime);
            if (window.IsOvernight || window.EndTime < window.StartTime)
            {
                expectedEnd = expectedEnd.AddDays(1);
            }

            if (record.CheckInAt.HasValue)
            {
                DateTime lateThreshold = expectedStart.AddMinutes(standard.LateGraceMinutes);
                record.LateMinutes = record.CheckInAt.Value > lateThreshold
                    ? Math.Max(0, (int)Math.Round((record.CheckInAt.Value - expectedStart).TotalMinutes))
                    : 0;
            }
            else
            {
                record.LateMinutes = 0;
            }

            if (record.CheckOutAt.HasValue)
            {
                DateTime earlyThreshold = expectedEnd.AddMinutes(-standard.EarlyLeaveGraceMinutes);
                record.EarlyMinutes = record.CheckOutAt.Value < earlyThreshold
                    ? Math.Max(0, (int)Math.Round((expectedEnd - record.CheckOutAt.Value).TotalMinutes))
                    : 0;
            }
            else
            {
                record.EarlyMinutes = 0;
            }

            if (record.CheckInAt.HasValue && record.CheckOutAt.HasValue)
            {
                record.WorkedMinutes = WorkedMinutesCalculator.Compute(
                    record.WorkDate,
                    record.CheckInAt.Value,
                    record.CheckOutAt.Value,
                    window);
                record.Status = record.LateMinutes > 0
                    ? AttendanceStatus.LATE
                    : record.EarlyMinutes > 0
                        ? AttendanceStatus.EARLY
                        : AttendanceStatus.ON_TIME;
            }
            else
            {
                record.WorkedMinutes = 0;
                record.OtMinutes = 0;
                record.NightMinutes = 0;
                record.Status = record.LateMinutes > 0 ? AttendanceStatus.LATE : AttendanceStatus.INCOMPLETE;
            }
        }

        public async Task FinalizeOtAndNightAsync(
            TimekeepingEntity record,
            WorkWindowResult window,
            AttendanceStandardResult standard,
            CancellationToken cancellationToken = default)
        {
            if (record.Status == AttendanceStatus.LEAVE
                || !record.CheckInAt.HasValue
                || !record.CheckOutAt.HasValue)
            {
                record.OtMinutes = 0;
                record.NightMinutes = 0;
                return;
            }

            DateTime inAt = NormalizeUtc(record.CheckInAt.Value);
            DateTime outAt = NormalizeUtc(record.CheckOutAt.Value);
            if (outAt <= inAt)
            {
                record.OtMinutes = 0;
                record.NightMinutes = 0;
                return;
            }

            List<OvertimeRequestEntity> approvedOt = await _context.OvertimeRequestEntities.AsNoTracking()
                .Where(x =>
                    x.EmployeeId == record.EmployeeId
                    && x.WorkDate == record.WorkDate
                    && !x.IsDeleted
                    && x.Status == OvertimeRequestStatus.Approved)
                .ToListAsync(cancellationToken);

            int otTotal = 0;
            foreach (OvertimeRequestEntity? ot in approvedOt)
            {
                DateTime otStart = BusinessDateHelper.ToUtc(record.WorkDate, ot.FromTime);
                DateTime otEnd = BusinessDateHelper.ToUtc(record.WorkDate, ot.ToTime);
                if (otEnd <= otStart)
                {
                    otEnd = otEnd.AddDays(1);
                }

                int overlap = OverlapMinutes(inAt, outAt, otStart, otEnd);
                int cap = ot.ApprovedMinutes ?? ot.RequestedMinutes;
                if (cap > 0)
                {
                    overlap = Math.Min(overlap, cap);
                }

                otTotal += overlap;
            }

            record.OtMinutes = Math.Max(0, otTotal);

            DateTime nightStart = BusinessDateHelper.ToUtc(record.WorkDate, standard.NightStartTime);
            DateTime nightEnd = BusinessDateHelper.ToUtc(record.WorkDate, standard.NightEndTime);
            if (nightEnd <= nightStart)
            {
                nightEnd = nightEnd.AddDays(1);
            }

            int night = OverlapMinutes(inAt, outAt, nightStart, nightEnd);
            if (window.IsOvernight || window.EndTime < window.StartTime)
            {
                DateTime prevNightStart = BusinessDateHelper.ToUtc(record.WorkDate.AddDays(-1), standard.NightStartTime);
                DateTime prevNightEnd = BusinessDateHelper.ToUtc(record.WorkDate, standard.NightEndTime);
                if (prevNightEnd <= prevNightStart)
                {
                    prevNightEnd = prevNightEnd.AddDays(1);
                }

                night += OverlapMinutes(inAt, outAt, prevNightStart, prevNightEnd);
            }

            record.NightMinutes = Math.Max(0, night);
        }

        private static int OverlapMinutes(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            DateTime start = aStart > bStart ? aStart : bStart;
            DateTime end = aEnd < bEnd ? aEnd : bEnd;
            return end <= start ? 0 : Math.Max(0, (int)Math.Floor((end - start).TotalSeconds / 60.0));
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            };
        }

        public async Task<TimekeepingEntity> GetOrCreateTodayRecordAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default)
        {
            TimekeepingEntity? existing = await _context.TimekeepingEntities
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.WorkDate == workDate && !x.IsDeleted, cancellationToken);
            if (existing != null)
            {
                return existing;
            }

            WorkWindowResult window = await ResolveWorkWindowAsync(employee, workDate, cancellationToken);
            bool onLeave = await HasApprovedLeaveAsync(employee.Id, workDate, cancellationToken);

            TimekeepingEntity entity = new()
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
                BranchId = window.BranchId ?? employee.BranchId,
                WorkDate = workDate,
                ShiftId = window.ShiftId,
                ShiftMasterId = window.ShiftMasterId,
                Status = onLeave ? AttendanceStatus.LEAVE : AttendanceStatus.INCOMPLETE,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _ = _context.TimekeepingEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }
    }
}
