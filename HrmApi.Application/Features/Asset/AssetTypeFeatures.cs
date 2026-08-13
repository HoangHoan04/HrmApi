using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Asset;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Asset;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Asset
{
    public class GetAssetTypesPagedQuery : AssetTypePagedQuery, IRequest<PagedResult<AssetTypeDto>> { }

    public class GetAssetTypesPagedQueryHandler : IRequestHandler<GetAssetTypesPagedQuery, PagedResult<AssetTypeDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAssetTypesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<AssetTypeDto>> Handle(GetAssetTypesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetTypeEntity> query = _context.AssetTypeEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<AssetTypeEntity> rows = await query
                .OrderBy(x => x.Name)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AssetTypeDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<AssetTypeDto>> MapManyAsync(List<AssetTypeEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return [];
            var companyIds = rows.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var companies = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
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
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }

        internal static AssetTypeDto ToDto(AssetTypeEntity e) => new()
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            CompanyId = e.CompanyId,
            Description = e.Description,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };
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
            if (e == null) return null;
            return (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateAssetTypeCommand : AssetTypeCommandFields, IRequest<Guid> { }

    public class CreateAssetTypeCommandHandler : IRequestHandler<CreateAssetTypeCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateAssetTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
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
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.AssetTypeEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(AssetTypeCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code)) throw new InvalidOperationException("Mã loại tài sản là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Tên loại tài sản là bắt buộc.");

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.AssetTypeEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
                throw new InvalidOperationException("Mã loại tài sản đã tồn tại.");

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty
                && !await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Công ty không tồn tại.");
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
        private readonly CreateAssetTypeCommandHandler _create;
        public UpdateAssetTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateAssetTypeCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateAssetTypeCommand request, CancellationToken cancellationToken)
        {
            AssetTypeEntity? entity = await _context.AssetTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            request.Code ??= entity.Code;
            request.Name ??= entity.Name;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.Name = request.Name!.Trim();
            entity.CompanyId = request.CompanyId;
            entity.Description = request.Description;
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteAssetTypeCommand : IdRequest, IRequest<bool> { }

    public class DeleteAssetTypeCommandHandler : IRequestHandler<DeleteAssetTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteAssetTypeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteAssetTypeCommand request, CancellationToken cancellationToken)
        {
            AssetTypeEntity? entity = await _context.AssetTypeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            if (await _context.AssetEntities.AnyAsync(x => !x.IsDeleted && x.AssetTypeId == request.Id, cancellationToken))
                throw new InvalidOperationException("Loại tài sản đang được dùng — không thể xóa.");
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
