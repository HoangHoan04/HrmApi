using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.RegisterDayOffs.Commands
{
    public class CreateRegisterDayOffCommand : IRequest<Guid>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? DayOffConfigId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public LeaveSession Session { get; set; } = LeaveSession.FULL;
        public string? Reason { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public class CreateRegisterDayOffCommandHandler : IRequestHandler<CreateRegisterDayOffCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IWorkflowEngine _workflow;
        private readonly INotificationService _notificationService;

        public CreateRegisterDayOffCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IWorkflowEngine workflow,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _workflow = workflow;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(CreateRegisterDayOffCommand request, CancellationToken cancellationToken)
        {
            Guid employeeId = request.EmployeeId is { } eid && eid != Guid.Empty
                ? eid
                : await CurrentEmployeeHelper.ResolveAsync(_context, _currentUser, cancellationToken);

            Domain.Entities.Employee.EmployeeEntity employee = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");

            if (!employee.CompanyId.HasValue)
            {
                throw new InvalidOperationException("Nhân viên chưa thuộc công ty.");
            }

            if (request.ToDate < request.FromDate)
            {
                throw new InvalidOperationException("Ngày kết thúc phải >= ngày bắt đầu.");
            }

            if (request.Session is LeaveSession.AM or LeaveSession.PM && request.FromDate != request.ToDate)
            {
                throw new InvalidOperationException("Nghỉ nửa ngày (AM/PM) chỉ áp dụng khi Từ ngày = Đến ngày.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new InvalidOperationException("Vui lòng nhập lý do nghỉ.");
            }

            DayOffConfigEntity? config = request.DayOffConfigId.HasValue
                ? await _context.DayOffConfigEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.DayOffConfigId && !x.IsDeleted && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Không tìm thấy cấu hình loại nghỉ.")
                : await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive
                        && (x.CompanyId == null || x.CompanyId == employee.CompanyId))
                    .OrderByDescending(x => x.CompanyId.HasValue)
                    .ThenBy(x => x.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException("Chưa cấu hình loại nghỉ. Vui lòng chọn DayOffConfig.");
            if (config.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
            {
                throw new InvalidOperationException("Loại nghỉ này bắt buộc đính kèm giấy tờ.");
            }

            if (config.MinNoticeDays > 0)
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
                DateOnly minFrom = today.AddDays(config.MinNoticeDays);
                if (request.FromDate < minFrom)
                {
                    throw new InvalidOperationException($"Phải đăng ký trước ít nhất {config.MinNoticeDays} ngày.");
                }
            }

            decimal totalDays = await LeaveDayCalculator.CountWorkingDaysAsync(
                _context, request.FromDate, request.ToDate, employee.CompanyId, request.Session, cancellationToken);

            if (totalDays <= 0)
            {
                throw new InvalidOperationException("Khoảng ngày chọn không có ngày làm việc (cuối tuần/ngày lễ).");
            }

            if (config.MaxDaysPerRequest.HasValue && totalDays > config.MaxDaysPerRequest.Value)
            {
                throw new InvalidOperationException($"Vượt số ngày tối đa mỗi đơn ({config.MaxDaysPerRequest:0.##}).");
            }

            bool overlap = await _context.RegisterDayOffEntities.AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employeeId &&
                    !x.IsDeleted &&
                    (x.Status == DayOffStatus.PENDING || x.Status == DayOffStatus.APPROVED) &&
                    x.FromDate <= request.ToDate &&
                    x.ToDate >= request.FromDate, cancellationToken);
            if (overlap)
            {
                throw new InvalidOperationException("Khoảng ngày nghỉ bị trùng với đơn khác đang chờ/đã duyệt.");
            }

            await LeaveBalanceHelper.EnsureBalanceAsync(
                _context, employeeId, employee.CompanyId, config, totalDays, request.FromDate.Year, cancellationToken);

            Guid? requestedApproverId = await LeaveApproverResolver.ResolveAsync(_context, employeeId, cancellationToken);

            RegisterDayOffEntity entity = new()
            {
                EmployeeId = employeeId,
                CompanyId = employee.CompanyId,
                BranchId = employee.BranchId,
                DayOffConfigId = config.Id,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Session = request.Session,
                TotalDays = totalDays,
                Reason = request.Reason.Trim(),
                AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim(),
                Status = DayOffStatus.PENDING,
                RequestedApproverId = requestedApproverId,
                CreatedBy = _currentUser.UserId ?? employeeId,
                CreatedAt = DateTime.UtcNow,
            };

            _ = _context.RegisterDayOffEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            _ = await _workflow.StartAsync(
                Domain.Enums.WorkflowEntityType.Leave, entity.Id, entity.CompanyId, cancellationToken);

            _ = await _notificationService.NotifyAdminsAndApproverAsync(
                approverEmployeeId: requestedApproverId,
                title: "Đơn xin nghỉ phép mới",
                content: $"Nhân viên {employee.FullName ?? employee.Code} vừa gửi đơn xin nghỉ phép ({request.FromDate:dd/MM/yyyy} - {request.ToDate:dd/MM/yyyy}, {totalDays:0.##} ngày).",
                type: NotificationType.Leave,
                severity: NotificationSeverity.Info,
                targetUrl: "/operate-manager/leave-manager",
                targetType: "LEAVE_REQUEST",
                targetId: entity.Id,
                companyId: entity.CompanyId,
                senderId: _currentUser.UserId,
                cancellationToken: cancellationToken);

            return entity.Id;
        }
    }

    public class ApproveRegisterDayOffCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? ApproverNote { get; set; }
        public bool AsAdmin { get; set; } = true;
    }

    public class ApproveRegisterDayOffCommandHandler : IRequestHandler<ApproveRegisterDayOffCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IWorkflowEngine _workflow;
        private readonly INotificationService _notificationService;

        public ApproveRegisterDayOffCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IWorkflowEngine workflow,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _workflow = workflow;
            _notificationService = notificationService;
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
                throw new InvalidOperationException("Chỉ có thể duyệt đơn đang chờ duyệt.");
            }

            Guid? actorEmployeeId = _currentUser.EmployeeId;
            await EnsureCanDecideAsync(_context, entity, actorEmployeeId, request.AsAdmin, cancellationToken);

            if (entity.DayOffConfigId.HasValue)
            {
                DayOffConfigEntity? config = await _context.DayOffConfigEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.DayOffConfigId && !x.IsDeleted, cancellationToken);
                if (config != null)
                {
                    decimal remaining = await LeaveBalanceHelper.GetRemainingAsync(
                        _context, entity.EmployeeId, entity.CompanyId, config.Id,
                        entity.FromDate.Year, cancellationToken) + entity.TotalDays;
                    if (config.DeductBalance && entity.TotalDays > remaining)
                    {
                        throw new InvalidOperationException(
                             $"Không đủ quỹ phép (còn {remaining:0.##}, yêu cầu {entity.TotalDays:0.##}).");
                    }
                }
            }

            entity.Status = DayOffStatus.APPROVED;
            entity.ApproverId = actorEmployeeId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApproverNote = string.IsNullOrWhiteSpace(request.ApproverNote) ? null : request.ApproverNote.Trim();
            entity.UpdatedBy = _currentUser.UserId ?? actorEmployeeId;
            entity.UpdatedAt = DateTime.UtcNow;

            await LeaveBalanceHelper.ApplyApprovedUsageAsync(_context, entity, cancellationToken);
            await SyncTimekeepingLeaveAsync(_context, entity, overwritePunches: false, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            _ = await _workflow.AdvanceForEntityAsync(
                Domain.Enums.WorkflowEntityType.Leave,
                entity.Id,
                approve: true,
                request.ApproverNote,
                _currentUser.UserId ?? Guid.Empty,
                actorEmployeeId,
                cancellationToken);

            _ = await _notificationService.CreateNotifyForEmployeeAsync(
                employeeId: entity.EmployeeId,
                title: "Đơn xin nghỉ phép đã được duyệt",
                content: $"Đơn xin nghỉ phép ({entity.FromDate:dd/MM/yyyy} - {entity.ToDate:dd/MM/yyyy}, {entity.TotalDays:0.##} ngày) của bạn đã được phê duyệt.",
                type: NotificationType.Leave,
                severity: NotificationSeverity.Success,
                targetUrl: "/operate-manager/leave-manager",
                targetType: "LEAVE_REQUEST",
                targetId: entity.Id,
                senderId: _currentUser.UserId,
                cancellationToken: cancellationToken);

            return true;
        }

        internal static async Task EnsureCanDecideAsync(
            IApplicationDbContext context,
            RegisterDayOffEntity entity,
            Guid? actorEmployeeId,
            bool asAdminFallback,
            CancellationToken cancellationToken)
        {
            if (!actorEmployeeId.HasValue || actorEmployeeId == Guid.Empty)
            {
                if (asAdminFallback)
                {
                    return;
                }

                throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
            }

            if (entity.RequestedApproverId.HasValue && entity.RequestedApproverId == actorEmployeeId)
            {
                return;
            }

            Guid? currentResolver = await LeaveApproverResolver.ResolveAsync(context, entity.EmployeeId, cancellationToken);
            if (currentResolver.HasValue && currentResolver == actorEmployeeId)
            {
                return;
            }

            if (asAdminFallback)
            {
                return;
            }

            throw new InvalidOperationException("Bạn không có quyền duyệt đơn này.");
        }

        internal static async Task SyncTimekeepingLeaveAsync(
            IApplicationDbContext context,
            RegisterDayOffEntity leave,
            bool overwritePunches,
            CancellationToken cancellationToken)
        {
            List<DateOnly> dates = await LeaveDayCalculator.GetWorkingDatesAsync(
                context, leave.FromDate, leave.ToDate, leave.CompanyId, cancellationToken);

            foreach (DateOnly d in dates)
            {
                TimekeepingEntity? existing = await context.TimekeepingEntities
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == leave.EmployeeId &&
                        x.WorkDate == d &&
                        !x.IsDeleted, cancellationToken);

                if (existing != null)
                {
                    bool hasPunch = existing.CheckInAt != null || existing.CheckOutAt != null;
                    if (hasPunch && !overwritePunches)
                    {
                        if (string.IsNullOrWhiteSpace(existing.Note))
                        {
                            existing.Note = "Có đơn nghỉ đã duyệt";
                        }

                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = leave.UpdatedBy;
                        continue;
                    }

                    existing.Status = AttendanceStatus.LEAVE;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = leave.UpdatedBy;
                }
                else
                {
                    _ = context.TimekeepingEntities.Add(new TimekeepingEntity
                    {
                        EmployeeId = leave.EmployeeId,
                        CompanyId = leave.CompanyId,
                        BranchId = leave.BranchId,
                        WorkDate = d,
                        Status = AttendanceStatus.LEAVE,
                        CreatedBy = leave.UpdatedBy ?? Guid.Empty,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
            }
        }
    }

    public class RejectRegisterDayOffCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? ApproverNote { get; set; }
        public bool AsAdmin { get; set; } = true;
    }

    public class RejectRegisterDayOffCommandHandler : IRequestHandler<RejectRegisterDayOffCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IWorkflowEngine _workflow;
        private readonly INotificationService _notificationService;

        public RejectRegisterDayOffCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IWorkflowEngine workflow,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _workflow = workflow;
            _notificationService = notificationService;
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
                throw new InvalidOperationException("Chỉ có thể từ chối đơn đang chờ duyệt.");
            }

            if (string.IsNullOrWhiteSpace(request.ApproverNote))
            {
                throw new InvalidOperationException("Vui lòng nhập lý do từ chối.");
            }

            await ApproveRegisterDayOffCommandHandler.EnsureCanDecideAsync(
                _context, entity, _currentUser.EmployeeId, request.AsAdmin, cancellationToken);

            entity.Status = DayOffStatus.REJECTED;
            entity.ApproverId = _currentUser.EmployeeId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApproverNote = request.ApproverNote.Trim();
            entity.UpdatedBy = _currentUser.UserId ?? _currentUser.EmployeeId;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            _ = await _workflow.AdvanceForEntityAsync(
                Domain.Enums.WorkflowEntityType.Leave,
                entity.Id,
                approve: false,
                request.ApproverNote,
                _currentUser.UserId ?? Guid.Empty,
                _currentUser.EmployeeId,
                cancellationToken);

            _ = await _notificationService.CreateNotifyForEmployeeAsync(
                employeeId: entity.EmployeeId,
                title: "Đơn xin nghỉ phép bị từ chối",
                content: $"Đơn xin nghỉ phép ({entity.FromDate:dd/MM/yyyy} - {entity.ToDate:dd/MM/yyyy}) của bạn đã bị từ chối. Lý do: {request.ApproverNote}",
                type: NotificationType.Leave,
                severity: NotificationSeverity.Danger,
                targetUrl: "/operate-manager/leave-manager",
                targetType: "LEAVE_REQUEST",
                targetId: entity.Id,
                senderId: _currentUser.UserId,
                cancellationToken: cancellationToken);

            return true;
        }
    }

    public class CancelRegisterDayOffCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool AsAdmin { get; set; }
        public string? CancelReason { get; set; }
    }

    public class CancelRegisterDayOffCommandHandler : IRequestHandler<CancelRegisterDayOffCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CancelRegisterDayOffCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
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

            Guid? actorEmployeeId = _currentUser.EmployeeId;
            if (!request.AsAdmin)
            {
                Guid employeeId = await CurrentEmployeeHelper.ResolveAsync(_context, _currentUser, cancellationToken);
                if (entity.EmployeeId != employeeId)
                {
                    throw new InvalidOperationException("Bạn không có quyền hủy đơn này.");
                }

                if (entity.Status != DayOffStatus.PENDING)
                {
                    throw new InvalidOperationException("Chỉ có thể hủy đơn đang chờ duyệt.");
                }
            }
            else
            {
                if (entity.Status is not (DayOffStatus.PENDING or DayOffStatus.APPROVED))
                {
                    throw new InvalidOperationException("Chỉ có thể hủy đơn đang chờ duyệt hoặc đã duyệt.");
                }

                if (entity.Status == DayOffStatus.APPROVED && entity.FromDate < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    throw new InvalidOperationException("Không thể hủy đơn đã duyệt có ngày bắt đầu trong quá khứ.");
                }

                if (string.IsNullOrWhiteSpace(request.CancelReason))
                {
                    throw new InvalidOperationException("Vui lòng nhập lý do hủy.");
                }
            }

            bool wasApproved = entity.Status == DayOffStatus.APPROVED;
            entity.Status = DayOffStatus.CANCELLED;
            entity.CancelReason = string.IsNullOrWhiteSpace(request.CancelReason) ? null : request.CancelReason.Trim();
            entity.UpdatedBy = _currentUser.UserId ?? actorEmployeeId;
            entity.UpdatedAt = DateTime.UtcNow;

            if (wasApproved)
            {
                await LeaveBalanceHelper.RevertApprovedUsageAsync(_context, entity, cancellationToken);
                await ClearTimekeepingLeaveAsync(entity, cancellationToken);
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task ClearTimekeepingLeaveAsync(RegisterDayOffEntity leave, CancellationToken cancellationToken)
        {
            List<DateOnly> dates = await LeaveDayCalculator.GetWorkingDatesAsync(
                _context, leave.FromDate, leave.ToDate, leave.CompanyId, cancellationToken);

            List<TimekeepingEntity> rows = await _context.TimekeepingEntities
                .Where(x =>
                    x.EmployeeId == leave.EmployeeId &&
                    dates.Contains(x.WorkDate) &&
                    x.Status == AttendanceStatus.LEAVE &&
                    !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (TimekeepingEntity row in rows)
            {
                if (row.CheckInAt == null && row.CheckOutAt == null)
                {
                    row.IsDeleted = true;
                    row.UpdatedAt = DateTime.UtcNow;
                    row.UpdatedBy = leave.UpdatedBy;
                }
                else
                {
                    row.Status = AttendanceStatus.INCOMPLETE;
                    row.UpdatedAt = DateTime.UtcNow;
                    row.UpdatedBy = leave.UpdatedBy;
                }
            }
        }
    }

    internal static class CurrentEmployeeHelper
    {
        public static async Task<Guid> ResolveAsync(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken)
        {
            if (currentUser.EmployeeId.HasValue && currentUser.EmployeeId != Guid.Empty)
            {
                return currentUser.EmployeeId.Value;
            }

            if (currentUser.UserId.HasValue)
            {
                Guid? empId = await context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == currentUser.UserId.Value)
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
