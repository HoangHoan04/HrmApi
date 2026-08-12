using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.WorkSchedules.Commands
{
    public class CreateWorkScheduleCommand : WorkScheduleCommandFields, IRequest<Guid> { }

    public class CreateWorkScheduleCommandHandler : IRequestHandler<CreateWorkScheduleCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateWorkScheduleCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateWorkScheduleCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            var entity = new WorkScheduledEmployeeEntity
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                EmployeeId = request.EmployeeId,
                WorkDate = request.WorkDate,
            };
            WorkScheduleMapper.ApplyCommandFields(entity, request);

            _context.WorkScheduledEmployeeEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "WorkScheduledEmployeeEntity",
                entity.Id,
                null,
                WorkScheduleMapper.ToLogObject(entity),
                "Xếp lịch làm việc");

            return entity.Id;
        }

        internal async Task ValidateAsync(WorkScheduleCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (request.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            if (request.WorkDate == default)
                throw new InvalidOperationException("Ngày làm việc là bắt buộc.");
            if (!request.ShiftMasterId.HasValue && !request.ShiftId.HasValue)
                throw new InvalidOperationException("Cần chọn ca làm việc (ShiftMaster hoặc Shift).");

            var employeeExists = await _context.EmployeeEntities
                .AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (!employeeExists)
                throw new InvalidOperationException("Nhân viên không tồn tại.");

            if (request.ShiftMasterId.HasValue)
            {
                var smExists = await _context.ShiftMasterEntities
                    .AnyAsync(x => x.Id == request.ShiftMasterId.Value && !x.IsDeleted, cancellationToken);
                if (!smExists)
                    throw new InvalidOperationException("Mẫu ca không tồn tại.");
            }

            if (request.ShiftId.HasValue)
            {
                var shiftExists = await _context.ShiftEntities
                    .AnyAsync(x => x.Id == request.ShiftId.Value && !x.IsDeleted, cancellationToken);
                if (!shiftExists)
                    throw new InvalidOperationException("Ca làm việc không tồn tại.");
            }

            if (request.BranchId.HasValue)
            {
                var branchExists = await _context.BranchEntities
                    .AnyAsync(x => x.Id == request.BranchId.Value && !x.IsDeleted, cancellationToken);
                if (!branchExists)
                    throw new InvalidOperationException("Chi nhánh không tồn tại.");
            }

            var duplicate = await _context.WorkScheduledEmployeeEntities
                .AnyAsync(x => x.EmployeeId == request.EmployeeId
                    && x.WorkDate == request.WorkDate
                    && !x.IsDeleted
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (duplicate)
                throw new InvalidOperationException("Nhân viên đã có lịch làm việc trong ngày này.");
        }
    }

    public class UpdateWorkScheduleCommand : WorkScheduleCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateWorkScheduleCommandHandler : IRequestHandler<UpdateWorkScheduleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreateWorkScheduleCommandHandler _createHandler;

        public UpdateWorkScheduleCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreateWorkScheduleCommandHandler(context, actionLog);
        }

        public async Task<bool> Handle(UpdateWorkScheduleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.WorkScheduledEmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            await _createHandler.ValidateAsync(request, request.Id, cancellationToken);
            var oldValue = WorkScheduleMapper.ToLogObject(entity);
            WorkScheduleMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "WorkScheduledEmployeeEntity",
                entity.Id,
                oldValue,
                WorkScheduleMapper.ToLogObject(entity),
                "Cập nhật lịch làm việc");

            return true;
        }
    }

    public class DeactivateWorkScheduleCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateWorkScheduleCommandHandler : IRequestHandler<DeactivateWorkScheduleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateWorkScheduleCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateWorkScheduleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.WorkScheduledEmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "WorkScheduledEmployeeEntity",
                entity.Id,
                null,
                WorkScheduleMapper.ToLogObject(entity),
                "Xóa lịch làm việc");

            return true;
        }
    }

    public class BulkCreateWorkScheduleCommand : IRequest<BulkWorkScheduleResult>
    {
        public List<Guid> EmployeeIds { get; set; } = [];
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Note { get; set; }
        public bool SkipWeekends { get; set; } = true;
        public bool OverwriteExisting { get; set; }
    }

    public class CopyWorkScheduleWeekCommand : IRequest<BulkWorkScheduleResult>
    {
        public List<Guid> EmployeeIds { get; set; } = [];
        public DateOnly SourceWeekStart { get; set; }
        public DateOnly TargetWeekStart { get; set; }
        public bool OverwriteExisting { get; set; }
    }

    public class BulkWorkScheduleResult
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
    }

    public class BulkCreateWorkScheduleCommandHandler : IRequestHandler<BulkCreateWorkScheduleCommand, BulkWorkScheduleResult>
    {
        private readonly IApplicationDbContext _context;
        public BulkCreateWorkScheduleCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<BulkWorkScheduleResult> Handle(BulkCreateWorkScheduleCommand request, CancellationToken cancellationToken)
        {
            if (request.EmployeeIds.Count == 0)
                throw new InvalidOperationException("Chọn ít nhất 1 nhân viên.");
            if (request.ToDate < request.FromDate)
                throw new InvalidOperationException("Đến ngày phải >= Từ ngày.");
            if (!request.ShiftMasterId.HasValue && !request.ShiftId.HasValue)
                throw new InvalidOperationException("Cần chọn ca làm việc.");

            BulkWorkScheduleResult result = new();
            foreach (Guid employeeId in request.EmployeeIds.Distinct())
            {
                for (DateOnly d = request.FromDate; d <= request.ToDate; d = d.AddDays(1))
                {
                    if (request.SkipWeekends && (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday))
                    {
                        result.Skipped++;
                        continue;
                    }

                    WorkScheduledEmployeeEntity? existing = await _context.WorkScheduledEmployeeEntities
                        .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.WorkDate == d && !x.IsDeleted, cancellationToken);

                    if (existing != null)
                    {
                        if (!request.OverwriteExisting)
                        {
                            result.Skipped++;
                            continue;
                        }
                        existing.ShiftMasterId = request.ShiftMasterId;
                        existing.ShiftId = request.ShiftId;
                        existing.BranchId = request.BranchId;
                        existing.Note = request.Note;
                        existing.UpdatedAt = DateTime.UtcNow;
                        result.Updated++;
                    }
                    else
                    {
                        _ = _context.WorkScheduledEmployeeEntities.Add(new WorkScheduledEmployeeEntity
                        {
                            EmployeeId = employeeId,
                            WorkDate = d,
                            ShiftMasterId = request.ShiftMasterId,
                            ShiftId = request.ShiftId,
                            BranchId = request.BranchId,
                            Note = request.Note,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                        });
                        result.Created++;
                    }
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return result;
        }
    }

    public class CopyWorkScheduleWeekCommandHandler : IRequestHandler<CopyWorkScheduleWeekCommand, BulkWorkScheduleResult>
    {
        private readonly IApplicationDbContext _context;
        public CopyWorkScheduleWeekCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<BulkWorkScheduleResult> Handle(CopyWorkScheduleWeekCommand request, CancellationToken cancellationToken)
        {
            if (request.EmployeeIds.Count == 0)
                throw new InvalidOperationException("Chọn ít nhất 1 nhân viên.");

            DateOnly sourceEnd = request.SourceWeekStart.AddDays(6);
            DateOnly targetEnd = request.TargetWeekStart.AddDays(6);
            int offsetDays = request.TargetWeekStart.DayNumber - request.SourceWeekStart.DayNumber;

            List<WorkScheduledEmployeeEntity> source = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .Where(x => request.EmployeeIds.Contains(x.EmployeeId)
                    && x.WorkDate >= request.SourceWeekStart
                    && x.WorkDate <= sourceEnd
                    && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            BulkWorkScheduleResult result = new();
            foreach (WorkScheduledEmployeeEntity src in source)
            {
                DateOnly targetDate = src.WorkDate.AddDays(offsetDays);
                if (targetDate < request.TargetWeekStart || targetDate > targetEnd) continue;

                WorkScheduledEmployeeEntity? existing = await _context.WorkScheduledEmployeeEntities
                    .FirstOrDefaultAsync(x => x.EmployeeId == src.EmployeeId && x.WorkDate == targetDate && !x.IsDeleted, cancellationToken);

                if (existing != null)
                {
                    if (!request.OverwriteExisting)
                    {
                        result.Skipped++;
                        continue;
                    }
                    existing.ShiftMasterId = src.ShiftMasterId;
                    existing.ShiftId = src.ShiftId;
                    existing.BranchId = src.BranchId;
                    existing.Note = src.Note;
                    existing.UpdatedAt = DateTime.UtcNow;
                    result.Updated++;
                }
                else
                {
                    _ = _context.WorkScheduledEmployeeEntities.Add(new WorkScheduledEmployeeEntity
                    {
                        EmployeeId = src.EmployeeId,
                        WorkDate = targetDate,
                        ShiftMasterId = src.ShiftMasterId,
                        ShiftId = src.ShiftId,
                        BranchId = src.BranchId,
                        Note = src.Note,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                    });
                    result.Created++;
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return result;
        }
    }
}
