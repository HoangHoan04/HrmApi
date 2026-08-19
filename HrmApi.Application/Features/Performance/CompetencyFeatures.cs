using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Performance;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Performance
{
    public class GetCompetencyFrameworksPagedQuery : CompetencyFrameworkPagedQuery, IRequest<PagedResult<CompetencyFrameworkDto>> { }

    public class GetCompetencyFrameworksPagedQueryHandler : IRequestHandler<GetCompetencyFrameworksPagedQuery, PagedResult<CompetencyFrameworkDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetCompetencyFrameworksPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CompetencyFrameworkDto>> Handle(GetCompetencyFrameworksPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CompetencyFrameworkEntity> query = _context.CompetencyFrameworkEntities.AsNoTracking().Where(x => !x.IsDeleted);
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
            List<CompetencyFrameworkEntity> rows = await query
                .OrderBy(x => x.Name)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<CompetencyFrameworkDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<CompetencyFrameworkDto>> MapManyAsync(List<CompetencyFrameworkEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> companyIds = rows.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            Dictionary<Guid, string> companies = companyIds.Count == 0 ? []
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e => new CompetencyFrameworkDto
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
    }

    public class GetCompetencyFrameworkByIdQuery : IdRequest, IRequest<CompetencyFrameworkDto?> { }

    public class GetCompetencyFrameworkByIdQueryHandler : IRequestHandler<GetCompetencyFrameworkByIdQuery, CompetencyFrameworkDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetCompetencyFrameworksPagedQueryHandler _mapper;
        public GetCompetencyFrameworkByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetCompetencyFrameworksPagedQueryHandler(context);
        }

        public async Task<CompetencyFrameworkDto?> Handle(GetCompetencyFrameworkByIdQuery request, CancellationToken cancellationToken)
        {
            CompetencyFrameworkEntity? e = await _context.CompetencyFrameworkEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateCompetencyFrameworkCommand : CompetencyFrameworkCommandFields, IRequest<Guid> { }

    public class CreateCompetencyFrameworkCommandHandler : IRequestHandler<CreateCompetencyFrameworkCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateCompetencyFrameworkCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateCompetencyFrameworkCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            CompetencyFrameworkEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                Name = request.Name!.Trim(),
                CompanyId = request.CompanyId,
                Description = request.Description,
                IsActive = request.IsActive ?? true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.CompetencyFrameworkEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(CompetencyFrameworkCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã khung năng lực là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên khung năng lực là bắt buộc.");
            }

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.CompetencyFrameworkEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Mã khung năng lực đã tồn tại.");
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty
                && !await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }
        }
    }

    public class UpdateCompetencyFrameworkCommand : CompetencyFrameworkCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateCompetencyFrameworkCommandHandler : IRequestHandler<UpdateCompetencyFrameworkCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateCompetencyFrameworkCommandHandler _create;
        public UpdateCompetencyFrameworkCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateCompetencyFrameworkCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateCompetencyFrameworkCommand request, CancellationToken cancellationToken)
        {
            CompetencyFrameworkEntity? entity = await _context.CompetencyFrameworkEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

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

    public class DeleteCompetencyFrameworkCommand : IdRequest, IRequest<bool> { }

    public class DeleteCompetencyFrameworkCommandHandler : IRequestHandler<DeleteCompetencyFrameworkCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteCompetencyFrameworkCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteCompetencyFrameworkCommand request, CancellationToken cancellationToken)
        {
            CompetencyFrameworkEntity? entity = await _context.CompetencyFrameworkEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
