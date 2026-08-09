using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Timekeepings.Commands
{
    public class ManualAdjustTimekeepingCommand : ManualAdjustTimekeepingFields, IRequest<bool> { }

    public class ManualAdjustTimekeepingCommandHandler : IRequestHandler<ManualAdjustTimekeepingCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly IAttendanceRuleService _rules;

        public ManualAdjustTimekeepingCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IAttendanceRuleService rules)
        {
            _context = context;
            _actionLog = actionLog;
            _rules = rules;
        }

        public async Task<bool> Handle(ManualAdjustTimekeepingCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.TimekeepingEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            var oldValue = TimekeepingMapper.ToLogObject(entity);

            if (request.CheckInAt.HasValue) entity.CheckInAt = request.CheckInAt;
            if (request.CheckOutAt.HasValue) entity.CheckOutAt = request.CheckOutAt;
            if (request.Note != null) entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                entity.Status = request.Status.Trim();
            }
            else
            {
                var employee = await _rules.ResolveEmployeeAsync(entity.EmployeeId, cancellationToken);
                var window = await _rules.ResolveWorkWindowAsync(employee, entity.WorkDate, cancellationToken);
                var standard = await _rules.ResolveStandardAsync(entity.BranchId ?? employee.BranchId, employee.CompanyId, cancellationToken);
                _rules.ComputeStatus(entity, window, standard);
            }

            entity.IsManualAdjusted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "TimekeepingEntity",
                entity.Id,
                oldValue,
                TimekeepingMapper.ToLogObject(entity),
                "Admin điều chỉnh chấm công");

            return true;
        }
    }

    public class SummarizeMonthTimekeepingCommand : IRequest<int>
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class SummarizeMonthTimekeepingCommandHandler : IRequestHandler<SummarizeMonthTimekeepingCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly IAttendanceRuleService _rules;

        public SummarizeMonthTimekeepingCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IAttendanceRuleService rules)
        {
            _context = context;
            _actionLog = actionLog;
            _rules = rules;
        }

        public async Task<int> Handle(SummarizeMonthTimekeepingCommand request, CancellationToken cancellationToken)
        {
            if (request.Year < 2000 || request.Month < 1 || request.Month > 12)
                throw new InvalidOperationException("Năm/tháng không hợp lệ.");

            var from = new DateOnly(request.Year, request.Month, 1);
            var to = from.AddMonths(1).AddDays(-1);

            // Employees with schedule or timekeeping in month (heuristic)
            var scheduleEmpIds = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.WorkDate >= from && x.WorkDate <= to)
                .Select(x => x.EmployeeId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var tkEmpIds = await _context.TimekeepingEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.WorkDate >= from && x.WorkDate <= to)
                .Select(x => x.EmployeeId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var employeeIds = scheduleEmpIds.Union(tkEmpIds).Distinct().ToList();

            if (request.CompanyId.HasValue || request.BranchId.HasValue)
            {
                var filtered = _context.EmployeeEntities.AsNoTracking().Where(e => employeeIds.Contains(e.Id));
                if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                    filtered = filtered.Where(e => e.CompanyId == request.CompanyId);
                if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                    filtered = filtered.Where(e => e.BranchId == request.BranchId);
                employeeIds = await filtered.Select(e => e.Id).ToListAsync(cancellationToken);
            }

            var holidays = await _context.PublicHolidayEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .ToListAsync(cancellationToken);

            var holidayDates = new HashSet<DateOnly>();
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                foreach (var h in holidays)
                {
                    if (h.IsRecurringYearly)
                    {
                        if (h.HolidayDate.Month == d.Month && h.HolidayDate.Day == d.Day)
                            holidayDates.Add(d);
                    }
                    else if (h.HolidayDate == d)
                    {
                        holidayDates.Add(d);
                    }
                }
            }

            var summarized = 0;
            foreach (var employeeId in employeeIds)
            {
                var employee = await _context.EmployeeEntities
                    .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken);
                if (employee == null) continue;

                // Mark missing scheduled workdays as ABSENT
                var scheduleDates = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                    .Where(x => x.EmployeeId == employeeId && !x.IsDeleted && x.WorkDate >= from && x.WorkDate <= to)
                    .Select(x => x.WorkDate)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var workDate in scheduleDates)
                {
                    if (holidayDates.Contains(workDate)) continue;

                    var existing = await _context.TimekeepingEntities
                        .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.WorkDate == workDate && !x.IsDeleted, cancellationToken);

                    if (existing == null)
                    {
                        var onLeave = await _rules.HasApprovedLeaveAsync(employeeId, workDate, cancellationToken);
                        var window = await _rules.ResolveWorkWindowAsync(employee, workDate, cancellationToken);
                        _context.TimekeepingEntities.Add(new TimekeepingEntity
                        {
                            EmployeeId = employeeId,
                            CompanyId = employee.CompanyId,
                            BranchId = window.BranchId ?? employee.BranchId,
                            WorkDate = workDate,
                            ShiftId = window.ShiftId,
                            ShiftMasterId = window.ShiftMasterId,
                            Status = onLeave ? AttendanceStatus.Leave : AttendanceStatus.Absent,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                    else if (!existing.CheckInAt.HasValue
                             && existing.Status != AttendanceStatus.Leave
                             && existing.Status != AttendanceStatus.Absent
                             && !existing.IsManualAdjusted)
                    {
                        var onLeave = await _rules.HasApprovedLeaveAsync(employeeId, workDate, cancellationToken);
                        existing.Status = onLeave ? AttendanceStatus.Leave : AttendanceStatus.Absent;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                var records = await _context.TimekeepingEntities.AsNoTracking()
                    .Where(x => x.EmployeeId == employeeId && !x.IsDeleted && x.WorkDate >= from && x.WorkDate <= to)
                    .ToListAsync(cancellationToken);

                var summary = await _context.TimekeepingSummaryEntities
                    .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Year == request.Year && x.Month == request.Month && !x.IsDeleted, cancellationToken);

                if (summary == null)
                {
                    summary = new TimekeepingSummaryEntity
                    {
                        EmployeeId = employeeId,
                        Year = request.Year,
                        Month = request.Month,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    _context.TimekeepingSummaryEntities.Add(summary);
                }

                summary.CompanyId = employee.CompanyId;
                summary.BranchId = employee.BranchId;
                summary.OnTimeDays = records.Count(r => r.Status == AttendanceStatus.OnTime);
                summary.LateDays = records.Count(r => r.Status == AttendanceStatus.Late);
                summary.EarlyDays = records.Count(r => r.Status == AttendanceStatus.Early);
                summary.LeaveDays = records.Count(r => r.Status == AttendanceStatus.Leave);
                summary.AbsentDays = records.Count(r => r.Status == AttendanceStatus.Absent);
                summary.IncompleteDays = records.Count(r => r.Status == AttendanceStatus.Incomplete);
                summary.WorkingDays = records.Count(r =>
                    r.Status == AttendanceStatus.OnTime
                    || r.Status == AttendanceStatus.Late
                    || r.Status == AttendanceStatus.Early);
                summary.TotalWorkedMinutes = records.Sum(r => r.WorkedMinutes);
                summary.TotalLateMinutes = records.Sum(r => r.LateMinutes);
                summary.TotalEarlyMinutes = records.Sum(r => r.EarlyMinutes);
                summary.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                summarized++;
            }

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "TimekeepingSummaryEntity",
                null,
                null,
                new { request.Year, request.Month, request.CompanyId, request.BranchId, Count = summarized },
                $"Tổng hợp công tháng {request.Month}/{request.Year}");

            return summarized;
        }
    }
}
