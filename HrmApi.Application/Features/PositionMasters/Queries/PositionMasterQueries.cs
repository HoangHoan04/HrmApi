using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.PositionMaster;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PositionMasters.Queries
{
    public class GetPositionMastersPagedQuery : PagedRequest, IRequest<PagedResult<PositionMasterDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetPositionMastersPagedQueryHandler : IRequestHandler<GetPositionMastersPagedQuery, PagedResult<PositionMasterDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPositionMastersPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PositionMasterDto>> Handle(GetPositionMastersPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PositionMasterEntities.AsNoTracking();

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

            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            query = ApplySorting(query, request);

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

            var items = entities.Select(x =>
            {
                string? companyName = x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var cName) ? cName : null;
                return PositionMasterMapper.ToDto(x, companyName);
            }).ToList();

            return new PagedResult<PositionMasterDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<Domain.Entities.Organization.PositionMasterEntity> ApplySorting(
            IQueryable<Domain.Entities.Organization.PositionMasterEntity> query,
            GetPositionMastersPagedQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SortField))
            {
                var isDesc = request.SortOrder?.ToLower() == "desc";
                return request.SortField.ToLower() switch
                {
                    "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
                    "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                    "status" or "isdeleted" => isDesc ? query.OrderByDescending(x => x.IsDeleted) : query.OrderBy(x => x.IsDeleted),
                    "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt)
                };
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }

    public class GetPositionMasterByIdQuery : IRequest<PositionMasterDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetPositionMasterByIdQueryHandler : IRequestHandler<GetPositionMasterByIdQuery, PositionMasterDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetPositionMasterByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PositionMasterDto?> Handle(GetPositionMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionMasterEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) return null;

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return PositionMasterMapper.ToDto(entity, companyName);
        }
    }

    public class GetPositionMasterSelectBoxQuery : IRequest<List<PositionMasterSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetPositionMasterSelectBoxQueryHandler : IRequestHandler<GetPositionMasterSelectBoxQuery, List<PositionMasterSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPositionMasterSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PositionMasterSelectBoxDto>> Handle(GetPositionMasterSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PositionMasterEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
                query = query.Where(x => x.Id != request.ExcludeId.Value);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new PositionMasterSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId,
                    BranchId = x.BranchId
                })
                .ToListAsync(cancellationToken);
        }
    }
}
