using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Salary;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Payroll;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.SalaryConfigs.Queries
{
    public class GetSalaryConfigsPagedQuery : PagedRequest, IRequest<PagedResult<SalaryConfigDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetSalaryConfigsPagedQueryHandler : IRequestHandler<GetSalaryConfigsPagedQuery, PagedResult<SalaryConfigDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetSalaryConfigsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<SalaryConfigDto>> Handle(GetSalaryConfigsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<SalaryConfigEntity> query = _context.SalaryConfigEntities.AsNoTracking();
            query = request.IsDeleted.HasValue
                ? query.Where(x => x.IsDeleted == request.IsDeleted.Value)
                : query.Where(x => !x.IsDeleted);

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
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(search) || x.Name.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name);

            List<SalaryConfigEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<Guid> companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            Dictionary<Guid, string> companies = companyIds.Count == 0
                ? []
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<SalaryConfigDto> items = entities.Select(x =>
                SalaryConfigMapper.ToDto(
                    x,
                    x.CompanyId.HasValue && companies.TryGetValue(x.CompanyId.Value, out string? n) ? n : null))
                .ToList();

            return new PagedResult<SalaryConfigDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetSalaryConfigByIdQuery : IRequest<SalaryConfigDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetSalaryConfigByIdQueryHandler : IRequestHandler<GetSalaryConfigByIdQuery, SalaryConfigDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetSalaryConfigByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<SalaryConfigDto?> Handle(GetSalaryConfigByIdQuery request, CancellationToken cancellationToken)
        {
            SalaryConfigEntity? entity = await _context.SalaryConfigEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return null;

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            return SalaryConfigMapper.ToDto(entity, companyName);
        }
    }

    public class GetSalaryConfigSelectBoxQuery : IRequest<List<SalaryConfigSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; } = true;
    }

    public class GetSalaryConfigSelectBoxQueryHandler : IRequestHandler<GetSalaryConfigSelectBoxQuery, List<SalaryConfigSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetSalaryConfigSelectBoxQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<SalaryConfigSelectBoxDto>> Handle(GetSalaryConfigSelectBoxQuery request, CancellationToken cancellationToken)
        {
            IQueryable<SalaryConfigEntity> query = _context.SalaryConfigEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == null || x.CompanyId == request.CompanyId);

            List<SalaryConfigEntity> entities = await query
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
            return entities.Select(SalaryConfigMapper.ToSelectBox).ToList();
        }
    }
}
