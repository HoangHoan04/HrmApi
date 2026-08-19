using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Company;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Companies.Queries
{
    #region Paged Query
    public class GetCompaniesPagedQuery : PagedRequest, IRequest<PagedResult<CompanyDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? ParentId { get; set; }
    }

    public class GetCompaniesPagedQueryHandler : IRequestHandler<GetCompaniesPagedQuery, PagedResult<CompanyDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCompaniesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CompanyDto>> Handle(GetCompaniesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CompanyEntity> query = _context.CompanyEntities.AsNoTracking();

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

            if (request.ParentId.HasValue && request.ParentId != Guid.Empty)
            {
                query = query.Where(x => x.ParentId == request.ParentId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, request);

            List<CompanyEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var parentIds = entities
                .Where(x => x.ParentId.HasValue)
                .Select(x => x.ParentId!.Value)
                .Distinct()
                .ToList();

            Dictionary<Guid, string> parentMap = parentIds.Count == 0
                ? []
                : await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => parentIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x =>
            {
                string? parentName = x.ParentId.HasValue && parentMap.TryGetValue(x.ParentId.Value, out string? name)
                    ? name
                    : null;
                return CompanyMapper.ToDto(x, parentName);
            }).ToList();

            return new PagedResult<CompanyDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<Domain.Entities.Organization.CompanyEntity> ApplySorting(
            IQueryable<Domain.Entities.Organization.CompanyEntity> query,
            GetCompaniesPagedQuery request)
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
    public class GetCompanyByIdQuery : IRequest<CompanyDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetCompanyByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            CompanyEntity? company = await _context.CompanyEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (company == null)
            {
                return null;
            }

            string? parentName = null;
            if (company.ParentId.HasValue)
            {
                parentName = await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => x.Id == company.ParentId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return CompanyMapper.ToDto(company, parentName);
        }
    }
    #endregion

    #region Select Box Query
    public class GetCompanySelectBoxQuery : IRequest<List<CompanySelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
    }

    public class GetCompanySelectBoxQueryHandler : IRequestHandler<GetCompanySelectBoxQuery, List<CompanySelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCompanySelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompanySelectBoxDto>> Handle(GetCompanySelectBoxQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CompanyEntity> query = _context.CompanyEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new CompanySelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion
}
