using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PublicHolidays.Commands
{
    public class CreatePublicHolidayCommand : PublicHolidayCommandFields, IRequest<Guid> { }

    public class CreatePublicHolidayCommandHandler : IRequestHandler<CreatePublicHolidayCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreatePublicHolidayCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreatePublicHolidayCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            PublicHolidayEntity entity = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                HolidayDate = request.HolidayDate,
                IsRecurringYearly = request.IsRecurringYearly ?? false,
                IsActive = request.IsActive ?? true,
            };
            PublicHolidayMapper.ApplyCommandFields(entity, request);

            _ = _context.PublicHolidayEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "PublicHolidayEntity",
                entity.Id,
                null,
                PublicHolidayMapper.ToLogObject(entity),
                "Tạo ngày nghỉ lễ " + entity.Name);

            return entity.Id;
        }

        internal async Task ValidateAsync(PublicHolidayCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã ngày lễ là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên ngày lễ là bắt buộc.");
            }

            if (request.HolidayDate == default)
            {
                throw new InvalidOperationException("Ngày lễ là bắt buộc.");
            }

            bool exists = await _context.PublicHolidayEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã ngày lễ đã tồn tại.");
            }
        }
    }

    public class UpdatePublicHolidayCommand : PublicHolidayCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePublicHolidayCommandHandler : IRequestHandler<UpdatePublicHolidayCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly CreatePublicHolidayCommandHandler _createHandler;

        public UpdatePublicHolidayCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
            _createHandler = new CreatePublicHolidayCommandHandler(context, actionLog);
        }

        public async Task<bool> Handle(UpdatePublicHolidayCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PublicHolidayEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            await _createHandler.ValidateAsync(request, request.Id, cancellationToken);
            object oldValue = PublicHolidayMapper.ToLogObject(entity);
            PublicHolidayMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "PublicHolidayEntity",
                entity.Id,
                oldValue,
                PublicHolidayMapper.ToLogObject(entity),
                "Cập nhật ngày nghỉ lễ " + entity.Name);

            return true;
        }
    }

    public class ActivatePublicHolidayCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivatePublicHolidayCommandHandler : IRequestHandler<ActivatePublicHolidayCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivatePublicHolidayCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivatePublicHolidayCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PublicHolidayEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.ACTIVATE, "PublicHolidayEntity", entity.Id, null, PublicHolidayMapper.ToLogObject(entity), "Kích hoạt ngày nghỉ lễ");
            return true;
        }
    }

    public class DeactivatePublicHolidayCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivatePublicHolidayCommandHandler : IRequestHandler<DeactivatePublicHolidayCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivatePublicHolidayCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivatePublicHolidayCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PublicHolidayEntities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(ActionType.DEACTIVATE, "PublicHolidayEntity", entity.Id, null, PublicHolidayMapper.ToLogObject(entity), "Vô hiệu hóa ngày nghỉ lễ");
            return true;
        }
    }
}
