using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.ShiftMasters.Commands
{
    public class CreateShiftMasterCommand : ShiftMasterCommandFields, IRequest<Guid> { }

    public class CreateShiftMasterCommandHandler : IRequestHandler<CreateShiftMasterCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateShiftMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateShiftMasterCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            ShiftMasterEntity entity = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                StartTime = request.StartTime ?? TimeSpan.FromHours(8),
                EndTime = request.EndTime ?? TimeSpan.FromHours(17),
                BreakMinutes = request.BreakMinutes ?? 60,
                WorkingMinutes = request.WorkingMinutes ?? 480,
                IsOvernight = request.IsOvernight ?? false,
                IsActive = request.IsActive ?? true,
            };
            ShiftMasterMapper.ApplyCommandFields(entity, request);

            if (!entity.BreakStartTime.HasValue && !entity.BreakEndTime.HasValue)
            {
                entity.BreakStartTime = TimeSpan.FromHours(12);
                entity.BreakEndTime = TimeSpan.FromHours(13);
            }

            if (entity.BreakStartTime.HasValue && entity.BreakEndTime.HasValue)
            {
                int breakMins = (int)(entity.BreakEndTime.Value - entity.BreakStartTime.Value).TotalMinutes;
                if (breakMins < 0)
                {
                    breakMins += 24 * 60;
                }

                entity.BreakMinutes = Math.Max(0, breakMins);
            }

            if (entity.WorkingMinutes <= 0)
            {
                int minutes = (int)(entity.EndTime - entity.StartTime).TotalMinutes;
                if (entity.IsOvernight)
                {
                    minutes += 24 * 60;
                }

                entity.WorkingMinutes = Math.Max(0, minutes - entity.BreakMinutes);
            }

            _ = _context.ShiftMasterEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.CREATE, "ShiftMasterEntity", entity.Id, null, ShiftMasterMapper.ToLogObject(entity), "Tạo ca làm việc " + entity.Name);
            return entity.Id;
        }

        internal async Task ValidateAsync(ShiftMasterCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã ca là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên ca là bắt buộc.");
            }

            if (!request.StartTime.HasValue || !request.EndTime.HasValue)
            {
                throw new InvalidOperationException("Giờ bắt đầu/kết thúc ca là bắt buộc.");
            }

            bool exists = await _context.ShiftMasterEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã ca đã tồn tại.");
            }
        }
    }

    public class UpdateShiftMasterCommand : ShiftMasterCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateShiftMasterCommandHandler : IRequestHandler<UpdateShiftMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateShiftMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateShiftMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.ShiftMasterEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            await new CreateShiftMasterCommandHandler(_context, _actionLog).ValidateAsync(request, request.Id, cancellationToken);
            object oldValue = ShiftMasterMapper.ToLogObject(entity);
            ShiftMasterMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.UPDATE, "ShiftMasterEntity", entity.Id, oldValue, ShiftMasterMapper.ToLogObject(entity), "Cập nhật ca " + entity.Name);
            return true;
        }
    }

    public class ActivateShiftMasterCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateShiftMasterCommandHandler : IRequestHandler<ActivateShiftMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateShiftMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateShiftMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.ShiftMasterEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.ACTIVATE, "ShiftMasterEntity", entity.Id, null, ShiftMasterMapper.ToLogObject(entity), "Kích hoạt ca");
            return true;
        }
    }

    public class DeactivateShiftMasterCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateShiftMasterCommandHandler : IRequestHandler<DeactivateShiftMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateShiftMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateShiftMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.ShiftMasterEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.DEACTIVATE, "ShiftMasterEntity", entity.Id, null, ShiftMasterMapper.ToLogObject(entity), "Vô hiệu hóa ca");
            return true;
        }
    }
}
