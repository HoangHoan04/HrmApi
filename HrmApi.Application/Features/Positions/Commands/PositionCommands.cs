using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Positions.Commands
{
    #region Create Command
    public class CreatePositionCommand : PositionCommandFields, IRequest<Guid>
    {
    }

    public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreatePositionCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            PositionEntity entity = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            PositionMapper.ApplyCommandFields(entity, request);

            _ = _context.PositionEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "PositionEntity",
                entity.Id,
                null,
                PositionMapper.ToLogObject(entity),
                "Tạo mới chức danh thành công");

            return entity.Id;
        }

        internal static async Task ValidateAsync(
            PositionCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (!request.PositionMasterId.HasValue)
            {
                throw new InvalidOperationException("Mẫu chức danh (PositionMaster) là bắt buộc.");
            }

            bool masterExists = await context.PositionMasterEntities
                .AnyAsync(x => x.Id == request.PositionMasterId.Value, cancellationToken);

            if (!masterExists)
            {
                throw new InvalidOperationException("Mẫu chức danh không tồn tại.");
            }

            if (request.DepartmentId.HasValue)
            {
                bool departmentExists = await context.DepartmentEntities
                    .AnyAsync(x => x.Id == request.DepartmentId.Value, cancellationToken);

                if (!departmentExists)
                {
                    throw new InvalidOperationException("Phòng ban không tồn tại.");
                }
            }

            if (request.PartId.HasValue)
            {
                bool partExists = await context.PartEntities
                    .AnyAsync(x => x.Id == request.PartId.Value, cancellationToken);

                if (!partExists)
                {
                    throw new InvalidOperationException("Tổ/nhóm không tồn tại.");
                }
            }

            if (request.CompanyId.HasValue)
            {
                bool companyExists = await context.CompanyEntities
                    .AnyAsync(x => x.Id == request.CompanyId.Value, cancellationToken);

                if (!companyExists)
                {
                    throw new InvalidOperationException("Công ty không tồn tại.");
                }
            }
        }
    }
    #endregion

    #region Update Command
    public class UpdatePositionCommand : PositionCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdatePositionCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return false;
            }

            await CreatePositionCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            object oldValue = PositionMapper.ToLogObject(entity);

            PositionMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "PositionEntity",
                entity.Id,
                oldValue,
                PositionMapper.ToLogObject(entity),
                "Cập nhật thông tin chức danh thành công");

            return true;
        }
    }
    #endregion

    #region Activate Command
    public class ActivatePositionCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivatePositionCommandHandler : IRequestHandler<ActivatePositionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivatePositionCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivatePositionCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = false;
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "PositionEntity",
                entity.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                "Kích hoạt chức danh thành công");

            return true;
        }
    }
    #endregion

    #region Deactivate Command
    public class DeactivatePositionCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivatePositionCommandHandler : IRequestHandler<DeactivatePositionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivatePositionCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivatePositionCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "PositionEntity",
                entity.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Ngưng hoạt động chức danh thành công");

            return true;
        }
    }
    #endregion
}
