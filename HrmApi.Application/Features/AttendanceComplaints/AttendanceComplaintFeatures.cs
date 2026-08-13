using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.AttendanceComplaint;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.AttendanceComplaints
{
    #region Create
    public class CreateAttendanceComplaintCommand : IRequest<Guid>
    {
        public DateOnly WorkDate { get; set; }
        public AttendanceComplaintType ComplaintType { get; set; } = AttendanceComplaintType.FORGOT_BOTH;
        public TimeSpan? RequestedCheckInTime { get; set; }
        public TimeSpan? RequestedCheckOutTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public Guid? EmployeeId { get; set; }
    }

    public class CreateAttendanceComplaintCommandHandler : IRequestHandler<CreateAttendanceComplaintCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IWorkflowEngine _workflow;

        public CreateAttendanceComplaintCommandHandler(
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

        public async Task<Guid> Handle(CreateAttendanceComplaintCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new InvalidOperationException("Vui lòng nhập lý do khiếu nại.");

            if (request.WorkDate > BusinessDateHelper.Today())
                throw new InvalidOperationException("Không thể khiếu nại ngày trong tương lai.");

            Guid employeeId = request.EmployeeId ?? await ResolveEmployeeIdAsync(cancellationToken);
            var employee = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");

            ValidateRequestedTimes(request.ComplaintType, request.RequestedCheckInTime, request.RequestedCheckOutTime);

            bool hasPending = await _context.AttendanceComplaintEntities.AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employeeId
                    && x.WorkDate == request.WorkDate
                    && !x.IsDeleted
                    && x.Status == AttendanceComplaintStatus.PENDING,
                    cancellationToken);
            if (hasPending)
                throw new InvalidOperationException("Đã có khiếu nại đang chờ duyệt cho ngày này.");

            var timekeeping = await _context.TimekeepingEntities.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId
                    && x.WorkDate == request.WorkDate
                    && !x.IsDeleted,
                    cancellationToken);

            var entity = new AttendanceComplaintEntity
            {
                EmployeeId = employeeId,
                CompanyId = employee.CompanyId,
                BranchId = employee.BranchId,
                WorkDate = request.WorkDate,
                TimekeepingId = timekeeping?.Id,
                ComplaintType = request.ComplaintType,
                RequestedCheckInTime = request.RequestedCheckInTime,
                RequestedCheckOutTime = request.RequestedCheckOutTime,
                Reason = request.Reason.Trim(),
                AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim(),
                Status = AttendanceComplaintStatus.PENDING,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };

            _context.AttendanceComplaintEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "AttendanceComplaintEntity",
                entity.Id,
                null,
                AttendanceComplaintMapper.ToLogObject(entity),
                "Tạo khiếu nại chấm công");

            _ = await _workflow.StartAsync(
                WorkflowEntityType.Complaint, entity.Id, entity.CompanyId, cancellationToken);

            return entity.Id;
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

        internal static void ValidateRequestedTimes(
            AttendanceComplaintType type,
            TimeSpan? checkIn,
            TimeSpan? checkOut)
        {
            switch (type)
            {
                case AttendanceComplaintType.FORGOT_CHECK_IN:
                    if (!checkIn.HasValue)
                        throw new InvalidOperationException("Vui lòng nhập giờ vào ca đề xuất.");
                    break;
                case AttendanceComplaintType.FORGOT_CHECK_OUT:
                    if (!checkOut.HasValue)
                        throw new InvalidOperationException("Vui lòng nhập giờ ra ca đề xuất.");
                    break;
                case AttendanceComplaintType.FORGOT_BOTH:
                case AttendanceComplaintType.WRONG_TIME:
                    if (!checkIn.HasValue || !checkOut.HasValue)
                        throw new InvalidOperationException("Vui lòng nhập đủ giờ vào và giờ ra đề xuất.");
                    if (checkOut <= checkIn)
                        throw new InvalidOperationException("Giờ ra ca phải sau giờ vào ca.");
                    break;
                case AttendanceComplaintType.OTHER:
                    if (!checkIn.HasValue && !checkOut.HasValue)
                        throw new InvalidOperationException("Vui lòng nhập ít nhất một mốc giờ đề xuất.");
                    break;
            }
        }
    }
    #endregion

    #region Cancel (employee)
    public class CancelAttendanceComplaintCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelAttendanceComplaintCommandHandler : IRequestHandler<CancelAttendanceComplaintCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public CancelAttendanceComplaintCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(CancelAttendanceComplaintCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.AttendanceComplaintEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            Guid employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            if (entity.EmployeeId != employeeId)
                throw new InvalidOperationException("Không thể hủy khiếu nại của người khác.");

            if (entity.Status != AttendanceComplaintStatus.PENDING)
                throw new InvalidOperationException("Chỉ hủy được khiếu nại đang chờ duyệt.");

            var old = AttendanceComplaintMapper.ToLogObject(entity);
            entity.Status = AttendanceComplaintStatus.CANCELLED;
            if (!string.IsNullOrWhiteSpace(request.Reason))
                entity.ApproverNote = request.Reason.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "AttendanceComplaintEntity",
                entity.Id,
                old,
                AttendanceComplaintMapper.ToLogObject(entity),
                "Hủy khiếu nại chấm công");

            return true;
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
                if (empId.HasValue && empId != Guid.Empty) return empId.Value;
            }
            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }
    #endregion

    #region Approve / Reject
    public class ReviewAttendanceComplaintCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool Approve { get; set; }
        public string? ApproverNote { get; set; }
        public TimeSpan? OverrideCheckInTime { get; set; }
        public TimeSpan? OverrideCheckOutTime { get; set; }
    }

    public class ReviewAttendanceComplaintCommandHandler : IRequestHandler<ReviewAttendanceComplaintCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IAttendanceRuleService _rules;

        public ReviewAttendanceComplaintCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IAttendanceRuleService rules)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _rules = rules;
        }

        public async Task<bool> Handle(ReviewAttendanceComplaintCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.AttendanceComplaintEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            if (entity.Status != AttendanceComplaintStatus.PENDING)
                throw new InvalidOperationException("Khiếu nại đã được xử lý.");

            var old = AttendanceComplaintMapper.ToLogObject(entity);

            if (!request.Approve)
            {
                if (string.IsNullOrWhiteSpace(request.ApproverNote))
                    throw new InvalidOperationException("Vui lòng nhập lý do từ chối.");

                entity.Status = AttendanceComplaintStatus.REJECTED;
                entity.ApproverNote = request.ApproverNote.Trim();
                entity.ApproverId = _currentUser.EmployeeId;
                entity.ReviewedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUser.UserId;
                await _context.SaveChangesAsync(cancellationToken);

                await _actionLog.LogActionAsync(
                    ActionType.UPDATE,
                    "AttendanceComplaintEntity",
                    entity.Id,
                    old,
                    AttendanceComplaintMapper.ToLogObject(entity),
                    "Từ chối khiếu nại chấm công");
                return true;
            }

            TimeSpan? checkInTime = request.OverrideCheckInTime ?? entity.RequestedCheckInTime;
            TimeSpan? checkOutTime = request.OverrideCheckOutTime ?? entity.RequestedCheckOutTime;
            CreateAttendanceComplaintCommandHandler.ValidateRequestedTimes(
                entity.ComplaintType, checkInTime, checkOutTime);

            var employee = await _rules.ResolveEmployeeAsync(entity.EmployeeId, cancellationToken);
            var window = await _rules.ResolveWorkWindowAsync(employee, entity.WorkDate, cancellationToken);
            var standard = await _rules.ResolveStandardAsync(
                entity.BranchId ?? window.BranchId ?? employee.BranchId,
                employee.CompanyId,
                cancellationToken);

            TimekeepingEntity? record = null;
            if (entity.TimekeepingId.HasValue)
            {
                record = await _context.TimekeepingEntities
                    .FirstOrDefaultAsync(x => x.Id == entity.TimekeepingId.Value && !x.IsDeleted, cancellationToken);
            }
            record ??= await _context.TimekeepingEntities
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == entity.EmployeeId
                    && x.WorkDate == entity.WorkDate
                    && !x.IsDeleted,
                    cancellationToken);

            if (record == null)
            {
                record = new TimekeepingEntity
                {
                    EmployeeId = entity.EmployeeId,
                    CompanyId = employee.CompanyId,
                    BranchId = entity.BranchId ?? window.BranchId ?? employee.BranchId,
                    WorkDate = entity.WorkDate,
                    ShiftId = window.ShiftId,
                    ShiftMasterId = window.ShiftMasterId,
                    Status = AttendanceStatus.INCOMPLETE,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _context.TimekeepingEntities.Add(record);
            }

            if (checkInTime.HasValue)
                record.CheckInAt = BusinessDateHelper.ToUtc(entity.WorkDate, checkInTime.Value);
            if (checkOutTime.HasValue)
                record.CheckOutAt = BusinessDateHelper.ToUtc(entity.WorkDate, checkOutTime.Value);

            record.Note = string.IsNullOrWhiteSpace(entity.Reason)
                ? "Điều chỉnh từ khiếu nại chấm công"
                : $"Khiếu nại: {entity.Reason}";
            record.IsManualAdjusted = true;
            record.UpdatedAt = DateTime.UtcNow;
            record.UpdatedBy = _currentUser.UserId;

            _rules.ComputeStatus(record, window, standard);
            await _rules.FinalizeOtAndNightAsync(record, window, standard, cancellationToken);

            entity.Status = AttendanceComplaintStatus.APPROVED;
            entity.ApproverNote = string.IsNullOrWhiteSpace(request.ApproverNote)
                ? null
                : request.ApproverNote.Trim();
            entity.ApproverId = _currentUser.EmployeeId;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.TimekeepingId = record.Id;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "AttendanceComplaintEntity",
                entity.Id,
                old,
                AttendanceComplaintMapper.ToLogObject(entity),
                "Duyệt khiếu nại chấm công");

            return true;
        }
    }
    #endregion

    #region Queries
    public class GetAttendanceComplaintsPagedQuery : PagedRequest, IRequest<PagedResult<AttendanceComplaintDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public AttendanceComplaintStatus? Status { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }

    public class GetAttendanceComplaintsPagedQueryHandler
        : IRequestHandler<GetAttendanceComplaintsPagedQuery, PagedResult<AttendanceComplaintDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAttendanceComplaintsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<AttendanceComplaintDto>> Handle(
            GetAttendanceComplaintsPagedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.AttendanceComplaintEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);
            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status);
            if (request.FromDate.HasValue)
                query = query.Where(x => x.WorkDate >= request.FromDate);
            if (request.ToDate.HasValue)
                query = query.Where(x => x.WorkDate <= request.ToDate);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Reason.ToLower().Contains(s));
            }

            var total = await query.CountAsync(cancellationToken);
            var entities = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = await MapManyAsync(entities, cancellationToken);
            return new PagedResult<AttendanceComplaintDto>(dtos, total, request.PageIndex, request.PageSize);
        }

        private async Task<List<AttendanceComplaintDto>> MapManyAsync(
            List<AttendanceComplaintEntity> entities,
            CancellationToken cancellationToken)
        {
            if (entities.Count == 0) return new List<AttendanceComplaintDto>();

            var empIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var tkIds = entities.Where(x => x.TimekeepingId.HasValue).Select(x => x.TimekeepingId!.Value).Distinct().ToList();
            var approverIds = entities.Where(x => x.ApproverId.HasValue).Select(x => x.ApproverId!.Value).Distinct().ToList();

            var empMap = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => new { x.Code, Name = x.FullName ?? ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim() },
                    cancellationToken);

            var branchMap = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var tkMap = tkIds.Count == 0
                ? new Dictionary<Guid, TimekeepingEntity>()
                : await _context.TimekeepingEntities.AsNoTracking()
                    .Where(x => tkIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

            var approverMap = approverIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => approverIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => x.FullName ?? ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim(),
                        cancellationToken);

            return entities.Select(e =>
            {
                empMap.TryGetValue(e.EmployeeId, out var emp);
                string? branchName = e.BranchId.HasValue && branchMap.TryGetValue(e.BranchId.Value, out var bn) ? bn : null;
                string? approverName = e.ApproverId.HasValue && approverMap.TryGetValue(e.ApproverId.Value, out var an) ? an : null;
                TimekeepingEntity? tk = e.TimekeepingId.HasValue && tkMap.TryGetValue(e.TimekeepingId.Value, out var t) ? t : null;
                return AttendanceComplaintMapper.ToDto(
                    e,
                    emp?.Name,
                    emp?.Code,
                    branchName,
                    approverName,
                    tk?.CheckInAt,
                    tk?.CheckOutAt,
                    tk?.Status);
            }).ToList();
        }
    }

    public class GetAttendanceComplaintByIdQuery : IRequest<AttendanceComplaintDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetAttendanceComplaintByIdQueryHandler
        : IRequestHandler<GetAttendanceComplaintByIdQuery, AttendanceComplaintDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetAttendanceComplaintByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<AttendanceComplaintDto?> Handle(
            GetAttendanceComplaintByIdQuery request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.AttendanceComplaintEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return null;

            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == entity.EmployeeId)
                .Select(x => new { x.Code, Name = x.FullName ?? ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim() })
                .FirstOrDefaultAsync(cancellationToken);

            string? branchName = null;
            if (entity.BranchId.HasValue)
            {
                branchName = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == entity.BranchId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? approverName = null;
            if (entity.ApproverId.HasValue)
            {
                approverName = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == entity.ApproverId.Value)
                    .Select(x => x.FullName ?? ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim())
                    .FirstOrDefaultAsync(cancellationToken);
            }

            TimekeepingEntity? tk = null;
            if (entity.TimekeepingId.HasValue)
            {
                tk = await _context.TimekeepingEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.TimekeepingId.Value, cancellationToken);
            }

            return AttendanceComplaintMapper.ToDto(
                entity,
                emp?.Name,
                emp?.Code,
                branchName,
                approverName,
                tk?.CheckInAt,
                tk?.CheckOutAt,
                tk?.Status);
        }
    }

    public class GetMyAttendanceComplaintsQuery : IRequest<List<AttendanceComplaintDto>>
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class GetMyAttendanceComplaintsQueryHandler
        : IRequestHandler<GetMyAttendanceComplaintsQuery, List<AttendanceComplaintDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyAttendanceComplaintsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<AttendanceComplaintDto>> Handle(
            GetMyAttendanceComplaintsQuery request,
            CancellationToken cancellationToken)
        {
            Guid employeeId;
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
                employeeId = _currentUser.EmployeeId.Value;
            else if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (!empId.HasValue || empId == Guid.Empty)
                    throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
                employeeId = empId.Value;
            }
            else
                throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");

            var query = _context.AttendanceComplaintEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted);

            if (request.Year.HasValue && request.Month.HasValue)
            {
                var from = new DateOnly(request.Year.Value, request.Month.Value, 1);
                var to = from.AddMonths(1).AddDays(-1);
                query = query.Where(x => x.WorkDate >= from && x.WorkDate <= to);
            }

            var entities = await query.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(cancellationToken);

            var result = new List<AttendanceComplaintDto>();
            foreach (var e in entities)
            {
                TimekeepingEntity? tk = null;
                if (e.TimekeepingId.HasValue)
                {
                    tk = await _context.TimekeepingEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == e.TimekeepingId.Value, cancellationToken);
                }
                result.Add(AttendanceComplaintMapper.ToDto(
                    e,
                    null,
                    null,
                    null,
                    null,
                    tk?.CheckInAt,
                    tk?.CheckOutAt,
                    tk?.Status));
            }
            return result;
        }
    }
    #endregion
}
