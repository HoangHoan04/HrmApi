using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Recruitment;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Recruitment
{
    public class GetHiringSourcesQuery : IRequest<List<HiringSourceDto>>
    {
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string? ChannelType { get; set; }
    }

    public class GetHiringSourcesQueryHandler : IRequestHandler<GetHiringSourcesQuery, List<HiringSourceDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public GetHiringSourcesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<HiringSourceDto>> Handle(GetHiringSourcesQuery request, CancellationToken cancellationToken)
        {
            await HiringSourceEnsure.EnsureSystemAsync(_context, _currentUser.UserId, cancellationToken);

            IQueryable<HiringSourceEntity> query = _context.HiringSourceEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.ChannelType))
            {
                string ch = request.ChannelType.Trim().ToUpperInvariant();
                query = query.Where(x => x.ChannelType == ch);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(s)
                    || x.Name.ToLower().Contains(s)
                    || (x.ContactEmail != null && x.ContactEmail.ToLower().Contains(s)));
            }

            List<HiringSourceEntity> rows = await query
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
            return rows.Select(RecruitmentMapper.ToDto).ToList();
        }
    }

    public class GetHiringSourceDetailQuery : IRequest<HiringSourceDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetHiringSourceDetailQueryHandler : IRequestHandler<GetHiringSourceDetailQuery, HiringSourceDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetHiringSourceDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HiringSourceDto?> Handle(GetHiringSourceDetailQuery request, CancellationToken cancellationToken)
        {
            HiringSourceEntity? e = await _context.HiringSourceEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : RecruitmentMapper.ToDto(e);
        }
    }

    public class CreateHiringSourceCommand : HiringSourceCommandFields, IRequest<Guid> { }

    public class CreateHiringSourceCommandHandler : IRequestHandler<CreateHiringSourceCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateHiringSourceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateHiringSourceCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            int nextOrder = request.DisplayOrder
                ?? ((await _context.HiringSourceEntities.Where(x => !x.IsDeleted).Select(x => (int?)x.DisplayOrder).MaxAsync(cancellationToken) ?? 0) + 10);

            HiringSourceEntity entity = new()
            {
                IsDeleted = false,
                IsSystem = false,
                IsActive = request.IsActive ?? true,
                ChannelType = string.IsNullOrWhiteSpace(request.ChannelType)
                    ? HiringSourceChannel.Other
                    : request.ChannelType.Trim().ToUpperInvariant(),
                DisplayOrder = nextOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            RecruitmentMapper.Apply(entity, request);
            if (string.IsNullOrWhiteSpace(entity.Code))
            {
                throw new InvalidOperationException("Mã nguồn tuyển là bắt buộc.");
            }

            entity.Code = entity.Code.Trim().ToUpperInvariant();

            _ = _context.HiringSourceEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(HiringSourceCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã nguồn tuyển là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên nguồn tuyển là bắt buộc.");
            }

            if (!string.IsNullOrWhiteSpace(request.ContactEmail) && !request.ContactEmail.Contains('@'))
            {
                throw new InvalidOperationException("Email liên hệ không hợp lệ.");
            }

            bool exists = await _context.HiringSourceEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == request.Code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã nguồn tuyển đã tồn tại.");
            }
        }
    }

    public class UpdateHiringSourceCommand : HiringSourceCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateHiringSourceCommandHandler : IRequestHandler<UpdateHiringSourceCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateHiringSourceCommandHandler _create;
        public UpdateHiringSourceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateHiringSourceCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateHiringSourceCommand request, CancellationToken cancellationToken)
        {
            HiringSourceEntity? entity = await _context.HiringSourceEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.IsSystem)
            {
                request.Code = entity.Code;
            }
            else
            {
                request.Code ??= entity.Code;
            }

            request.Name ??= entity.Name;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            RecruitmentMapper.Apply(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteHiringSourceCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteHiringSourceCommandHandler : IRequestHandler<DeleteHiringSourceCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteHiringSourceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteHiringSourceCommand request, CancellationToken cancellationToken)
        {
            HiringSourceEntity? entity = await _context.HiringSourceEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.IsSystem)
            {
                throw new InvalidOperationException("Không xóa được nguồn hệ thống. Hãy tắt hoạt động nếu không dùng.");
            }

            bool inUse = await _context.CandidateEntities.AnyAsync(x =>
                !x.IsDeleted && x.HiringSourceId == entity.Id, cancellationToken);
            if (inUse)
            {
                throw new InvalidOperationException("Nguồn đang gắn ứng viên — không thể xóa. Hãy tắt hoạt động.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    internal static class HiringSourceEnsure
    {
        public static async Task EnsureSystemAsync(
            IApplicationDbContext context,
            Guid? userId,
            CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            Guid createdBy = userId ?? Guid.Empty;
            List<HiringSourceEntity> existing = await context.HiringSourceEntities
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
            Dictionary<string, HiringSourceEntity> byCode = existing
                .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key.ToUpperInvariant(), g => g.First(), StringComparer.OrdinalIgnoreCase);

            bool dirty = false;
            foreach (HiringSourceCatalog.SeedItem seed in HiringSourceCatalog.SystemDefaults)
            {
                if (byCode.TryGetValue(seed.Code, out HiringSourceEntity? row))
                {
                    bool changed = false;
                    if (!row.IsSystem) { row.IsSystem = true; changed = true; }
                    if (row.DisplayOrder != seed.DisplayOrder) { row.DisplayOrder = seed.DisplayOrder; changed = true; }
                    if (row.ChannelType != seed.ChannelType) { row.ChannelType = seed.ChannelType; changed = true; }
                    if (string.IsNullOrWhiteSpace(row.Description) && !string.IsNullOrWhiteSpace(seed.Description))
                    {
                        row.Description = seed.Description;
                        changed = true;
                    }
                    if (changed)
                    {
                        row.UpdatedAt = now;
                        row.UpdatedBy = userId;
                        dirty = true;
                    }
                }
                else
                {
                    _ = context.HiringSourceEntities.Add(new HiringSourceEntity
                    {
                        Code = seed.Code,
                        Name = seed.Name,
                        Description = seed.Description,
                        ChannelType = seed.ChannelType,
                        DisplayOrder = seed.DisplayOrder,
                        IsSystem = true,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = now,
                        CreatedBy = createdBy,
                    });
                    dirty = true;
                }
            }

            if (dirty)
            {
                _ = await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
