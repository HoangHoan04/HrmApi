using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Asset;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Asset;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Asset
{
    public class GetAssetTypesPagedQuery : AssetTypePagedQuery, IRequest<PagedResult<AssetTypeDto>> { }

    public class GetAssetTypesPagedQueryHandler : IRequestHandler<GetAssetTypesPagedQuery, PagedResult<AssetTypeDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAssetTypesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AssetTypeDto>> Handle(GetAssetTypesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetTypeEntity> query = _context.AssetTypeEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<AssetTypeEntity> rows = await query
                .OrderBy(x => x.Code)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AssetTypeDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<AssetTypeDto>> MapManyAsync(List<AssetTypeEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> companyIds = rows.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            Dictionary<Guid, string> companies = companyIds.Count == 0
                ? []
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e => new AssetTypeDto
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                CompanyId = e.CompanyId,
                CompanyName = e.CompanyId.HasValue ? companies.GetValueOrDefault(e.CompanyId.Value) : null,
                Description = e.Description,
                IsActive = e.IsActive,
                IsSerialRequired = e.IsSerialRequired,
                MaxPerEmployee = e.MaxPerEmployee,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }
    }

    public class GetAssetTypeByIdQuery : IdRequest, IRequest<AssetTypeDto?> { }

    public class GetAssetTypeByIdQueryHandler : IRequestHandler<GetAssetTypeByIdQuery, AssetTypeDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetAssetTypesPagedQueryHandler _mapper;
        public GetAssetTypeByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetAssetTypesPagedQueryHandler(context);
        }

        public async Task<AssetTypeDto?> Handle(GetAssetTypeByIdQuery request, CancellationToken cancellationToken)
        {
            AssetTypeEntity? e = await _context.AssetTypeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateAssetTypeCommand : AssetTypeCommandFields, IRequest<Guid> { }

    public class CreateAssetTypeCommandHandler : IRequestHandler<CreateAssetTypeCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public CreateAssetTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateAssetTypeCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            AssetTypeEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                Name = request.Name!.Trim(),
                CompanyId = request.CompanyId,
                Description = request.Description,
                IsActive = request.IsActive ?? true,
                IsSerialRequired = request.IsSerialRequired ?? true,
                MaxPerEmployee = request.MaxPerEmployee,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.AssetTypeEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "AssetTypeEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.Name },
                "Tạo mới loại tài sản " + entity.Name);

            return entity.Id;
        }

        internal async Task ValidateAsync(AssetTypeCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã loại tài sản là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên loại tài sản là bắt buộc.");
            }

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.AssetTypeEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && x.CompanyId == request.CompanyId && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Mã loại tài sản đã tồn tại trong công ty.");
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty
                && !await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }
        }
    }

    public class UpdateAssetTypeCommand : AssetTypeCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateAssetTypeCommandHandler : IRequestHandler<UpdateAssetTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly CreateAssetTypeCommandHandler _create;

        public UpdateAssetTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _create = new CreateAssetTypeCommandHandler(context, currentUser, actionLog);
        }

        public async Task<bool> Handle(UpdateAssetTypeCommand request, CancellationToken cancellationToken)
        {
            AssetTypeEntity? entity = await _context.AssetTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.Code ??= entity.Code;
            request.Name ??= entity.Name;
            request.CompanyId ??= entity.CompanyId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.Name = request.Name!.Trim();
            entity.CompanyId = request.CompanyId;
            entity.Description = request.Description;
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.IsSerialRequired = request.IsSerialRequired ?? entity.IsSerialRequired;
            entity.MaxPerEmployee = request.MaxPerEmployee;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "AssetTypeEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.Name },
                "Cập nhật loại tài sản " + entity.Name);

            return true;
        }
    }

    public class DeleteAssetTypeCommand : IdRequest, IRequest<bool> { }

    public class DeleteAssetTypeCommandHandler : IRequestHandler<DeleteAssetTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public DeleteAssetTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteAssetTypeCommand request, CancellationToken cancellationToken)
        {
            AssetTypeEntity? entity = await _context.AssetTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (await _context.AssetEntities.AnyAsync(x => !x.IsDeleted && x.AssetTypeId == request.Id, cancellationToken))
            {
                throw new InvalidOperationException("Loại tài sản đang có tài sản trực thuộc — không thể xóa.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DELETE,
                "AssetTypeEntity",
                entity.Id,
                null,
                null,
                "Xóa loại tài sản " + entity.Name);

            return true;
        }
    }
}
