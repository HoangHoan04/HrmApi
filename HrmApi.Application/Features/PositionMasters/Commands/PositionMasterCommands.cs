using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PositionMasters.Commands
{
    #region Create Command
    public class CreatePositionMasterCommand : PositionMasterCommandFields, IRequest<Guid>
    {
    }

    public class CreatePositionMasterCommandHandler : IRequestHandler<CreatePositionMasterCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreatePositionMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreatePositionMasterCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            var entity = new PositionMasterEntity
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            PositionMasterMapper.ApplyCommandFields(entity, request);

            _context.PositionMasterEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "PositionMasterEntity",
                entity.Id,
                null,
                PositionMasterMapper.ToLogObject(entity),
                "Tạo mới mẫu chức danh " + entity.Name + " thành công");

            return entity.Id;
        }

        internal static async Task ValidateAsync(
            PositionMasterCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("Mã mẫu chức danh là bắt buộc.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên mẫu chức danh là bắt buộc.");

            var exists = await context.PositionMasterEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

            if (exists)
                throw new InvalidOperationException("Mã mẫu chức danh đã tồn tại trong hệ thống.");

            if (request.CompanyId.HasValue)
            {
                var companyExists = await context.CompanyEntities
                    .AnyAsync(x => x.Id == request.CompanyId.Value, cancellationToken);

                if (!companyExists)
                    throw new InvalidOperationException("Công ty không tồn tại.");
            }
        }
    }
    #endregion

    #region Update Command
    public class UpdatePositionMasterCommand : PositionMasterCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePositionMasterCommandHandler : IRequestHandler<UpdatePositionMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdatePositionMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdatePositionMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionMasterEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) return false;

            await CreatePositionMasterCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            var oldValue = PositionMasterMapper.ToLogObject(entity);

            PositionMasterMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "PositionMasterEntity",
                entity.Id,
                oldValue,
                PositionMasterMapper.ToLogObject(entity),
                "Cập nhật thông tin mẫu chức danh " + entity.Name + " thành công");

            return true;
        }
    }
    #endregion

    #region Activate Command
    public class ActivatePositionMasterCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivatePositionMasterCommandHandler : IRequestHandler<ActivatePositionMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivatePositionMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivatePositionMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionMasterEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) return false;

            entity.IsDeleted = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "PositionMasterEntity",
                entity.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                "Kích hoạt mẫu chức danh " + entity.Name + " thành công");

            return true;
        }
    }
    #endregion

    #region Deactivate Command
    public class DeactivatePositionMasterCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivatePositionMasterCommandHandler : IRequestHandler<DeactivatePositionMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivatePositionMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivatePositionMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionMasterEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "PositionMasterEntity",
                entity.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Ngưng hoạt động mẫu chức danh " + entity.Name + " thành công");

            return true;
        }
    }
    #endregion
}
