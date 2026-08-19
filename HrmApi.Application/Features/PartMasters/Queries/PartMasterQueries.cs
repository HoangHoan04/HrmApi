using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.PartMaster;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PartMasters.Queries
{
    #region Paged Query
    public class GetPartMastersPagedQuery : PagedRequest, IRequest<PagedResult<PartMasterDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetPartMastersPagedQueryHandler : IRequestHandler<GetPartMastersPagedQuery, PagedResult<PartMasterDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPartMastersPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PartMasterDto>> Handle(GetPartMastersPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PartMasterEntity> query = _context.PartMasterEntities.AsNoTracking();

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

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, request);

            List<PartMasterEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<Guid> companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            List<Guid> branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            Dictionary<Guid, string> companyMap = companyIds.Count == 0
                ? []
                : await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> branchMap = branchIds.Count == 0
                ? []
                : await _context.BranchEntities
                    .AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<PartMasterDto> items = entities.Select(x =>
            {
                string? companyName = x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out string? cName) ? cName : null;
                string? branchName = x.BranchId.HasValue && branchMap.TryGetValue(x.BranchId.Value, out string? bName) ? bName : null;
                return PartMasterMapper.ToDto(x, companyName, branchName);
            }).ToList();

            return new PagedResult<PartMasterDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<Domain.Entities.Organization.PartMasterEntity> ApplySorting(
            IQueryable<Domain.Entities.Organization.PartMasterEntity> query,
            GetPartMastersPagedQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SortField))
            {
                bool isDesc = request.SortOrder?.ToLower() == "desc";
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
    public class GetPartMasterByIdQuery : IRequest<PartMasterDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetPartMasterByIdQueryHandler : IRequestHandler<GetPartMasterByIdQuery, PartMasterDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetPartMasterByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PartMasterDto?> Handle(GetPartMasterByIdQuery request, CancellationToken cancellationToken)
        {
            PartMasterEntity? partMaster = await _context.PartMasterEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (partMaster == null)
            {
                return null;
            }

            string? companyName = null;
            if (partMaster.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => x.Id == partMaster.CompanyId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return PartMasterMapper.ToDto(partMaster, companyName);
        }
    }
    #endregion

    #region Select Box Query
    public class GetPartMasterSelectBoxQuery : IRequest<List<PartMasterSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetPartMasterSelectBoxQueryHandler : IRequestHandler<GetPartMasterSelectBoxQuery, List<PartMasterSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPartMasterSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartMasterSelectBoxDto>> Handle(GetPartMasterSelectBoxQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PartMasterEntity> query = _context.PartMasterEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new PartMasterSelectBoxDto
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
    #endregion

    #region Cascade Query
    public class GetPartMastersByScopeQuery : IRequest<List<PartMasterSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetPartMastersByScopeQueryHandler : IRequestHandler<GetPartMastersByScopeQuery, List<PartMasterSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPartMastersByScopeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartMasterSelectBoxDto>> Handle(GetPartMastersByScopeQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PartMasterEntity> query = _context.PartMasterEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new PartMasterSelectBoxDto
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
    #endregion
}
