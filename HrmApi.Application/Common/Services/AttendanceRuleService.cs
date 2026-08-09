using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Employee;
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
            var employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken);
            if (employee == null)
                throw new InvalidOperationException("Không tìm thấy nhân viên.");
            return employee;
        }

        public async Task<WorkWindowResult> ResolveWorkWindowAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default)
        {
            var schedule = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employee.Id && x.WorkDate == workDate && !x.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (schedule != null)
            {
                if (schedule.ShiftId.HasValue)
                {
                    var shift = await _context.ShiftEntities.AsNoTracking()
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
                    var master = await _context.ShiftMasterEntities.AsNoTracking()
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
                var positionMasterId = await _context.PositionEntities.AsNoTracking()
                    .Where(x => x.Id == employee.PositionId.Value && !x.IsDeleted)
                    .Select(x => x.PositionMasterId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (positionMasterId.HasValue)
                {
                    var pm = await _context.PositionMasterEntities.AsNoTracking()
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
                var std = await _context.TimeKeepingStandardEntities.AsNoTracking()
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
                var companyStd = await _context.TimeKeepingStandardEntities.AsNoTracking()
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
                throw new InvalidOperationException("Chi nhánh chưa cấu hình tọa độ GPS. Vui lòng liên hệ Admin.");

            var distance = GeoHelper.HaversineDistanceMeters(branchLat.Value, branchLng.Value, punchLat, punchLng);
            if (distance > allowedRadiusMeters)
                throw new InvalidOperationException(
                    $"Bạn đang ngoài phạm vi chấm công ({distance:0}m / cho phép {allowedRadiusMeters}m).");

            return distance;
        }

        public async Task<bool> HasApprovedLeaveAsync(Guid employeeId, DateOnly workDate, CancellationToken cancellationToken = default)
        {
            return await _context.RegisterDayOffEntities.AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employeeId
                    && !x.IsDeleted
                    && x.Status == DayOffStatus.Approved
                    && x.FromDate <= workDate
                    && x.ToDate >= workDate, cancellationToken);
        }

        public void ComputeStatus(TimekeepingEntity record, WorkWindowResult window, AttendanceStandardResult standard)
        {
            if (record.Status == AttendanceStatus.Leave)
                return;

            if (!record.CheckInAt.HasValue && !record.CheckOutAt.HasValue)
            {
                record.Status = AttendanceStatus.Incomplete;
                record.LateMinutes = 0;
                record.EarlyMinutes = 0;
                record.WorkedMinutes = 0;
                return;
            }

            var expectedStart = record.WorkDate.ToDateTime(TimeOnly.FromTimeSpan(window.StartTime), DateTimeKind.Utc);
            var expectedEnd = record.WorkDate.ToDateTime(TimeOnly.FromTimeSpan(window.EndTime), DateTimeKind.Utc);
            if (window.IsOvernight || window.EndTime < window.StartTime)
                expectedEnd = expectedEnd.AddDays(1);

            if (record.CheckInAt.HasValue)
            {
                var lateThreshold = expectedStart.AddMinutes(standard.LateGraceMinutes);
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
                var earlyThreshold = expectedEnd.AddMinutes(-standard.EarlyLeaveGraceMinutes);
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
                if (record.LateMinutes > 0)
                    record.Status = AttendanceStatus.Late;
                else if (record.EarlyMinutes > 0)
                    record.Status = AttendanceStatus.Early;
                else
                    record.Status = AttendanceStatus.OnTime;
            }
            else
            {
                record.WorkedMinutes = 0;
                // Có check-in nhưng chưa check-out: đánh LATE nếu trễ, còn lại INCOMPLETE
                record.Status = record.LateMinutes > 0 ? AttendanceStatus.Late : AttendanceStatus.Incomplete;
            }
        }

        public async Task<TimekeepingEntity> GetOrCreateTodayRecordAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default)
        {
            var existing = await _context.TimekeepingEntities
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.WorkDate == workDate && !x.IsDeleted, cancellationToken);
            if (existing != null)
                return existing;

            var window = await ResolveWorkWindowAsync(employee, workDate, cancellationToken);
            var onLeave = await HasApprovedLeaveAsync(employee.Id, workDate, cancellationToken);

            var entity = new TimekeepingEntity
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
                BranchId = window.BranchId ?? employee.BranchId,
                WorkDate = workDate,
                ShiftId = window.ShiftId,
                ShiftMasterId = window.ShiftMasterId,
                Status = onLeave ? AttendanceStatus.Leave : AttendanceStatus.Incomplete,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.TimekeepingEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }
    }
}
