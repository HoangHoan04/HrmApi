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

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }
            else
            {
                query = query.Where(x => !x.IsDeleted);
            }

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
                query = query.Where(x => x.CompanyId == request.CompanyId);
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

            List<Guid> companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            Dictionary<Guid, string> companyMap = companyIds.Count == 0
                ? []
                : await _context.CompanyEntities
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<ContractTypeDto> items = entities.Select(x => ContractTypeMapper.ToDto(
                x, x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out string? n) ? n : null)).ToList();

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

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            return ContractTypeMapper.ToDto(entity, companyName);
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
                query = query.Where(x => x.CompanyId == request.CompanyId || x.CompanyId == null);
            }

            return await query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                .Select(x => new ContractTypeSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId,
                    IsProbation = x.IsProbation,
                    IsUnlimited = x.IsUnlimited,
                    DefaultDurationMonths = x.DefaultDurationMonths,
                    MaxRenewalTimes = x.MaxRenewalTimes,
                    NotifyBeforeExpiryDays = x.NotifyBeforeExpiryDays
                }).ToListAsync(cancellationToken);
        }
    }
}
