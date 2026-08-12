using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.RegisterDayOffs.Commands
{
    public class CreateRegisterDayOffCommand : CreateRegisterDayOffRequest, IRequest<Guid>
    {
        public Guid? EmployeeId { get; set; }
    }

    public class CreateRegisterDayOffCommandHandler : IRequestHandler<CreateRegisterDayOffCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public CreateRegisterDayOffCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateRegisterDayOffCommand request, CancellationToken cancellationToken)
        {
            Guid? employeeId = request.EmployeeId;
            if (!employeeId.HasValue || employeeId == Guid.Empty)
            {
                employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            }

            if (request.FromDate == default || request.ToDate == default)
            {
                throw new InvalidOperationException("Từ ngày / đến ngày là bắt buộc.");
            }

            if (request.ToDate < request.FromDate)
            {
                throw new InvalidOperationException("Đến ngày phải lớn hơn hoặc bằng từ ngày.");
            }

            EmployeeEntity? employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == employeeId.Value && !x.IsDeleted, cancellationToken);
            if (employee == null)
            {
                throw new InvalidOperationException("Không tìm thấy nhân viên.");
            }

            DayOffType dayOffType = request.DayOffType ?? DayOffType.ANNUAL;

            if (request.DayOffConfigId.HasValue)
            {
                DayOffConfigEntity? config = await _context.DayOffConfigEntities
                    .FirstOrDefaultAsync(x => x.Id == request.DayOffConfigId.Value && !x.IsDeleted, cancellationToken);
                if (config == null)
                {
                    throw new InvalidOperationException("Cấu hình nghỉ phép không tồn tại.");
                }

                dayOffType = config.DayOffType;
            }

            bool overlap = await _context.RegisterDayOffEntities
                .AnyAsync(x => x.EmployeeId == employeeId.Value
                    && !x.IsDeleted
                    && (x.Status == DayOffStatus.PENDING || x.Status == DayOffStatus.APPROVED)
                    && x.FromDate <= request.ToDate
                    && x.ToDate >= request.FromDate, cancellationToken);
            if (overlap)
            {
                throw new InvalidOperationException("Đã có đơn nghỉ phép trùng khoảng thời gian.");
            }

            int totalDays = request.ToDate.DayNumber - request.FromDate.DayNumber + 1;

            var entity = new RegisterDayOffEntity
            {
                EmployeeId = employeeId.Value,
                CompanyId = employee.CompanyId,
                BranchId = employee.BranchId,
                DayOffConfigId = request.DayOffConfigId,
                DayOffType = dayOffType,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = totalDays,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                Status = DayOffStatus.PENDING,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _ = _context.RegisterDayOffEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "RegisterDayOffEntity",
                entity.Id,
                null,
                RegisterDayOffMapper.ToLogObject(entity),
                "Đăng ký nghỉ phép");

            return entity.Id;
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

    public class ApproveRegisterDayOffCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? ApproverNote { get; set; }
    }

    public class ApproveRegisterDayOffCommandHandler : IRequestHandler<ApproveRegisterDayOffCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;
        private readonly IAttendanceRuleService _rules;

        public ApproveRegisterDayOffCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser,
            IAttendanceRuleService rules)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
            _rules = rules;
        }

        public async Task<bool> Handle(ApproveRegisterDayOffCommand request, CancellationToken cancellationToken)
        {
            RegisterDayOffEntity? entity = await _context.RegisterDayOffEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != DayOffStatus.PENDING)
            {
                throw new InvalidOperationException("Chỉ duyệt đơn đang chờ duyệt.");
            }

            object oldValue = RegisterDayOffMapper.ToLogObject(entity);
            entity.Status = DayOffStatus.APPROVED;
            entity.ApproverId = await ResolveApproverEmployeeIdAsync(cancellationToken);
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApproverNote = string.IsNullOrWhiteSpace(request.ApproverNote) ? null : request.ApproverNote.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            EmployeeEntity employee = await _rules.ResolveEmployeeAsync(entity.EmployeeId, cancellationToken);

            for (DateOnly d = entity.FromDate; d <= entity.ToDate; d = d.AddDays(1))
            {
                // Include soft-deleted to avoid unique (EmployeeId, WorkDate) violation
                TimekeepingEntity? tk = await _context.TimekeepingEntities
                    .FirstOrDefaultAsync(
                        x => x.EmployeeId == entity.EmployeeId && x.WorkDate == d,
                        cancellationToken);

                if (tk != null)
                {
                    tk.IsDeleted = false;
                    if (!tk.CheckInAt.HasValue || tk.IsManualAdjusted == false)
                    {
                        tk.Status = AttendanceStatus.LEAVE;
                        tk.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    WorkWindowResult window = await _rules.ResolveWorkWindowAsync(employee, d, cancellationToken);
                    _ = _context.TimekeepingEntities.Add(new TimekeepingEntity
                    {
                        EmployeeId = entity.EmployeeId,
                        CompanyId = entity.CompanyId ?? employee.CompanyId,
                        BranchId = entity.BranchId ?? window.BranchId ?? employee.BranchId,
                        WorkDate = d,
                        ShiftId = window.ShiftId,
                        ShiftMasterId = window.ShiftMasterId,
                        Status = AttendanceStatus.LEAVE,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "RegisterDayOffEntity",
                entity.Id,
                oldValue,
                RegisterDayOffMapper.ToLogObject(entity),
                "Duyệt đơn nghỉ phép");

            return true;
        }

        private async Task<Guid?> ResolveApproverEmployeeIdAsync(CancellationToken cancellationToken)
        {
            // ApproverId FK → EmployeeEntity (không phải UserId)
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
            {
                bool exists = await _context.EmployeeEntities.AsNoTracking()
                    .AnyAsync(x => x.Id == _currentUser.EmployeeId.Value && !x.IsDeleted, cancellationToken);
                if (exists)
                {
                    return _currentUser.EmployeeId.Value;
                }
            }

            if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId;
                }
            }

            return null;
        }
    }

    public class RejectRegisterDayOffCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? ApproverNote { get; set; }
    }

    public class RejectRegisterDayOffCommandHandler : IRequestHandler<RejectRegisterDayOffCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public RejectRegisterDayOffCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(RejectRegisterDayOffCommand request, CancellationToken cancellationToken)
        {
            RegisterDayOffEntity? entity = await _context.RegisterDayOffEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != DayOffStatus.PENDING)
            {
                throw new InvalidOperationException("Chỉ từ chối đơn đang chờ duyệt.");
            }

            object oldValue = RegisterDayOffMapper.ToLogObject(entity);
            entity.Status = DayOffStatus.REJECTED;
            entity.ApproverId = await ResolveApproverEmployeeIdAsync(cancellationToken);
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApproverNote = string.IsNullOrWhiteSpace(request.ApproverNote) ? null : request.ApproverNote.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "RegisterDayOffEntity",
                entity.Id,
                oldValue,
                RegisterDayOffMapper.ToLogObject(entity),
                "Từ chối đơn nghỉ phép");

            return true;
        }

        private async Task<Guid?> ResolveApproverEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
            {
                bool exists = await _context.EmployeeEntities.AsNoTracking()
                    .AnyAsync(x => x.Id == _currentUser.EmployeeId.Value && !x.IsDeleted, cancellationToken);
                if (exists)
                {
                    return _currentUser.EmployeeId.Value;
                }
            }

            if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId;
                }
            }

            return null;
        }
    }

    public class CancelRegisterDayOffCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class CancelRegisterDayOffCommandHandler : IRequestHandler<CancelRegisterDayOffCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public CancelRegisterDayOffCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(CancelRegisterDayOffCommand request, CancellationToken cancellationToken)
        {
            RegisterDayOffEntity? entity = await _context.RegisterDayOffEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != DayOffStatus.PENDING)
            {
                throw new InvalidOperationException("Chỉ hủy đơn đang chờ duyệt.");
            }

            Guid employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            if (entity.EmployeeId != employeeId)
            {
                throw new InvalidOperationException("Không có quyền hủy đơn này.");
            }

            object oldValue = RegisterDayOffMapper.ToLogObject(entity);
            entity.Status = DayOffStatus.CANCELLED;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "RegisterDayOffEntity",
                entity.Id,
                oldValue,
                RegisterDayOffMapper.ToLogObject(entity),
                "Hủy đơn nghỉ phép");

            return true;
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
