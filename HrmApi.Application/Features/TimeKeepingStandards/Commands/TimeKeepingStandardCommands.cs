using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.TimeKeepingStandards.Commands
{
    public class CreateTimeKeepingStandardCommand : TimeKeepingStandardCommandFields, IRequest<Guid> { }

    public class CreateTimeKeepingStandardCommandHandler : IRequestHandler<CreateTimeKeepingStandardCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateTimeKeepingStandardCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateTimeKeepingStandardCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            TimeKeepingStandardEntity entity = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                AllowedRadiusMeters = request.AllowedRadiusMeters ?? 200,
                LateGraceMinutes = request.LateGraceMinutes ?? 0,
                EarlyLeaveGraceMinutes = request.EarlyLeaveGraceMinutes ?? 0,
                IsActive = request.IsActive ?? true,
            };
            TimeKeepingStandardMapper.ApplyCommandFields(entity, request);

            _ = _context.TimeKeepingStandardEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "TimeKeepingStandardEntity",
                entity.Id,
                null,
                TimeKeepingStandardMapper.ToLogObject(entity),
                "Tạo chuẩn chấm công " + entity.Name);

            return entity.Id;
        }

        internal async Task ValidateAsync(TimeKeepingStandardCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã chuẩn chấm công là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên chuẩn chấm công là bắt buộc.");
            }

            if ((request.AllowedRadiusMeters ?? 200) <= 0)
            {
                throw new InvalidOperationException("Bán kính cho phép phải lớn hơn 0.");
            }

            bool exists = await _context.TimeKeepingStandardEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã chuẩn chấm công đã tồn tại.");
            }
        }
    }

    public class UpdateTimeKeepingStandardCommand : TimeKeepingStandardCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateTimeKeepingStandardCommandHandler : IRequestHandler<UpdateTimeKeepingStandardCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreateTimeKeepingStandardCommandHandler _createHandler;

        public UpdateTimeKeepingStandardCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreateTimeKeepingStandardCommandHandler(context, actionLog);
        }

        public async Task<bool> Handle(UpdateTimeKeepingStandardCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.TimeKeepingStandardEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            await _createHandler.ValidateAsync(request, request.Id, cancellationToken);
            object oldValue = TimeKeepingStandardMapper.ToLogObject(entity);
            TimeKeepingStandardMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "TimeKeepingStandardEntity",
                entity.Id,
                oldValue,
                TimeKeepingStandardMapper.ToLogObject(entity),
                "Cập nhật chuẩn chấm công " + entity.Name);

            return true;
        }
    }

    public class ActivateTimeKeepingStandardCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateTimeKeepingStandardCommandHandler : IRequestHandler<ActivateTimeKeepingStandardCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateTimeKeepingStandardCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateTimeKeepingStandardCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.TimeKeepingStandardEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.ACTIVATE, "TimeKeepingStandardEntity", entity.Id, null, TimeKeepingStandardMapper.ToLogObject(entity), "Kích hoạt chuẩn chấm công");
            return true;
        }
    }

    public class DeactivateTimeKeepingStandardCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateTimeKeepingStandardCommandHandler : IRequestHandler<DeactivateTimeKeepingStandardCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateTimeKeepingStandardCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateTimeKeepingStandardCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.TimeKeepingStandardEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.DEACTIVATE, "TimeKeepingStandardEntity", entity.Id, null, TimeKeepingStandardMapper.ToLogObject(entity), "Vô hiệu hóa chuẩn chấm công");
            return true;
        }
    }
}
