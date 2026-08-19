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
    public class GetAssetsPagedQuery : AssetPagedQuery, IRequest<PagedResult<AssetDto>> { }

    public class GetAssetsPagedQueryHandler : IRequestHandler<GetAssetsPagedQuery, PagedResult<AssetDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAssetsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AssetDto>> Handle(GetAssetsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetEntity> query = _context.AssetEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.AssetTypeId.HasValue && request.AssetTypeId != Guid.Empty)
            {
                query = query.Where(x => x.AssetTypeId == request.AssetTypeId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s)
                    || x.Name.ToLower().Contains(s)
                    || (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<AssetEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AssetDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<AssetDto>> MapManyAsync(List<AssetEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> assetIds = rows.Select(x => x.Id).ToList();
            List<Guid> typeIds = rows.Select(x => x.AssetTypeId).Distinct().ToList();
            List<Guid> companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();
            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            Dictionary<Guid, string> types = await _context.AssetTypeEntities.AsNoTracking()
                .Where(x => typeIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> branches = branchIds.Count == 0
                ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, AssetAssignmentEntity> activeAssignments = await _context.AssetAssignmentEntities.AsNoTracking()
                .Include(x => x.Employee)
                .Where(x => !x.IsDeleted && assetIds.Contains(x.AssetId) && !x.ReturnedAt.HasValue)
                .ToDictionaryAsync(x => x.AssetId, x => x, cancellationToken);

            return rows.Select(e =>
            {
                _ = activeAssignments.TryGetValue(e.Id, out AssetAssignmentEntity? assignment);
                string? empName = assignment?.Employee != null ? (assignment.Employee.FullName ?? $"{assignment.Employee.LastName} {assignment.Employee.FirstName}".Trim()) : null;

                return new AssetDto
                {
                    Id = e.Id,
                    Code = e.Code,
                    Name = e.Name,
                    AssetTypeId = e.AssetTypeId,
                    AssetTypeName = types.GetValueOrDefault(e.AssetTypeId),
                    CompanyId = e.CompanyId,
                    CompanyName = companies.GetValueOrDefault(e.CompanyId),
                    BranchId = e.BranchId,
                    BranchName = e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                    SerialNumber = e.SerialNumber,
                    PurchaseDate = e.PurchaseDate,
                    PurchaseCost = e.PurchaseCost,
                    WarrantyExpiryDate = e.WarrantyExpiryDate,
                    Vendor = e.Vendor,
                    Model = e.Model,
                    Location = e.Location,
                    Status = e.Status,
                    Note = e.Note,
                    CurrentHolderEmployeeId = assignment?.EmployeeId,
                    CurrentHolderEmployeeCode = assignment?.Employee?.Code,
                    CurrentHolderEmployeeName = empName,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }

    public class GetAssetByIdQuery : IdRequest, IRequest<AssetDto?> { }

    public class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, AssetDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetAssetsPagedQueryHandler _mapper;
        public GetAssetByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetAssetsPagedQueryHandler(context);
        }

        public async Task<AssetDto?> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
        {
            AssetEntity? e = await _context.AssetEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class GetAssetSelectBoxQuery : IRequest<List<AssetSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
        public string? Status { get; set; }
    }

    public class AssetSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
    }

    public class GetAssetSelectBoxQueryHandler : IRequestHandler<GetAssetSelectBoxQuery, List<AssetSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAssetSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AssetSelectBoxDto>> Handle(GetAssetSelectBoxQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetEntity> query = _context.AssetEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            return await query.OrderBy(x => x.Code).Select(x => new AssetSelectBoxDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Status = x.Status,
                SerialNumber = x.SerialNumber
            }).ToListAsync(cancellationToken);
        }
    }

    public class CreateAssetCommand : AssetCommandFields, IRequest<Guid> { }

    public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public CreateAssetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            AssetEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                Name = request.Name!.Trim(),
                AssetTypeId = request.AssetTypeId!.Value,
                CompanyId = request.CompanyId!.Value,
                BranchId = request.BranchId,
                SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim(),
                PurchaseDate = request.PurchaseDate,
                PurchaseCost = request.PurchaseCost,
                WarrantyExpiryDate = request.WarrantyExpiryDate,
                Vendor = request.Vendor,
                Model = request.Model,
                Location = request.Location,
                Status = string.IsNullOrWhiteSpace(request.Status) ? AssetStatus.Available : request.Status.Trim().ToUpperInvariant(),
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.AssetEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "AssetEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.Name },
                "Tạo mới tài sản " + entity.Name);

            return entity.Id;
        }

        internal async Task ValidateAsync(AssetCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã tài sản là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên tài sản là bắt buộc.");
            }

            if (!request.AssetTypeId.HasValue || request.AssetTypeId == Guid.Empty)
            {
                throw new InvalidOperationException("Loại tài sản là bắt buộc.");
            }

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException("Công ty là bắt buộc.");
            }

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.AssetEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && x.CompanyId == request.CompanyId && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Mã tài sản đã tồn tại trong công ty.");
            }

            if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                string serial = request.SerialNumber.Trim().ToUpperInvariant();
                if (await _context.AssetEntities.AnyAsync(
                        x => !x.IsDeleted && x.SerialNumber != null && x.SerialNumber.ToUpper() == serial && x.CompanyId == request.CompanyId && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
                {
                    throw new InvalidOperationException("Số Serial đã tồn tại trong công ty.");
                }
            }

            if (!await _context.AssetTypeEntities.AnyAsync(x => x.Id == request.AssetTypeId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Loại tài sản không tồn tại.");
            }

            if (!await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty
                && !await _context.BranchEntities.AnyAsync(x => x.Id == request.BranchId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Chi nhánh không tồn tại.");
            }
        }
    }

    public class UpdateAssetCommand : AssetCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly CreateAssetCommandHandler _create;

        public UpdateAssetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _create = new CreateAssetCommandHandler(context, currentUser, actionLog);
        }

        public async Task<bool> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
        {
            AssetEntity? entity = await _context.AssetEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.Code ??= entity.Code;
            request.Name ??= entity.Name;
            request.AssetTypeId ??= entity.AssetTypeId;
            request.CompanyId ??= entity.CompanyId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.Name = request.Name!.Trim();
            entity.AssetTypeId = request.AssetTypeId!.Value;
            entity.CompanyId = request.CompanyId!.Value;
            entity.BranchId = request.BranchId;
            entity.SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim();
            entity.PurchaseDate = request.PurchaseDate ?? entity.PurchaseDate;
            entity.PurchaseCost = request.PurchaseCost ?? entity.PurchaseCost;
            entity.WarrantyExpiryDate = request.WarrantyExpiryDate ?? entity.WarrantyExpiryDate;
            entity.Vendor = request.Vendor ?? entity.Vendor;
            entity.Model = request.Model ?? entity.Model;
            entity.Location = request.Location ?? entity.Location;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                entity.Status = request.Status.Trim().ToUpperInvariant();
            }

            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "AssetEntity",
                entity.Id,
                null,
                new { entity.Id, entity.Code, entity.Name },
                "Cập nhật tài sản " + entity.Name);

            return true;
        }
    }

    public class DeleteAssetCommand : IdRequest, IRequest<bool> { }

    public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public DeleteAssetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
        {
            AssetEntity? entity = await _context.AssetEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            bool hasTickets = await _context.AssetTicketEntities.AnyAsync(x => x.AssetId == request.Id && !x.IsDeleted, cancellationToken);
            if (hasTickets)
            {
                throw new InvalidOperationException("Tài sản đã phát sinh phiếu giao dịch — không thể xóa.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DELETE,
                "AssetEntity",
                entity.Id,
                null,
                null,
                "Xóa tài sản " + entity.Name);

            return true;
        }
    }
}
