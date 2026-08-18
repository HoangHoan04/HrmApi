using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.OvertimeRequest;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.OvertimeRequests
{
    #region Create
    public class CreateOvertimeRequestCommand : IRequest<Guid>
    {
        public Guid? EmployeeId { get; set; }
        public DateOnly WorkDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public int? RequestedMinutes { get; set; }
        public string OtType { get; set; } = OvertimeType.AfterShift;
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public bool Submit { get; set; }
    }

    public class CreateOvertimeRequestCommandHandler : IRequestHandler<CreateOvertimeRequestCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IWorkflowEngine _workflow;
        private readonly INotificationService _notificationService;

        public CreateOvertimeRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IWorkflowEngine workflow,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _workflow = workflow;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(CreateOvertimeRequestCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new InvalidOperationException("Vui lòng nhập lý do tăng ca.");

            ValidateOtType(request.OtType);
            int minutes = ResolveRequestedMinutes(request.FromTime, request.ToTime, request.RequestedMinutes);

            Guid employeeId = request.EmployeeId ?? await ResolveEmployeeIdAsync(cancellationToken);
            var employee = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");

            bool conflict = await _context.OvertimeRequestEntities.AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employeeId
                    && x.WorkDate == request.WorkDate
                    && !x.IsDeleted
                    && (x.Status == OvertimeRequestStatus.Draft
                        || x.Status == OvertimeRequestStatus.Submitted
                        || x.Status == OvertimeRequestStatus.Approved),
                    cancellationToken);
            if (conflict)
                throw new InvalidOperationException("Đã có đơn OT đang mở/đã duyệt cho ngày này.");

            var entity = new OvertimeRequestEntity
            {
                Code = await GenerateCodeAsync(cancellationToken),
                EmployeeId = employeeId,
                CompanyId = employee.CompanyId,
                BranchId = employee.BranchId,
                WorkDate = request.WorkDate,
                FromTime = request.FromTime,
                ToTime = request.ToTime,
                RequestedMinutes = minutes,
                OtType = request.OtType.Trim().ToUpperInvariant(),
                Reason = request.Reason.Trim(),
                AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim(),
                Status = request.Submit ? OvertimeRequestStatus.Submitted : OvertimeRequestStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };

            _context.OvertimeRequestEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "OvertimeRequestEntity",
                entity.Id,
                null,
                OvertimeRequestMapper.ToLogObject(entity),
                request.Submit ? "Tạo và gửi đơn OT" : "Tạo đơn OT (nháp)");

            if (request.Submit)
            {
                _ = await _workflow.StartAsync(
                    WorkflowEntityType.Ot, entity.Id, entity.CompanyId, cancellationToken);

                await _notificationService.NotifyAdminsAsync(
                    title: "Đơn đăng ký tăng ca mới",
                    content: $"Nhân viên {employee.FullName ?? employee.Code} vừa gửi đơn tăng ca ({request.WorkDate:dd/MM/yyyy}, {minutes} phút).",
                    type: NotificationType.Overtime,
                    severity: NotificationSeverity.Info,
                    targetUrl: "/time-attendance-manager/overtime-request-manager",
                    targetType: "OVERTIME_REQUEST",
                    targetId: entity.Id,
                    companyId: entity.CompanyId,
                    senderId: _currentUser.UserId,
                    cancellationToken: cancellationToken);
            }

            return entity.Id;
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            string prefix = $"OT-{DateTime.UtcNow:yyyyMMdd}-";
            int count = await _context.OvertimeRequestEntities.AsNoTracking()
                .CountAsync(x => x.Code.StartsWith(prefix), cancellationToken);
            return $"{prefix}{(count + 1):D4}";
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
                return _currentUser.EmployeeId.Value;

            if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                    return empId.Value;
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }

        internal static void ValidateOtType(string otType)
        {
            string t = (otType ?? string.Empty).Trim().ToUpperInvariant();
            if (t is not (OvertimeType.AfterShift or OvertimeType.DayOff or OvertimeType.Holiday))
                throw new InvalidOperationException("Loại OT không hợp lệ (AFTER_SHIFT / DAY_OFF / HOLIDAY).");
        }

        internal static int ResolveRequestedMinutes(TimeSpan from, TimeSpan to, int? overrideMinutes)
        {
            if (overrideMinutes.HasValue)
            {
                if (overrideMinutes.Value <= 0)
                    throw new InvalidOperationException("Số phút OT phải > 0.");
                return overrideMinutes.Value;
            }

            DateTime baseDay = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            DateTime start = baseDay.Add(from);
            DateTime end = baseDay.Add(to);
            if (end <= start) end = end.AddDays(1);
            int minutes = (int)Math.Floor((end - start).TotalMinutes);
            if (minutes <= 0)
                throw new InvalidOperationException("Khung giờ OT không hợp lệ.");
            return minutes;
        }
    }
    #endregion

    #region Submit
    public class SubmitOvertimeRequestCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class SubmitOvertimeRequestCommandHandler : IRequestHandler<SubmitOvertimeRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IWorkflowEngine _workflow;

        public SubmitOvertimeRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IWorkflowEngine workflow)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _workflow = workflow;
        }

        public async Task<bool> Handle(SubmitOvertimeRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.OvertimeRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            if (entity.Status != OvertimeRequestStatus.Draft)
                throw new InvalidOperationException("Chỉ gửi được đơn đang nháp.");

            var old = OvertimeRequestMapper.ToLogObject(entity);
            entity.Status = OvertimeRequestStatus.Submitted;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "OvertimeRequestEntity",
                entity.Id,
                old,
                OvertimeRequestMapper.ToLogObject(entity),
                "Gửi đơn OT duyệt");

            _ = await _workflow.StartAsync(
                WorkflowEntityType.Ot, entity.Id, entity.CompanyId, cancellationToken);

            return true;
        }
    }
    #endregion

    #region Cancel
    public class CancelOvertimeRequestCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelOvertimeRequestCommandHandler : IRequestHandler<CancelOvertimeRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public CancelOvertimeRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(CancelOvertimeRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.OvertimeRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            if (entity.Status is not (OvertimeRequestStatus.Draft or OvertimeRequestStatus.Submitted))
                throw new InvalidOperationException("Chỉ hủy được đơn nháp hoặc đang chờ duyệt.");

            var old = OvertimeRequestMapper.ToLogObject(entity);
            entity.Status = OvertimeRequestStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(request.Reason))
                entity.ApproverNote = request.Reason.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "OvertimeRequestEntity",
                entity.Id,
                old,
                OvertimeRequestMapper.ToLogObject(entity),
                "Hủy đơn OT");

            return true;
        }
    }
    #endregion

    #region Approve / Reject
    public class ReviewOvertimeRequestCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool Approve { get; set; }
        public int? ApprovedMinutes { get; set; }
        public string? ApproverNote { get; set; }
    }

    public class ReviewOvertimeRequestCommandHandler : IRequestHandler<ReviewOvertimeRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IAttendanceRuleService _rules;
        private readonly IWorkflowEngine _workflow;
        private readonly INotificationService _notificationService;

        public ReviewOvertimeRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IAttendanceRuleService rules,
            IWorkflowEngine workflow,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _rules = rules;
            _workflow = workflow;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(ReviewOvertimeRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.OvertimeRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            if (entity.Status != OvertimeRequestStatus.Submitted)
                throw new InvalidOperationException("Chỉ duyệt/từ chối đơn đang chờ duyệt.");

            var old = OvertimeRequestMapper.ToLogObject(entity);

            if (!request.Approve)
            {
                if (string.IsNullOrWhiteSpace(request.ApproverNote))
                    throw new InvalidOperationException("Vui lòng nhập lý do từ chối.");

                entity.Status = OvertimeRequestStatus.Rejected;
                entity.ApproverNote = request.ApproverNote.Trim();
                entity.ApproverId = _currentUser.EmployeeId;
                entity.ReviewedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUser.UserId;
                await _context.SaveChangesAsync(cancellationToken);

                await _actionLog.LogActionAsync(
                    ActionType.UPDATE,
                    "OvertimeRequestEntity",
                    entity.Id,
                    old,
                    OvertimeRequestMapper.ToLogObject(entity),
                    "Từ chối đơn OT");

                _ = await _workflow.AdvanceForEntityAsync(
                    WorkflowEntityType.Ot,
                    entity.Id,
                    approve: false,
                    request.ApproverNote,
                    _currentUser.UserId ?? Guid.Empty,
                    _currentUser.EmployeeId,
                    cancellationToken);

                await _notificationService.CreateNotifyForEmployeeAsync(
                    employeeId: entity.EmployeeId,
                    title: "Đơn đăng ký tăng ca bị từ chối",
                    content: $"Đơn tăng ca ngày {entity.WorkDate:dd/MM/yyyy} của bạn đã bị từ chối. Lý do: {request.ApproverNote}",
                    type: NotificationType.Overtime,
                    severity: NotificationSeverity.Danger,
                    targetUrl: "/time-attendance-manager/overtime-request-manager",
                    targetType: "OVERTIME_REQUEST",
                    targetId: entity.Id,
                    senderId: _currentUser.UserId,
                    cancellationToken: cancellationToken);

                return true;
            }

            if (request.ApprovedMinutes.HasValue && request.ApprovedMinutes.Value <= 0)
                throw new InvalidOperationException("Số phút OT duyệt phải > 0.");

            entity.Status = OvertimeRequestStatus.Approved;
            entity.ApprovedMinutes = request.ApprovedMinutes ?? entity.RequestedMinutes;
            entity.ApproverNote = string.IsNullOrWhiteSpace(request.ApproverNote) ? null : request.ApproverNote.Trim();
            entity.ApproverId = _currentUser.EmployeeId;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await EnsureScheduleForDayOffOtAsync(entity, cancellationToken);
            await RefreshTimekeepingMetricsAsync(entity, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "OvertimeRequestEntity",
                entity.Id,
                old,
                OvertimeRequestMapper.ToLogObject(entity),
                "Duyệt đơn OT");

            _ = await _workflow.AdvanceForEntityAsync(
                WorkflowEntityType.Ot,
                entity.Id,
                approve: true,
                request.ApproverNote,
                _currentUser.UserId ?? Guid.Empty,
                _currentUser.EmployeeId,
                cancellationToken);

            await _notificationService.CreateNotifyForEmployeeAsync(
                employeeId: entity.EmployeeId,
                title: "Đơn đăng ký tăng ca đã được duyệt",
                content: $"Đơn tăng ca ngày {entity.WorkDate:dd/MM/yyyy} ({entity.ApprovedMinutes} phút) của bạn đã được phê duyệt.",
                type: NotificationType.Overtime,
                severity: NotificationSeverity.Success,
                targetUrl: "/time-attendance-manager/overtime-request-manager",
                targetType: "OVERTIME_REQUEST",
                targetId: entity.Id,
                senderId: _currentUser.UserId,
                cancellationToken: cancellationToken);

            return true;
        }

        private async Task EnsureScheduleForDayOffOtAsync(OvertimeRequestEntity ot, CancellationToken cancellationToken)
        {
            if (ot.OtType is not (OvertimeType.DayOff or OvertimeType.Holiday))
                return;

            bool exists = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .AnyAsync(x => x.EmployeeId == ot.EmployeeId && x.WorkDate == ot.WorkDate && !x.IsDeleted, cancellationToken);
            if (exists) return;

            var employee = await _rules.ResolveEmployeeAsync(ot.EmployeeId, cancellationToken);
            var window = await _rules.ResolveWorkWindowAsync(employee, ot.WorkDate, cancellationToken);

            _context.WorkScheduledEmployeeEntities.Add(new WorkScheduledEmployeeEntity
            {
                EmployeeId = ot.EmployeeId,
                WorkDate = ot.WorkDate,
                BranchId = ot.BranchId ?? window.BranchId ?? employee.BranchId,
                ShiftId = window.ShiftId,
                ShiftMasterId = window.ShiftMasterId,
                Note = $"Auto từ OT {ot.Code}",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            });
        }

        private async Task RefreshTimekeepingMetricsAsync(OvertimeRequestEntity ot, CancellationToken cancellationToken)
        {
            var record = await _context.TimekeepingEntities
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == ot.EmployeeId
                    && x.WorkDate == ot.WorkDate
                    && !x.IsDeleted,
                    cancellationToken);
            if (record == null || !record.CheckInAt.HasValue || !record.CheckOutAt.HasValue)
                return;

            var employee = await _rules.ResolveEmployeeAsync(ot.EmployeeId, cancellationToken);
            var window = await _rules.ResolveWorkWindowAsync(employee, ot.WorkDate, cancellationToken);
            var standard = await _rules.ResolveStandardAsync(
                record.BranchId ?? ot.BranchId ?? employee.BranchId,
                employee.CompanyId,
                cancellationToken);
            _rules.ComputeStatus(record, window, standard);
            await _rules.FinalizeOtAndNightAsync(record, window, standard, cancellationToken);
            record.UpdatedAt = DateTime.UtcNow;
        }
    }
    #endregion

    #region Bulk approve
    public class BulkApproveOvertimeRequestCommand : IRequest<int>
    {
        public List<Guid> Ids { get; set; } = [];
        public string? ApproverNote { get; set; }
    }

    public class BulkApproveOvertimeRequestCommandHandler : IRequestHandler<BulkApproveOvertimeRequestCommand, int>
    {
        private readonly IMediator _mediator;

        public BulkApproveOvertimeRequestCommandHandler(IMediator mediator) => _mediator = mediator;

        public async Task<int> Handle(BulkApproveOvertimeRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
                throw new InvalidOperationException("Chọn ít nhất một đơn OT.");

            int ok = 0;
            foreach (Guid id in request.Ids.Distinct())
            {
                bool success = await _mediator.Send(new ReviewOvertimeRequestCommand
                {
                    Id = id,
                    Approve = true,
                    ApproverNote = request.ApproverNote,
                }, cancellationToken);
                if (success) ok++;
            }

            return ok;
        }
    }
    #endregion

    #region Queries
    public class GetOvertimeRequestsPagedQuery : PagedRequest, IRequest<PagedResult<OvertimeRequestDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Status { get; set; }
        public string? OtType { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }

    public class GetOvertimeRequestsPagedQueryHandler
        : IRequestHandler<GetOvertimeRequestsPagedQuery, PagedResult<OvertimeRequestDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetOvertimeRequestsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<OvertimeRequestDto>> Handle(
            GetOvertimeRequestsPagedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.OvertimeRequestEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string st = request.Status.Trim().ToUpperInvariant();
                query = query.Where(x => x.Status == st);
            }
            if (!string.IsNullOrWhiteSpace(request.OtType))
            {
                string ot = request.OtType.Trim().ToUpperInvariant();
                query = query.Where(x => x.OtType == ot);
            }
            if (request.FromDate.HasValue)
                query = query.Where(x => x.WorkDate >= request.FromDate);
            if (request.ToDate.HasValue)
                query = query.Where(x => x.WorkDate <= request.ToDate);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(s)
                    || x.Reason.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            var entities = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = await OvertimeRequestQueryHelper.MapManyAsync(_context, entities, cancellationToken);
            return new PagedResult<OvertimeRequestDto>(dtos, total, request.PageIndex, request.PageSize);
        }
    }

    public class GetOvertimeRequestByIdQuery : IRequest<OvertimeRequestDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetOvertimeRequestByIdQueryHandler : IRequestHandler<GetOvertimeRequestByIdQuery, OvertimeRequestDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetOvertimeRequestByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<OvertimeRequestDto?> Handle(GetOvertimeRequestByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.OvertimeRequestEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return null;

            var list = await OvertimeRequestQueryHelper.MapManyAsync(_context, [entity], cancellationToken);
            return list.FirstOrDefault();
        }
    }

    internal static class OvertimeRequestQueryHelper
    {
        public static async Task<List<OvertimeRequestDto>> MapManyAsync(
            IApplicationDbContext context,
            List<OvertimeRequestEntity> entities,
            CancellationToken cancellationToken)
        {
            if (entities.Count == 0) return [];

            var empIds = entities.Select(x => x.EmployeeId)
                .Concat(entities.Where(x => x.ApproverId.HasValue).Select(x => x.ApproverId!.Value))
                .Distinct()
                .ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            var empMap = await context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => new { x.Code, Name = x.FullName ?? ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim() },
                    cancellationToken);

            var branchMap = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return entities.Select(e =>
            {
                empMap.TryGetValue(e.EmployeeId, out var emp);
                string? branchName = e.BranchId.HasValue && branchMap.TryGetValue(e.BranchId.Value, out var bn) ? bn : null;
                string? approverName = e.ApproverId.HasValue && empMap.TryGetValue(e.ApproverId.Value, out var ap)
                    ? ap.Name
                    : null;
                return OvertimeRequestMapper.ToDto(e, emp?.Name, emp?.Code, branchName, approverName);
            }).ToList();
        }
    }
    #endregion
}
