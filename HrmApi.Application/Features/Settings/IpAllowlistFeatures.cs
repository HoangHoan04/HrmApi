using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Settings
{
    public class GetIpAllowlistPagedQuery : SettingsPagedQuery, IRequest<PagedResult<IpAllowlistEntryDto>> { }

    public class GetIpAllowlistPagedQueryHandler : IRequestHandler<GetIpAllowlistPagedQuery, PagedResult<IpAllowlistEntryDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetIpAllowlistPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<IpAllowlistEntryDto>> Handle(GetIpAllowlistPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<IpAllowlistEntryEntity> query = _context.IpAllowlistEntryEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.CidrOrIp.ToLower().Contains(s) || (x.Note != null && x.Note.ToLower().Contains(s)));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<IpAllowlistEntryEntity> rows = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<IpAllowlistEntryDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static IpAllowlistEntryDto ToDto(IpAllowlistEntryEntity e) => new()
        {
            Id = e.Id,
            CidrOrIp = e.CidrOrIp,
            Note = e.Note,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };
    }

    public class GetIpAllowlistByIdQuery : SettingsIdRequest, IRequest<IpAllowlistEntryDto?> { }

    public class GetIpAllowlistByIdQueryHandler : IRequestHandler<GetIpAllowlistByIdQuery, IpAllowlistEntryDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetIpAllowlistByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<IpAllowlistEntryDto?> Handle(GetIpAllowlistByIdQuery request, CancellationToken cancellationToken)
        {
            IpAllowlistEntryEntity? e = await _context.IpAllowlistEntryEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetIpAllowlistPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateIpAllowlistCommand : IpAllowlistCommandFields, IRequest<IpAllowlistEntryDto> { }

    public class CreateIpAllowlistCommandHandler : IRequestHandler<CreateIpAllowlistCommand, IpAllowlistEntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IIpAllowlistCache _ipAllowlistCache;
        public CreateIpAllowlistCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IIpAllowlistCache ipAllowlistCache)
        {
            _context = context;
            _currentUser = currentUser;
            _ipAllowlistCache = ipAllowlistCache;
        }

        public async Task<IpAllowlistEntryDto> Handle(CreateIpAllowlistCommand request, CancellationToken cancellationToken)
        {
            string cidr = (request.CidrOrIp ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cidr))
                throw new InvalidOperationException("IP/CIDR là bắt buộc.");

            IpAllowlistEntryEntity entity = new()
            {
                CidrOrIp = cidr,
                Note = request.Note,
                IsActive = request.IsActive ?? true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.IpAllowlistEntryEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            _ipAllowlistCache.Invalidate();
            return GetIpAllowlistPagedQueryHandler.ToDto(entity);
        }
    }

    public class UpdateIpAllowlistCommand : IpAllowlistCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateIpAllowlistCommandHandler : IRequestHandler<UpdateIpAllowlistCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IIpAllowlistCache _ipAllowlistCache;
        public UpdateIpAllowlistCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IIpAllowlistCache ipAllowlistCache)
        {
            _context = context;
            _currentUser = currentUser;
            _ipAllowlistCache = ipAllowlistCache;
        }

        public async Task<bool> Handle(UpdateIpAllowlistCommand request, CancellationToken cancellationToken)
        {
            IpAllowlistEntryEntity? entity = await _context.IpAllowlistEntryEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            string cidr = (request.CidrOrIp ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cidr))
                throw new InvalidOperationException("IP/CIDR là bắt buộc.");

            entity.CidrOrIp = cidr;
            entity.Note = request.Note;
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            _ipAllowlistCache.Invalidate();
            return true;
        }
    }

    public class DeleteIpAllowlistCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteIpAllowlistCommandHandler : IRequestHandler<DeleteIpAllowlistCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IIpAllowlistCache _ipAllowlistCache;
        public DeleteIpAllowlistCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IIpAllowlistCache ipAllowlistCache)
        {
            _context = context;
            _currentUser = currentUser;
            _ipAllowlistCache = ipAllowlistCache;
        }

        public async Task<bool> Handle(DeleteIpAllowlistCommand request, CancellationToken cancellationToken)
        {
            IpAllowlistEntryEntity? entity = await _context.IpAllowlistEntryEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            _ipAllowlistCache.Invalidate();
            return true;
        }
    }
}
