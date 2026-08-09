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
}
