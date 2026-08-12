using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PartMasters.Commands
{
    #region Create Command
    public class CreatePartMasterCommand : PartMasterCommandFields, IRequest<Guid>
    {
    }

    public class CreatePartMasterCommandHandler : IRequestHandler<CreatePartMasterCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreatePartMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreatePartMasterCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            var partMaster = new PartMasterEntity
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            PartMasterMapper.ApplyCommandFields(partMaster, request);

            _context.PartMasterEntities.Add(partMaster);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "PartMasterEntity",
                partMaster.Id,
                null,
                PartMasterMapper.ToLogObject(partMaster),
                "Tạo mới mẫu tổ/nhóm " + partMaster.Name + " thành công");

            return partMaster.Id;
        }

        internal static async Task ValidateAsync(
            PartMasterCommandFields request,
            Guid? excludeId,
            CancellationToken cancellationToken,
            IApplicationDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("Mã mẫu tổ/nhóm là bắt buộc.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên mẫu tổ/nhóm là bắt buộc.");

            var exists = await context.PartMasterEntities
                .AnyAsync(x => x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

            if (exists)
                throw new InvalidOperationException("Mã mẫu tổ/nhóm đã tồn tại trong hệ thống.");

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
    public class UpdatePartMasterCommand : PartMasterCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePartMasterCommandHandler : IRequestHandler<UpdatePartMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdatePartMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdatePartMasterCommand request, CancellationToken cancellationToken)
        {
            var partMaster = await _context.PartMasterEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (partMaster == null) return false;

            await CreatePartMasterCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);

            var oldValue = PartMasterMapper.ToLogObject(partMaster);

            PartMasterMapper.ApplyCommandFields(partMaster, request);
            partMaster.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var newValue = PartMasterMapper.ToLogObject(partMaster);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "PartMasterEntity",
                partMaster.Id,
                oldValue,
                newValue,
                "Cập nhật thông tin mẫu tổ/nhóm " + partMaster.Name + " thành công");

            return true;
        }
    }
    #endregion

    #region Activate Command
    public class ActivatePartMasterCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivatePartMasterCommandHandler : IRequestHandler<ActivatePartMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivatePartMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivatePartMasterCommand request, CancellationToken cancellationToken)
        {
            var partMaster = await _context.PartMasterEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (partMaster == null) return false;

            partMaster.IsDeleted = false;
            partMaster.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "PartMasterEntity",
                partMaster.Id,
                new { IsDeleted = true },
                new { IsDeleted = false },
                "Kích hoạt mẫu tổ/nhóm " + partMaster.Name + " thành công");

            return true;
        }
    }
    #endregion

    #region Deactivate Command
    public class DeactivatePartMasterCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivatePartMasterCommandHandler : IRequestHandler<DeactivatePartMasterCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivatePartMasterCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivatePartMasterCommand request, CancellationToken cancellationToken)
        {
            var partMaster = await _context.PartMasterEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (partMaster == null) return false;

            partMaster.IsDeleted = true;
            partMaster.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "PartMasterEntity",
                partMaster.Id,
                new { IsDeleted = false },
                new { IsDeleted = true },
                "Ngưng hoạt động mẫu tổ/nhóm " + partMaster.Name + " thành công");

            return true;
        }
    }
    #endregion
}
