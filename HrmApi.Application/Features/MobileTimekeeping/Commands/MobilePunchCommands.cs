using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.MobileTimekeeping.Commands
{
    public class MobileCheckInCommand : MobilePunchRequest, IRequest<MobileTodayDto> { }

    public class MobileCheckInCommandHandler : IRequestHandler<MobileCheckInCommand, MobileTodayDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAttendanceRuleService _rules;
        private readonly IActionLogService _actionLog;

        public MobileCheckInCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IAttendanceRuleService rules,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _rules = rules;
            _actionLog = actionLog;
        }

        public async Task<MobileTodayDto> Handle(MobileCheckInCommand request, CancellationToken cancellationToken)
        {
            var employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            var employee = await _rules.ResolveEmployeeAsync(employeeId, cancellationToken);
            var today = BusinessDateHelper.Today();

            if (await _rules.HasApprovedLeaveAsync(employee.Id, today, cancellationToken))
                throw new InvalidOperationException("Bạn đang trong ngày nghỉ phép đã duyệt, không thể chấm công.");

            var record = await _rules.GetOrCreateTodayRecordAsync(employee, today, cancellationToken);
            if (record.Status == AttendanceStatus.LEAVE)
                throw new InvalidOperationException("Bạn đang trong ngày nghỉ phép đã duyệt, không thể chấm công.");
            if (record.CheckInAt.HasValue)
                throw new InvalidOperationException("Bạn đã chấm vào ca hôm nay.");

            var window = await _rules.ResolveWorkWindowAsync(employee, today, cancellationToken);
            if (!window.IsScheduledWorkDay)
                throw new InvalidOperationException(
                    "Hôm nay không phải ngày làm theo ca mặc định. Cần lịch ngày (ngoại lệ/OT) nếu muốn chấm công.");

            var branchId = window.BranchId ?? employee.BranchId;
            var standard = await _rules.ResolveStandardAsync(branchId, employee.CompanyId, cancellationToken);

            var branch = branchId.HasValue
                ? await _context.BranchEntities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == branchId.Value, cancellationToken)
                : null;
            if (branch == null)
                throw new InvalidOperationException("Không xác định được chi nhánh chấm công.");

            var distance = _rules.ValidateGeofence(branch.Latitude, branch.Longitude, request.Latitude, request.Longitude, standard.AllowedRadiusMeters);

            record.CheckInAt = DateTime.UtcNow;
            record.CheckInLatitude = request.Latitude;
            record.CheckInLongitude = request.Longitude;
            record.CheckInDistanceM = distance;
            record.BranchId = branchId;
            record.CompanyId = employee.CompanyId;
            record.ShiftId = window.ShiftId;
            record.ShiftMasterId = window.ShiftMasterId;
            record.UpdatedAt = DateTime.UtcNow;

            _rules.ComputeStatus(record, window, standard);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "TimekeepingEntity",
                record.Id,
                null,
                TimekeepingMapper.ToLogObject(record),
                "Mobile check-in");

            return TimekeepingMapper.ToTodayDto(record, today, false, window, branch.Name, standard.AllowedRadiusMeters);
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
                return _currentUser.EmployeeId.Value;

            if (_currentUser.UserId.HasValue)
            {
                var empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                    return empId.Value;
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên. Không thể chấm công.");
        }
    }

    public class MobileCheckOutCommand : MobilePunchRequest, IRequest<MobileTodayDto> { }

    public class MobileCheckOutCommandHandler : IRequestHandler<MobileCheckOutCommand, MobileTodayDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAttendanceRuleService _rules;
        private readonly IActionLogService _actionLog;

        public MobileCheckOutCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IAttendanceRuleService rules,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _rules = rules;
            _actionLog = actionLog;
        }

        public async Task<MobileTodayDto> Handle(MobileCheckOutCommand request, CancellationToken cancellationToken)
        {
            var employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            var employee = await _rules.ResolveEmployeeAsync(employeeId, cancellationToken);
            var today = BusinessDateHelper.Today();

            if (await _rules.HasApprovedLeaveAsync(employee.Id, today, cancellationToken))
                throw new InvalidOperationException("Bạn đang trong ngày nghỉ phép đã duyệt, không thể chấm công.");

            var record = await _context.TimekeepingEntities
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.WorkDate == today && !x.IsDeleted, cancellationToken);
            if (record == null || !record.CheckInAt.HasValue)
                throw new InvalidOperationException("Bạn chưa chấm vào ca hôm nay.");
            if (record.CheckOutAt.HasValue)
                throw new InvalidOperationException("Bạn đã chấm ra ca hôm nay.");

            var window = await _rules.ResolveWorkWindowAsync(employee, today, cancellationToken);
            var branchId = record.BranchId ?? window.BranchId ?? employee.BranchId;
            var standard = await _rules.ResolveStandardAsync(branchId, employee.CompanyId, cancellationToken);

            var branch = branchId.HasValue
                ? await _context.BranchEntities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == branchId.Value, cancellationToken)
                : null;
            if (branch == null)
                throw new InvalidOperationException("Không xác định được chi nhánh chấm công.");

            var distance = _rules.ValidateGeofence(branch.Latitude, branch.Longitude, request.Latitude, request.Longitude, standard.AllowedRadiusMeters);

            record.CheckOutAt = DateTime.UtcNow;
            record.CheckOutLatitude = request.Latitude;
            record.CheckOutLongitude = request.Longitude;
            record.CheckOutDistanceM = distance;
            record.UpdatedAt = DateTime.UtcNow;

            _rules.ComputeStatus(record, window, standard);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "TimekeepingEntity",
                record.Id,
                null,
                TimekeepingMapper.ToLogObject(record),
                "Mobile check-out");

            return TimekeepingMapper.ToTodayDto(record, today, false, window, branch.Name, standard.AllowedRadiusMeters);
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
                return _currentUser.EmployeeId.Value;

            if (_currentUser.UserId.HasValue)
            {
                var empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                    return empId.Value;
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên. Không thể chấm công.");
        }
    }
}
