using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.MobileTimekeeping.Queries
{
    public class GetMobileTodayQuery : IRequest<MobileTodayDto> { }

    public class GetMobileTodayQueryHandler : IRequestHandler<GetMobileTodayQuery, MobileTodayDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAttendanceRuleService _rules;

        public GetMobileTodayQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IAttendanceRuleService rules)
        {
            _context = context;
            _currentUser = currentUser;
            _rules = rules;
        }

        public async Task<MobileTodayDto> Handle(GetMobileTodayQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            EmployeeEntity employee = await _rules.ResolveEmployeeAsync(employeeId, cancellationToken);
            DateOnly today = BusinessDateHelper.Today();

            bool onLeave = await _rules.HasApprovedLeaveAsync(employee.Id, today, cancellationToken);
            WorkWindowResult window = await _rules.ResolveWorkWindowAsync(employee, today, cancellationToken);
            Guid? branchId = window.BranchId ?? employee.BranchId;
            AttendanceStandardResult standard = await _rules.ResolveStandardAsync(branchId, employee.CompanyId, cancellationToken);

            string? branchName = null;
            if (branchId.HasValue)
            {
                branchName = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == branchId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            TimekeepingEntity? record = await _context.TimekeepingEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.WorkDate == today && !x.IsDeleted, cancellationToken);

            if (onLeave && record != null && record.Status != AttendanceStatus.LEAVE)
            {
                //! reflect leave in response even if record not yet updated
            }

            return TimekeepingMapper.ToTodayDto(record, today, onLeave, window, branchName, standard.AllowedRadiusMeters);
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
            {
                return _currentUser.EmployeeId.Value;
            }

            if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId.Value;
                }
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }

    public class GetMobileMonthQuery : IRequest<MobileMonthDto>
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class GetMobileMonthQueryHandler : IRequestHandler<GetMobileMonthQuery, MobileMonthDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAttendanceRuleService _rules;

        public GetMobileMonthQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IAttendanceRuleService rules)
        {
            _context = context;
            _currentUser = currentUser;
            _rules = rules;
        }

        public async Task<MobileMonthDto> Handle(GetMobileMonthQuery request, CancellationToken cancellationToken)
        {
            if (request.Year < 2000 || request.Month < 1 || request.Month > 12)
            {
                throw new InvalidOperationException("Năm/tháng không hợp lệ.");
            }

            Guid employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            EmployeeEntity employee = await _rules.ResolveEmployeeAsync(employeeId, cancellationToken);
            DateOnly from = new(request.Year, request.Month, 1);
            DateOnly to = from.AddMonths(1).AddDays(-1);

            List<TimekeepingEntity> records = await _context.TimekeepingEntities
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted && x.WorkDate >= from && x.WorkDate <= to)
                .OrderBy(x => x.WorkDate)
                .ToListAsync(cancellationToken);

            bool dirty = false;
            List<MobileMonthDayDto> days = new();
            foreach (TimekeepingEntity record in records)
            {
                if (record.CheckInAt.HasValue && record.CheckOutAt.HasValue)
                {
                    WorkWindowResult? dayWindow = null;
                    try
                    {
                        dayWindow = await _rules.ResolveWorkWindowAsync(employee, record.WorkDate, cancellationToken);
                    }
                    catch
                    {
                        if (record.ShiftMasterId.HasValue)
                        {
                            var master = await _context.ShiftMasterEntities.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == record.ShiftMasterId.Value && !x.IsDeleted, cancellationToken);
                            if (master != null)
                            {
                                dayWindow = new WorkWindowResult
                                {
                                    StartTime = master.StartTime,
                                    EndTime = master.EndTime,
                                    BreakStartTime = master.BreakStartTime,
                                    BreakEndTime = master.BreakEndTime,
                                    BreakMinutes = master.BreakMinutes,
                                    ShiftMasterId = master.Id,
                                    IsOvernight = master.IsOvernight || master.EndTime < master.StartTime,
                                };
                            }
                        }
                    }

                    int computed = WorkedMinutesCalculator.Compute(
                        record.WorkDate,
                        record.CheckInAt.Value,
                        record.CheckOutAt.Value,
                        dayWindow);
                    if (record.WorkedMinutes != computed)
                    {
                        record.WorkedMinutes = computed;
                        record.UpdatedAt = DateTime.UtcNow;
                        dirty = true;
                    }
                }

                days.Add(TimekeepingMapper.ToMonthDayDto(record));
            }

            if (dirty)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            WorkWindowResult? window = await ResolveMonthReferenceWindowAsync(
                employee,
                employeeId,
                from,
                to,
                cancellationToken);

            int dailyExpectedMinutes = window != null
                ? await ResolveDailyExpectedMinutesAsync(window, cancellationToken)
                : 0;
            int expectedWorkingDays = await ResolveExpectedWorkingDaysAsync(employeeId, from, to, cancellationToken);
            int expectedWorkedMinutes = Math.Max(0, dailyExpectedMinutes) * Math.Max(0, expectedWorkingDays);

            return new MobileMonthDto
            {
                Year = request.Year,
                Month = request.Month,
                Days = days,
                OnTimeDays = days.Count(d => d.Status == AttendanceStatus.ON_TIME),
                LateDays = days.Count(d => d.Status == AttendanceStatus.LATE),
                EarlyDays = days.Count(d => d.Status == AttendanceStatus.EARLY),
                LeaveDays = days.Count(d => d.Status == AttendanceStatus.LEAVE),
                AbsentDays = days.Count(d => d.Status == AttendanceStatus.ABSENT),
                IncompleteDays = days.Count(d => d.Status == AttendanceStatus.INCOMPLETE),
                TotalWorkedMinutes = days.Sum(d => d.WorkedMinutes),
                ExpectedWorkingDays = expectedWorkingDays,
                DailyExpectedMinutes = dailyExpectedMinutes,
                ExpectedWorkedMinutes = expectedWorkedMinutes,
            };
        }

        private async Task<WorkWindowResult?> ResolveMonthReferenceWindowAsync(
            EmployeeEntity employee,
            Guid employeeId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken)
        {
            List<DateOnly> candidates = new();

            DateOnly today = BusinessDateHelper.Today();
            if (today >= from && today <= to)
            {
                candidates.Add(today);
            }

            DateOnly? patternDay = await _context.EmployeeWorkPatternEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId
                    && !x.IsDeleted
                    && x.IsActive
                    && x.PatternType == WorkPatternType.FIXED_WEEKLY
                    && x.EffectiveFrom <= to
                    && (x.EffectiveTo == null || x.EffectiveTo >= from))
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => (DateOnly?)(x.EffectiveFrom < from ? from : x.EffectiveFrom))
                .FirstOrDefaultAsync(cancellationToken);
            if (patternDay.HasValue && patternDay.Value <= to)
            {
                candidates.Add(patternDay.Value);
            }

            DateOnly? scheduledDay = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted && x.WorkDate >= from && x.WorkDate <= to)
                .OrderBy(x => x.WorkDate)
                .Select(x => (DateOnly?)x.WorkDate)
                .FirstOrDefaultAsync(cancellationToken);
            if (scheduledDay.HasValue)
            {
                candidates.Add(scheduledDay.Value);
            }

            candidates.Add(from);

            foreach (DateOnly day in candidates.Distinct())
            {
                try
                {
                    return await _rules.ResolveWorkWindowAsync(employee, day, cancellationToken);
                }
                catch
                {
                    //! try next candidate
                }
            }

            return null;
        }

        private async Task<int> ResolveDailyExpectedMinutesAsync(
            WorkWindowResult window,
            CancellationToken cancellationToken)
        {
            int breakMinutes = 0;
            int configuredWorkingMinutes = 0;

            if (window.ShiftMasterId.HasValue)
            {
                var master = await _context.ShiftMasterEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == window.ShiftMasterId.Value && !x.IsDeleted, cancellationToken);
                if (master != null)
                {
                    breakMinutes = Math.Max(0, master.BreakMinutes);
                    configuredWorkingMinutes = Math.Max(0, master.WorkingMinutes);
                }
            }
            else if (window.ShiftId.HasValue)
            {
                var shift = await _context.ShiftEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == window.ShiftId.Value && !x.IsDeleted, cancellationToken);
                if (shift?.ShiftMasterId is Guid masterId)
                {
                    var master = await _context.ShiftMasterEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == masterId && !x.IsDeleted, cancellationToken);
                    if (master != null)
                    {
                        breakMinutes = Math.Max(0, master.BreakMinutes);
                        configuredWorkingMinutes = Math.Max(0, master.WorkingMinutes);
                    }
                }
            }

            if (configuredWorkingMinutes > 0)
            {
                return configuredWorkingMinutes;
            }

            TimeSpan span = window.IsOvernight || window.EndTime < window.StartTime
                ? window.EndTime.Add(TimeSpan.FromDays(1)) - window.StartTime
                : window.EndTime - window.StartTime;

            if (breakMinutes <= 0)
            {
                breakMinutes = window.BreakMinutes;
            }
            if (breakMinutes <= 0 && window.BreakStartTime.HasValue && window.BreakEndTime.HasValue)
            {
                breakMinutes = (int)(window.BreakEndTime.Value - window.BreakStartTime.Value).TotalMinutes;
                if (breakMinutes < 0) breakMinutes += 24 * 60;
            }

            return Math.Max(0, (int)Math.Round(span.TotalMinutes) - Math.Max(0, breakMinutes));
        }

        private async Task<int> ResolveExpectedWorkingDaysAsync(
            Guid employeeId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken)
        {
            HashSet<DateOnly> dayOverrides = (await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employeeId
                    && !x.IsDeleted
                    && x.WorkDate >= from
                    && x.WorkDate <= to)
                .Select(x => x.WorkDate)
                .Distinct()
                .ToListAsync(cancellationToken)).ToHashSet();

            List<EmployeeWorkPatternEntity> patterns = await _context.EmployeeWorkPatternEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId
                    && !x.IsDeleted
                    && x.IsActive
                    && x.PatternType == WorkPatternType.FIXED_WEEKLY
                    && x.EffectiveFrom <= to
                    && (x.EffectiveTo == null || x.EffectiveTo >= from))
                .ToListAsync(cancellationToken);

            HashSet<DateOnly> holidays = (await _context.PublicHolidayEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.HolidayDate >= from
                    && x.HolidayDate <= to)
                .Select(x => x.HolidayDate)
                .ToListAsync(cancellationToken)).ToHashSet();

            List<PublicHolidayEntity> recurring = await _context.PublicHolidayEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsRecurringYearly)
                .ToListAsync(cancellationToken);
            for (DateOnly d = from; d <= to; d = d.AddDays(1))
            {
                if (recurring.Any(h => h.HolidayDate.Month == d.Month && h.HolidayDate.Day == d.Day))
                {
                    holidays.Add(d);
                }
            }

            int count = 0;
            bool hasPatternOrSchedule = patterns.Count > 0 || dayOverrides.Count > 0;

            for (DateOnly d = from; d <= to; d = d.AddDays(1))
            {
                if (holidays.Contains(d))
                {
                    continue;
                }

                if (dayOverrides.Contains(d))
                {
                    count++;
                    continue;
                }

                if (patterns.Any(p => WorkPatternHelper.IsWorkDay(p, d)))
                {
                    count++;
                    continue;
                }

                if (!hasPatternOrSchedule
                    && d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                {
                    count++;
                }
            }

            return count;
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
            {
                return _currentUser.EmployeeId.Value;
            }

            if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId.Value;
                }
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }
}
