using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.ContractType.Commands
{
    public class CreateContractTypeCommand : ContractTypeCommandFields, IRequest<Guid> { }

    public class CreateContractTypeCommandHandler : IRequestHandler<CreateContractTypeCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateContractTypeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateContractTypeCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken, _context);

            ContractTypeEntity entity = new()
            {
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                IsActive = request.IsActive ?? true,
                DisplayOrder = request.DisplayOrder ?? 0,
            };

            ContractTypeMapper.ApplyCommandFields(entity, request);
            _ = _context.ContractTypeEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "ContractTypeEntity",
                entity.Id,
                null,
                ContractTypeMapper.ToLogObject(entity),
                "Tạo loại hợp đồng " + entity.Name);
            return entity.Id;
        }

        internal static async Task ValidateAsync(
            ContractTypeCommandFields request, Guid? excludeId, CancellationToken cancellationToken, IApplicationDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã loại hợp đồng là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên loại hợp đồng là bắt buộc.");
            }

            bool exists = await context.ContractTypeEntities
                .AnyAsync(x => !x.IsDeleted
                    && x.Code.ToLower() == request.Code.Trim().ToLower()
                    && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã loại hợp đồng đã tồn tại.");
            }

            if (request.CompanyIds != null && request.CompanyIds.Count > 0)
            {
                List<Guid> validCompanyIds = request.CompanyIds.Where(x => x != Guid.Empty).Distinct().ToList();
                if (validCompanyIds.Count > 0)
                {
                    int existingCount = await context.CompanyEntities
                        .CountAsync(x => validCompanyIds.Contains(x.Id) && !x.IsDeleted, cancellationToken);
                    if (existingCount != validCompanyIds.Count)
                    {
                        throw new InvalidOperationException("Một hoặc nhiều công ty được chọn không tồn tại.");
                    }
                }
            }
            else if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                bool companyExists = await context.CompanyEntities
                    .AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
                if (!companyExists)
                {
                    throw new InvalidOperationException("Công ty không tồn tại.");
                }
            }
        }
    }

    public class UpdateContractTypeCommand : ContractTypeCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateContractTypeCommandHandler : IRequestHandler<UpdateContractTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateContractTypeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateContractTypeCommand request, CancellationToken cancellationToken)
        {
            ContractTypeEntity? entity = await _context.ContractTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

            if (entity == null)
            {
                return false;
            }

            await CreateContractTypeCommandHandler.ValidateAsync(request, request.Id, cancellationToken, _context);
            object oldValue = ContractTypeMapper.ToLogObject(entity);
            ContractTypeMapper.ApplyCommandFields(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "ContractTypeEntity",
                entity.Id,
                oldValue,
                ContractTypeMapper.ToLogObject(entity),
                "Cập nhật loại hợp đồng " + entity.Name);

            return true;
        }
    }

    public class ActivateContractTypeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class ActivateContractTypeCommandHandler : IRequestHandler<ActivateContractTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ActivateContractTypeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ActivateContractTypeCommand request, CancellationToken cancellationToken)
        {
            ContractTypeEntity? entity = await _context.ContractTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsActive = true;
            entity.IsDeleted = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.ACTIVATE,
                "ContractTypeEntity",
                entity.Id,
                null,
                ContractTypeMapper.ToLogObject(entity),
                "Kích hoạt loại hợp đồng " + entity.Name);
            return true;
        }
    }

    public class DeactivateContractTypeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeactivateContractTypeCommandHandler : IRequestHandler<DeactivateContractTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public DeactivateContractTypeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeactivateContractTypeCommand request, CancellationToken cancellationToken)
        {
            ContractTypeEntity? entity = await _context.ContractTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.DEACTIVATE,
                "ContractTypeEntity",
                entity.Id,
                null,
                ContractTypeMapper.ToLogObject(entity),
                "Hủy kích hoạt loại hợp đồng " + entity.Name);
            return true;
        }
    }
}
