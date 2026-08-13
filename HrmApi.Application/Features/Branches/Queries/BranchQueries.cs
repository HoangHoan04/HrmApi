using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.Common.Services;
using HrmApi.Application.DTOs.Branch;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Branches.Queries
{
    #region Paged Query
    public class GetBranchesPagedQuery : PagedRequest, IRequest<PagedResult<BranchDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class GetBranchesPagedQueryHandler : IRequestHandler<GetBranchesPagedQuery, PagedResult<BranchDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetBranchesPagedQueryHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<PagedResult<BranchDto>> Handle(GetBranchesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.BranchEntities.AsNoTracking();
            query = await query.ApplyBranchDataScopeAsync(
                _dataScope, PermissionCodes.OrgBranchView, cancellationToken);

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

            if (!string.IsNullOrWhiteSpace(request.ShortName))
            {
                var shortName = request.ShortName.Trim().ToLower();
                query = query.Where(x => x.ShortName.ToLower().Contains(shortName));
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

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

            var companyIds = entities
                .Where(x => x.CompanyId.HasValue)
                .Select(x => x.CompanyId!.Value)
                .Distinct()
                .ToList();

            var parentIds = entities
                .Where(x => x.ParentBranchId.HasValue)
                .Select(x => x.ParentBranchId!.Value)
                .Distinct()
                .ToList();

            var companyMap = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var parentMap = parentIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities
                    .AsNoTracking()
                    .Where(x => parentIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x =>
            {
                string? companyName = x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var cn)
                    ? cn
                    : null;
                string? parentBranchName = x.ParentBranchId.HasValue && parentMap.TryGetValue(x.ParentBranchId.Value, out var pn)
                    ? pn
                    : null;
                return BranchMapper.ToDto(x, companyName, parentBranchName);
            }).ToList();

            return new PagedResult<BranchDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<Domain.Entities.Organization.BranchEntity> ApplySorting(
          IQueryable<Domain.Entities.Organization.BranchEntity> query,
          GetBranchesPagedQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SortField))
            {
                var isDesc = request.SortOrder?.ToLower() == "desc";
                return request.SortField.ToLower() switch
                {
                    "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
                    "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                    "status" or "isdeleted" => isDesc
                        ? query.OrderByDescending(x => x.IsDeleted)
                        : query.OrderBy(x => x.IsDeleted),
                    "createdat" => isDesc
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt)
                };
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
    #endregion

    #region Detail Query
    public class GetBranchByIdQuery : IRequest<BranchDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetBranchByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BranchDto?> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _context.BranchEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null) return null;

            string? companyName = null;
            if (branch.CompanyId.HasValue)
            {
                var company = await _context.CompanyEntities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == branch.CompanyId.Value, cancellationToken);
                companyName = company?.Name;
            }

            string? parentBranchName = null;
            if (branch.ParentBranchId.HasValue)
            {
                var parentBranch = await _context.BranchEntities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == branch.ParentBranchId.Value, cancellationToken);
                parentBranchName = parentBranch?.Name;
            }

            return BranchMapper.ToDto(branch, companyName, parentBranchName);
        }
    }
    #endregion

    #region Select Box Query
    public class GetBranchSelectBoxQuery : IRequest<List<BranchSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class GetBranchSelectBoxQueryHandler : IRequestHandler<GetBranchSelectBoxQuery, List<BranchSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetBranchSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchSelectBoxDto>> Handle(GetBranchSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.BranchEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new BranchSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion

    #region Cascade Query
    public class GetBranchesByCompanyQuery : IRequest<List<BranchSelectBoxDto>>
    {
        public Guid CompanyId { get; set; }
        public Guid? ExcludeId { get; set; }
    }

    public class GetBranchesByCompanyQueryHandler : IRequestHandler<GetBranchesByCompanyQuery, List<BranchSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetBranchesByCompanyQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchSelectBoxDto>> Handle(GetBranchesByCompanyQuery request, CancellationToken cancellationToken)
        {
            if (request.CompanyId == Guid.Empty)
                throw new InvalidOperationException("Id công ty là bắt buộc.");

            var query = _context.BranchEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == request.CompanyId);

            if (request.ExcludeId.HasValue)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new BranchSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion
}
