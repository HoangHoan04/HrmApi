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
                if (schedule.ShiftId.HasValue)
                {
                    ShiftEntity? shift = await _context.ShiftEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == schedule.ShiftId.Value && !x.IsDeleted, cancellationToken);
                    if (shift != null)
                    {
                        return new WorkWindowResult
                        {
                            StartTime = shift.StartTime,
                            EndTime = shift.EndTime,
                            ShiftId = shift.Id,
                            ShiftMasterId = shift.ShiftMasterId,
                            BranchId = schedule.BranchId ?? shift.BranchId ?? employee.BranchId,
                            IsOvernight = shift.EndTime < shift.StartTime
                        };
                    }
                }

                if (schedule.ShiftMasterId.HasValue)
                {
                    ShiftMasterEntity? master = await _context.ShiftMasterEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == schedule.ShiftMasterId.Value && !x.IsDeleted, cancellationToken);
                    if (master != null)
                    {
                        return new WorkWindowResult
                        {
                            StartTime = master.StartTime,
                            EndTime = master.EndTime,
                            ShiftMasterId = master.Id,
                            ShiftId = schedule.ShiftId,
                            BranchId = schedule.BranchId ?? employee.BranchId,
                            IsOvernight = master.IsOvernight || master.EndTime < master.StartTime
                        };
                    }
                }

                return new WorkWindowResult
                {
                    StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(17),
                    ShiftMasterId = schedule.ShiftMasterId,
                    ShiftId = schedule.ShiftId,
                    BranchId = schedule.BranchId ?? employee.BranchId
                };
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
                        return new WorkWindowResult
                        {
                            StartTime = pm.HourWorkingStart.Value,
                            EndTime = pm.HourWorkingEnd.Value,
                            BranchId = employee.BranchId,
                            IsOvernight = pm.HourWorkingEnd.Value < pm.HourWorkingStart.Value
                        };
                    }
                }
            }

            return new WorkWindowResult
            {
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(17),
                BranchId = employee.BranchId
            };
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
                    return new AttendanceStandardResult
                    {
                        StandardId = std.Id,
                        AllowedRadiusMeters = std.AllowedRadiusMeters > 0 ? std.AllowedRadiusMeters : 200,
                        LateGraceMinutes = std.LateGraceMinutes,
                        EarlyLeaveGraceMinutes = std.EarlyLeaveGraceMinutes
                    };
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
                    return new AttendanceStandardResult
                    {
                        StandardId = companyStd.Id,
                        AllowedRadiusMeters = companyStd.AllowedRadiusMeters > 0 ? companyStd.AllowedRadiusMeters : 200,
                        LateGraceMinutes = companyStd.LateGraceMinutes,
                        EarlyLeaveGraceMinutes = companyStd.EarlyLeaveGraceMinutes
                    };
                }
            }

            return new AttendanceStandardResult
            {
                AllowedRadiusMeters = 200,
                LateGraceMinutes = 0,
                EarlyLeaveGraceMinutes = 0
            };
        }

        public double ValidateGeofence(double? branchLat, double? branchLng, double punchLat, double punchLng, int allowedRadiusMeters)
        {
            if (!branchLat.HasValue || !branchLng.HasValue)
            {
                throw new InvalidOperationException("Chi nhánh chưa cấu hình tọa độ GPS. Vui lòng liên hệ Admin.");
            }

            double distance = GeoHelper.HaversineDistanceMeters(branchLat.Value, branchLng.Value, punchLat, punchLng);
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
                return;
            }

            if (!record.CheckInAt.HasValue && !record.CheckOutAt.HasValue)
            {
                record.Status = AttendanceStatus.INCOMPLETE;
                record.LateMinutes = 0;
                record.EarlyMinutes = 0;
                record.WorkedMinutes = 0;
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
                record.WorkedMinutes = Math.Max(0, (int)Math.Round((record.CheckOutAt.Value - record.CheckInAt.Value).TotalMinutes));
                record.Status = record.LateMinutes > 0 ? AttendanceStatus.LATE : record.EarlyMinutes > 0 ? AttendanceStatus.EARLY : AttendanceStatus.ON_TIME;
            }
            else
            {
                record.WorkedMinutes = 0;
                record.Status = record.LateMinutes > 0 ? AttendanceStatus.LATE : AttendanceStatus.INCOMPLETE;
            }
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
