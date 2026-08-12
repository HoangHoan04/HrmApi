using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.DayOffConfig;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.DayOffConfigs.Queries
{
    public class GetDayOffConfigsPagedQuery : PagedRequest, IRequest<PagedResult<DayOffConfigDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public string? DayOffType { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetDayOffConfigsPagedQueryHandler : IRequestHandler<GetDayOffConfigsPagedQuery, PagedResult<DayOffConfigDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetDayOffConfigsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DayOffConfigDto>> Handle(GetDayOffConfigsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<DayOffConfigEntity> query = _context.DayOffConfigEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                string code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                string name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (!string.IsNullOrWhiteSpace(request.DayOffType)
            && Enum.TryParse<HrmApi.Domain.Enums.DayOffType>(request.DayOffType.Trim(), true, out DayOffType dayOffType))
            {
                query = query.Where(x => x.DayOffType == dayOffType);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortOrder, "ascend", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.Code)
                : query.OrderByDescending(x => x.CreatedAt);

            List<DayOffConfigEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<Guid> companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            Dictionary<Guid, string> companyMap = companyIds.Count == 0
                ? []
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<DayOffConfigDto> items = entities.Select(x => DayOffConfigMapper.ToDto(
                x, x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out string? n) ? n : null)).ToList();

            return new PagedResult<DayOffConfigDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetDayOffConfigByIdQuery : IRequest<DayOffConfigDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetDayOffConfigByIdQueryHandler : IRequestHandler<GetDayOffConfigByIdQuery, DayOffConfigDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetDayOffConfigByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DayOffConfigDto?> Handle(GetDayOffConfigByIdQuery request, CancellationToken cancellationToken)
        {
            DayOffConfigEntity? entity = await _context.DayOffConfigEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            return DayOffConfigMapper.ToDto(entity, companyName);
        }
    }

    public class GetDayOffConfigSelectBoxQuery : IRequest<List<DayOffConfigSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
    }

    public class GetDayOffConfigSelectBoxQueryHandler : IRequestHandler<GetDayOffConfigSelectBoxQuery, List<DayOffConfigSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetDayOffConfigSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DayOffConfigSelectBoxDto>> Handle(GetDayOffConfigSelectBoxQuery request, CancellationToken cancellationToken)
        {
            IQueryable<DayOffConfigEntity> query = _context.DayOffConfigEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId || x.CompanyId == null);
            }

            return await query.OrderBy(x => x.Name)
                .Select(x => new DayOffConfigSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    DayOffType = x.DayOffType,
                    CompanyId = x.CompanyId,
                    RequireAttachment = x.RequireAttachment,
                    DeductBalance = x.DeductBalance,
                }).ToListAsync(cancellationToken);
        }
    }
}
