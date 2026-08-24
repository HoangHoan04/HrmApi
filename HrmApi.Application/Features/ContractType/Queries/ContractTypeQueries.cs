using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ContractType;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.ContractType.Queries
{
    public class GetContractTypesPagedQuery : PagedRequest, IRequest<PagedResult<ContractTypeDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetContractTypesPagedQueryHandler : IRequestHandler<GetContractTypesPagedQuery, PagedResult<ContractTypeDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetContractTypesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ContractTypeDto>> Handle(GetContractTypesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ContractTypeEntity> query = _context.ContractTypeEntities.AsNoTracking();

            query = request.IsDeleted.HasValue ? query.Where(x => x.IsDeleted == request.IsDeleted.Value) : query.Where(x => !x.IsDeleted);

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

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
                Guid targetCompanyId = request.CompanyId.Value;
                query = query.Where(x => x.CompanyIds.Contains(targetCompanyId) || x.CompanyId == targetCompanyId || (x.CompanyIds.Count == 0 && x.CompanyId == null));
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortOrder, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(request.SortOrder, "ascend", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
                : query.OrderByDescending(x => x.DisplayOrder).ThenByDescending(x => x.CreatedAt);

            List<ContractTypeEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<Guid> allCompanyIds = entities
                .SelectMany(x => (IEnumerable<Guid>)(x.CompanyIds.Count > 0 ? x.CompanyIds : (x.CompanyId.HasValue ? new[] { x.CompanyId.Value } : Array.Empty<Guid>())))
                .Distinct()
                .ToList();

            Dictionary<Guid, string> companyMap = allCompanyIds.Count == 0
                ? []
                : await _context.CompanyEntities
                    .Where(x => allCompanyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<ContractTypeDto> items = entities.Select(x =>
            {
                List<Guid> cIds = x.CompanyIds.Count > 0 ? x.CompanyIds : (x.CompanyId.HasValue ? [x.CompanyId.Value] : []);
                List<string> cNames = cIds.Where(id => companyMap.ContainsKey(id)).Select(id => companyMap[id]).ToList();
                return ContractTypeMapper.ToDto(x, cIds, cNames);
            }).ToList();

            return new PagedResult<ContractTypeDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetContractTypeByIdQuery : IRequest<ContractTypeDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetContractTypeByIdQueryHandler : IRequestHandler<GetContractTypeByIdQuery, ContractTypeDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetContractTypeByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContractTypeDto?> Handle(GetContractTypeByIdQuery request, CancellationToken cancellationToken)
        {
            ContractTypeEntity? entity = await _context.ContractTypeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<Guid> cIds = entity.CompanyIds.Count > 0 ? entity.CompanyIds : (entity.CompanyId.HasValue ? [entity.CompanyId.Value] : []);
            List<string> cNames = [];
            if (cIds.Count > 0)
            {
                cNames = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => cIds.Contains(x.Id))
                    .Select(x => x.Name)
                    .ToListAsync(cancellationToken);
            }
            return ContractTypeMapper.ToDto(entity, cIds, cNames);
        }
    }

    public class GetContractTypeSelectBoxQuery : IRequest<List<ContractTypeSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
    }

    public class GetContractTypeSelectBoxQueryHandler : IRequestHandler<GetContractTypeSelectBoxQuery, List<ContractTypeSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetContractTypeSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContractTypeSelectBoxDto>> Handle(GetContractTypeSelectBoxQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ContractTypeEntity> query = _context.ContractTypeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                Guid targetCompanyId = request.CompanyId.Value;
                query = query.Where(x => x.CompanyIds.Contains(targetCompanyId) || x.CompanyId == targetCompanyId || (x.CompanyIds.Count == 0 && x.CompanyId == null));
            }

            return await query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                .Select(x => new ContractTypeSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId ?? (x.CompanyIds.Count > 0 ? x.CompanyIds[0] : null),
                    CompanyIds = x.CompanyIds.Count > 0 ? x.CompanyIds : (x.CompanyId.HasValue ? new List<Guid> { x.CompanyId.Value } : new List<Guid>()),
                    IsProbation = x.IsProbation,
                    IsUnlimited = x.IsUnlimited,
                    DefaultDurationMonths = x.DefaultDurationMonths,
                    MaxRenewalTimes = x.MaxRenewalTimes,
                    NotifyBeforeExpiryDays = x.NotifyBeforeExpiryDays
                }).ToListAsync(cancellationToken);
        }
    }
}
