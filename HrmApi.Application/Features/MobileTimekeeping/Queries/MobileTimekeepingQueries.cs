using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
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
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

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
                // reflect leave in response even if record not yet updated
            }

            return TimekeepingMapper.ToTodayDto(record, today, onLeave, window.StartTime, window.EndTime, branchName, standard.AllowedRadiusMeters);
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

        public GetMobileMonthQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<MobileMonthDto> Handle(GetMobileMonthQuery request, CancellationToken cancellationToken)
        {
            if (request.Year < 2000 || request.Month < 1 || request.Month > 12)
            {
                throw new InvalidOperationException("Năm/tháng không hợp lệ.");
            }

            Guid employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            DateOnly from = new(request.Year, request.Month, 1);
            DateOnly to = from.AddMonths(1).AddDays(-1);

            List<TimekeepingEntity> records = await _context.TimekeepingEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted && x.WorkDate >= from && x.WorkDate <= to)
                .OrderBy(x => x.WorkDate)
                .ToListAsync(cancellationToken);

            List<MobileMonthDayDto> days = records.Select(TimekeepingMapper.ToMonthDayDto).ToList();

            return new MobileMonthDto
            {
                Year = request.Year,
                Month = request.Month,
                Days = days,
                OnTimeDays = days.Count(d => d.Status == AttendanceStatus.ON_TIME.ToString()),
                LateDays = days.Count(d => d.Status == AttendanceStatus.LATE.ToString()),
                EarlyDays = days.Count(d => d.Status == AttendanceStatus.EARLY.ToString()),
                LeaveDays = days.Count(d => d.Status == AttendanceStatus.LEAVE.ToString()),
                AbsentDays = days.Count(d => d.Status == AttendanceStatus.ABSENT.ToString()),
                IncompleteDays = days.Count(d => d.Status == AttendanceStatus.INCOMPLETE.ToString()),
                TotalWorkedMinutes = days.Sum(d => d.WorkedMinutes)
            };
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
