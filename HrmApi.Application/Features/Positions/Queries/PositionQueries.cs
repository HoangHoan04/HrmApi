using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Position;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Positions.Queries
{
    public class GetPositionsPagedQuery : PagedRequest, IRequest<PagedResult<PositionDto>>
    {
        public bool? IsDeleted { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionMasterId { get; set; }
    }

    public class GetPositionsPagedQueryHandler : IRequestHandler<GetPositionsPagedQuery, PagedResult<PositionDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPositionsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PositionDto>> Handle(GetPositionsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PositionEntities.AsNoTracking();

            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId);

            if (request.PartId.HasValue && request.PartId != Guid.Empty)
                query = query.Where(x => x.PartId == request.PartId);

            if (request.PositionMasterId.HasValue && request.PositionMasterId != Guid.Empty)
                query = query.Where(x => x.PositionMasterId == request.PositionMasterId);

            var totalCount = await query.CountAsync(cancellationToken);
            query = ApplySorting(query, request);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var masterIds = entities.Where(x => x.PositionMasterId.HasValue).Select(x => x.PositionMasterId!.Value).Distinct().ToList();
            var departmentIds = entities.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            var companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();

            var masterMap = masterIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.PositionMasterEntities.AsNoTracking()
                    .Where(x => masterIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var departmentMap = departmentIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.DepartmentEntities.AsNoTracking()
                    .Where(x => departmentIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var companyMap = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x =>
            {
                string? masterName = x.PositionMasterId.HasValue && masterMap.TryGetValue(x.PositionMasterId.Value, out var mName) ? mName : null;
                string? departmentName = x.DepartmentId.HasValue && departmentMap.TryGetValue(x.DepartmentId.Value, out var dName) ? dName : null;
                string? companyName = x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var cName) ? cName : null;
                return PositionMapper.ToDto(x, companyName, null, departmentName, null, masterName);
            }).ToList();

            return new PagedResult<PositionDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<Domain.Entities.Organization.PositionEntity> ApplySorting(
            IQueryable<Domain.Entities.Organization.PositionEntity> query,
            GetPositionsPagedQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SortField))
            {
                var isDesc = request.SortOrder?.ToLower() == "desc";
                return request.SortField.ToLower() switch
                {
                    "status" or "isdeleted" => isDesc ? query.OrderByDescending(x => x.IsDeleted) : query.OrderBy(x => x.IsDeleted),
                    "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt)
                };
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }

    public class GetPositionByIdQuery : IRequest<PositionDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetPositionByIdQueryHandler : IRequestHandler<GetPositionByIdQuery, PositionDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetPositionByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PositionDto?> Handle(GetPositionByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.PositionEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) return null;

            string? masterName = null;
            if (entity.PositionMasterId.HasValue)
            {
                masterName = await _context.PositionMasterEntities
                    .AsNoTracking()
                    .Where(x => x.Id == entity.PositionMasterId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? departmentName = null;
            if (entity.DepartmentId.HasValue)
            {
                departmentName = await _context.DepartmentEntities
                    .AsNoTracking()
                    .Where(x => x.Id == entity.DepartmentId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return PositionMapper.ToDto(entity, companyName, null, departmentName, null, masterName);
        }
    }

    public class GetPositionSelectBoxQuery : IRequest<List<PositionSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionMasterId { get; set; }
    }

    public class GetPositionSelectBoxQueryHandler : IRequestHandler<GetPositionSelectBoxQuery, List<PositionSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPositionSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PositionSelectBoxDto>> Handle(GetPositionSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PositionEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
                query = query.Where(x => x.Id != request.ExcludeId.Value);

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId);

            if (request.PositionMasterId.HasValue && request.PositionMasterId != Guid.Empty)
                query = query.Where(x => x.PositionMasterId == request.PositionMasterId);

            return await query
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PositionSelectBoxDto
                {
                    Id = x.Id,
                    PositionMasterId = x.PositionMasterId,
                    DepartmentId = x.DepartmentId
                })
                .ToListAsync(cancellationToken);
        }
    }
}
