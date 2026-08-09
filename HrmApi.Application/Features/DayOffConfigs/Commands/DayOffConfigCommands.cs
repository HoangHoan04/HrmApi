using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.DayOffConfigs.Commands
{
    public class CreateDayOffConfigCommand : DayOffConfigCommandFields, IRequest<Guid> { }

    public class CreateDayOffConfigCommandHandler : IRequestHandler<CreateDayOffConfigCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateDayOffConfigCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateDayOffConfigCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            var entity = new DayOffConfigEntity
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                DayOffType = request.DayOffType ?? DayOffType.Annual,
                DefaultDaysPerYear = request.DefaultDaysPerYear ?? 12,
                IsPaid = request.IsPaid ?? true,
                IsActive = request.IsActive ?? true,
            };
            DayOffConfigMapper.ApplyCommandFields(entity, request);

            _context.DayOffConfigEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "DayOffConfigEntity",
                entity.Id,
                null,
                DayOffConfigMapper.ToLogObject(entity),
                "Tạo cấu hình nghỉ phép " + entity.Name);

            return entity.Id;
        }

        internal async Task ValidateAsync(DayOffConfigCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("Mã cấu hình nghỉ phép là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên cấu hình nghỉ phép là bắt buộc.");

            var exists = await _context.DayOffConfigEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
                throw new InvalidOperationException("Mã cấu hình nghỉ phép đã tồn tại.");
        }
    }

    public class UpdateDayOffConfigCommand : DayOffConfigCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateDayOffConfigCommandHandler : IRequestHandler<UpdateDayOffConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreateDayOffConfigCommandHandler _createHandler;

        public UpdateDayOffConfigCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreateDayOffConfigCommandHandler(context, actionLog);
        }

        public async Task<bool> Handle(UpdateDayOffConfigCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.DayOffConfigEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;

            await _createHandler.ValidateAsync(request, request.Id, cancellationToken);
            var oldValue = DayOffConfigMapper.ToLogObject(entity);
            DayOffConfigMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "DayOffConfigEntity",
                entity.Id,
                oldValue,
                DayOffConfigMapper.ToLogObject(entity),
                "Cập nhật cấu hình nghỉ phép " + entity.Name);

            return true;
        }
    }

    public class ActivateDayOffConfigCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateDayOffConfigCommandHandler : IRequestHandler<ActivateDayOffConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateDayOffConfigCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateDayOffConfigCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.DayOffConfigEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.ACTIVATE, "DayOffConfigEntity", entity.Id, null, DayOffConfigMapper.ToLogObject(entity), "Kích hoạt cấu hình nghỉ phép");
            return true;
        }
    }

    public class DeactivateDayOffConfigCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateDayOffConfigCommandHandler : IRequestHandler<DeactivateDayOffConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateDayOffConfigCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateDayOffConfigCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.DayOffConfigEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.DEACTIVATE, "DayOffConfigEntity", entity.Id, null, DayOffConfigMapper.ToLogObject(entity), "Vô hiệu hóa cấu hình nghỉ phép");
            return true;
        }
    }
}
