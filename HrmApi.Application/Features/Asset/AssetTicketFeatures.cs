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
    public class GetAssetTicketsPagedQuery : AssetTicketPagedQuery, IRequest<PagedResult<AssetTicketDto>> { }

    public class GetAssetTicketsPagedQueryHandler : IRequestHandler<GetAssetTicketsPagedQuery, PagedResult<AssetTicketDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetAssetTicketsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<AssetTicketDto>> Handle(GetAssetTicketsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetTicketEntity> query = _context.AssetTicketEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.AssetId.HasValue && request.AssetId != Guid.Empty)
                query = query.Where(x => x.AssetId == request.AssetId);
            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (!string.IsNullOrWhiteSpace(request.TicketType))
                query = query.Where(x => x.TicketType == request.TicketType.Trim().ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s)
                    || (x.Note != null && x.Note.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<AssetTicketEntity> rows = await query
                .OrderByDescending(x => x.TicketAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AssetTicketDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<AssetTicketDto>> MapManyAsync(List<AssetTicketEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return [];
            var assetIds = rows.Select(x => x.AssetId).Distinct().ToList();
            var empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
            var companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();

            var assets = await _context.AssetEntities.AsNoTracking()
                .Where(x => assetIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, x.Name }, cancellationToken);
            var emps = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() }, cancellationToken);
            var companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e =>
            {
                assets.TryGetValue(e.AssetId, out var asset);
                emps.TryGetValue(e.EmployeeId, out var emp);
                return new AssetTicketDto
                {
                    Id = e.Id,
                    Code = e.Code,
                    AssetId = e.AssetId,
                    AssetCode = asset?.Code,
                    AssetName = asset?.Name,
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = emp?.Code,
                    EmployeeName = emp?.Name,
                    CompanyId = e.CompanyId,
                    CompanyName = companies.GetValueOrDefault(e.CompanyId),
                    TicketType = e.TicketType,
                    Status = e.Status,
                    TicketAt = e.TicketAt,
                    Note = e.Note,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }

    public class GetAssetTicketByIdQuery : IdRequest, IRequest<AssetTicketDto?> { }

    public class GetAssetTicketByIdQueryHandler : IRequestHandler<GetAssetTicketByIdQuery, AssetTicketDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetAssetTicketsPagedQueryHandler _mapper;
        public GetAssetTicketByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetAssetTicketsPagedQueryHandler(context);
        }

        public async Task<AssetTicketDto?> Handle(GetAssetTicketByIdQuery request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? e = await _context.AssetTicketEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (e == null) return null;
            return (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateAssetTicketCommand : AssetTicketCommandFields, IRequest<Guid> { }

    public class CreateAssetTicketCommandHandler : IRequestHandler<CreateAssetTicketCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateAssetTicketCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            string ticketType = string.IsNullOrWhiteSpace(request.TicketType)
                ? AssetTicketType.Issue
                : request.TicketType.Trim().ToUpperInvariant();
            string status = string.IsNullOrWhiteSpace(request.Status)
                ? AssetTicketStatus.Draft
                : request.Status.Trim().ToUpperInvariant();

            AssetTicketEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                AssetId = request.AssetId!.Value,
                EmployeeId = request.EmployeeId!.Value,
                CompanyId = request.CompanyId!.Value,
                TicketType = ticketType,
                Status = status,
                TicketAt = request.TicketAt ?? DateTime.UtcNow,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.AssetTicketEntities.Add(entity);
            await ApplyAssetStatusAsync(entity.AssetId, ticketType, status, isCreate: true, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(AssetTicketCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code)) throw new InvalidOperationException("Mã phiếu là bắt buộc.");
            if (!request.AssetId.HasValue || request.AssetId == Guid.Empty)
                throw new InvalidOperationException("Tài sản là bắt buộc.");
            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
                throw new InvalidOperationException("Công ty là bắt buộc.");

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.AssetTicketEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
                throw new InvalidOperationException("Mã phiếu đã tồn tại.");
            if (!await _context.AssetEntities.AnyAsync(x => x.Id == request.AssetId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Tài sản không tồn tại.");
            if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Nhân viên không tồn tại.");
            if (!await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Công ty không tồn tại.");

            if (!string.IsNullOrWhiteSpace(request.TicketType))
            {
                string t = request.TicketType.Trim().ToUpperInvariant();
                if (t is not (AssetTicketType.Issue or AssetTicketType.Return))
                    throw new InvalidOperationException("Loại phiếu không hợp lệ.");
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string s = request.Status.Trim().ToUpperInvariant();
                if (s is not (AssetTicketStatus.Draft or AssetTicketStatus.Done or AssetTicketStatus.Cancelled))
                    throw new InvalidOperationException("Trạng thái phiếu không hợp lệ.");
            }
        }


        internal async Task ApplyAssetStatusAsync(
            Guid assetId,
            string ticketType,
            string status,
            bool isCreate,
            CancellationToken cancellationToken)
        {
            bool shouldAssign = ticketType == AssetTicketType.Issue
                && (isCreate || status == AssetTicketStatus.Done);
            bool shouldRelease = ticketType == AssetTicketType.Return
                && status == AssetTicketStatus.Done;

            if (!shouldAssign && !shouldRelease) return;

            AssetEntity? asset = await _context.AssetEntities
                .FirstOrDefaultAsync(x => x.Id == assetId && !x.IsDeleted, cancellationToken);
            if (asset == null) return;

            if (shouldAssign) asset.Status = AssetStatus.Assigned;
            else if (shouldRelease) asset.Status = AssetStatus.Available;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = _currentUser.UserId;
        }
    }

    public class UpdateAssetTicketCommand : AssetTicketCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateAssetTicketCommandHandler : IRequestHandler<UpdateAssetTicketCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateAssetTicketCommandHandler _create;
        public UpdateAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateAssetTicketCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateAssetTicketCommand request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? entity = await _context.AssetTicketEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            request.Code ??= entity.Code;
            request.AssetId ??= entity.AssetId;
            request.EmployeeId ??= entity.EmployeeId;
            request.CompanyId ??= entity.CompanyId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            string ticketType = string.IsNullOrWhiteSpace(request.TicketType)
                ? entity.TicketType
                : request.TicketType.Trim().ToUpperInvariant();
            string status = string.IsNullOrWhiteSpace(request.Status)
                ? entity.Status
                : request.Status.Trim().ToUpperInvariant();

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.AssetId = request.AssetId!.Value;
            entity.EmployeeId = request.EmployeeId!.Value;
            entity.CompanyId = request.CompanyId!.Value;
            entity.TicketType = ticketType;
            entity.Status = status;
            entity.TicketAt = request.TicketAt ?? entity.TicketAt;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await _create.ApplyAssetStatusAsync(entity.AssetId, ticketType, status, isCreate: false, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class CompleteAssetTicketCommand : IdRequest, IRequest<bool> { }

    public class CompleteAssetTicketCommandHandler : IRequestHandler<CompleteAssetTicketCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateAssetTicketCommandHandler _create;
        public CompleteAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateAssetTicketCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(CompleteAssetTicketCommand request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? entity = await _context.AssetTicketEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            if (entity.Status == AssetTicketStatus.Cancelled)
                throw new InvalidOperationException("Phiếu đã hủy — không thể hoàn tất.");
            if (entity.Status == AssetTicketStatus.Done)
                throw new InvalidOperationException("Phiếu đã hoàn tất.");

            entity.Status = AssetTicketStatus.Done;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            await _create.ApplyAssetStatusAsync(entity.AssetId, entity.TicketType, entity.Status, isCreate: false, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteAssetTicketCommand : IdRequest, IRequest<bool> { }

    public class DeleteAssetTicketCommandHandler : IRequestHandler<DeleteAssetTicketCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteAssetTicketCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteAssetTicketCommand request, CancellationToken cancellationToken)
        {
            AssetTicketEntity? entity = await _context.AssetTicketEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
