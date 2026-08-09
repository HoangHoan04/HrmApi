using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.TimeKeepingStandard;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.TimeKeepingStandards.Queries
{
    public class GetTimeKeepingStandardsPagedQuery : PagedRequest, IRequest<PagedResult<TimeKeepingStandardDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetTimeKeepingStandardsPagedQueryHandler : IRequestHandler<GetTimeKeepingStandardsPagedQuery, PagedResult<TimeKeepingStandardDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTimeKeepingStandardsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<TimeKeepingStandardDto>> Handle(GetTimeKeepingStandardsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TimeKeepingStandardEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortOrder, "ascend", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.Code)
                : query.OrderByDescending(x => x.CreatedAt);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var companyMap = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x => TimeKeepingStandardMapper.ToDto(
                x, x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var n) ? n : null)).ToList();

            return new PagedResult<TimeKeepingStandardDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetTimeKeepingStandardByIdQuery : IRequest<TimeKeepingStandardDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetTimeKeepingStandardByIdQueryHandler : IRequestHandler<GetTimeKeepingStandardByIdQuery, TimeKeepingStandardDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetTimeKeepingStandardByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<TimeKeepingStandardDto?> Handle(GetTimeKeepingStandardByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.TimeKeepingStandardEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return null;

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            return TimeKeepingStandardMapper.ToDto(entity, companyName);
        }
    }

    public class GetTimeKeepingStandardSelectBoxQuery : IRequest<List<TimeKeepingStandardSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
    }

    public class GetTimeKeepingStandardSelectBoxQueryHandler : IRequestHandler<GetTimeKeepingStandardSelectBoxQuery, List<TimeKeepingStandardSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTimeKeepingStandardSelectBoxQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<TimeKeepingStandardSelectBoxDto>> Handle(GetTimeKeepingStandardSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TimeKeepingStandardEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId || x.CompanyId == null);

            return await query.OrderBy(x => x.Name)
                .Select(x => new TimeKeepingStandardSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId
                }).ToListAsync(cancellationToken);
        }
    }
}
