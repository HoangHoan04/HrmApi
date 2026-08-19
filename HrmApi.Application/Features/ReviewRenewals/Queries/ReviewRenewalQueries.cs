using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ReviewRenewal;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.ReviewRenewals.Queries
{
    internal static class ReviewRenewalQueryHelper
    {
        public static async Task<List<ReviewRenewalDto>> MapAsync(
            IApplicationDbContext context,
            List<ReviewRenewalEntity> entities,
            CancellationToken cancellationToken)
        {
            if (entities.Count == 0)
            {
                return [];
            }

            List<Guid> contractIds = entities.Select(x => x.ContractId).Distinct().ToList();
            List<Guid> employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> typeIds = entities.Where(x => x.ProposedContractTypeId.HasValue)
                .Select(x => x.ProposedContractTypeId!.Value).Distinct().ToList();
            List<Guid> newContractIds = entities.Where(x => x.NewContractId.HasValue)
                .Select(x => x.NewContractId!.Value).Distinct().ToList();

            Dictionary<Guid, (string Code, DateTime? EndDate)> contracts = await context.ContractEntities.AsNoTracking()
                .Where(x => contractIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Code, x.EndDate), cancellationToken);

            Dictionary<Guid, (string Code, string? Name)> employees = await context.EmployeeEntities.AsNoTracking()
                .Where(x => employeeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Code, x.FullName), cancellationToken);

            Dictionary<Guid, string> types = typeIds.Count == 0
                ? []
                : await context.ContractTypeEntities.AsNoTracking()
                    .Where(x => typeIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> newContracts = newContractIds.Count == 0
                ? []
                : await context.ContractEntities.AsNoTracking()
                    .Where(x => newContractIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            return entities.Select(x =>
            {
                _ = contracts.TryGetValue(x.ContractId, out (string Code, DateTime? EndDate) c);
                _ = employees.TryGetValue(x.EmployeeId, out (string Code, string? Name) e);
                return ReviewRenewalMapper.ToDto(
                    x,
                    c.Code,
                    e.Code,
                    e.Name,
                    x.ProposedContractTypeId.HasValue && types.TryGetValue(x.ProposedContractTypeId.Value, out string? tn) ? tn : null,
                    x.NewContractId.HasValue && newContracts.TryGetValue(x.NewContractId.Value, out string? nc) ? nc : null,
                    c.EndDate);
            }).ToList();
        }
    }

    public class GetReviewRenewalsPagedQuery : PagedRequest, IRequest<PagedResult<ReviewRenewalDto>>
    {
        public Guid? ContractId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Status { get; set; }
        public string? Recommendation { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetReviewRenewalsPagedQueryHandler : IRequestHandler<GetReviewRenewalsPagedQuery, PagedResult<ReviewRenewalDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetReviewRenewalsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ReviewRenewalDto>> Handle(GetReviewRenewalsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ReviewRenewalEntity> query = _context.ReviewRenewalEntities.AsNoTracking();

            query = request.IsDeleted.HasValue ? query.Where(x => x.IsDeleted == request.IsDeleted.Value) : query.Where(x => !x.IsDeleted);

            if (request.ContractId.HasValue && request.ContractId != Guid.Empty)
            {
                query = query.Where(x => x.ContractId == request.ContractId);
            }
            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string status = request.Status.Trim();
                query = query.Where(x => x.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(request.Recommendation))
            {
                string recommendation = request.Recommendation.Trim();
                query = query.Where(x => x.Recommendation == recommendation);
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortOrder, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(request.SortOrder, "ascend", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.CreatedAt)
                : query.OrderByDescending(x => x.CreatedAt);

            List<ReviewRenewalEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<ReviewRenewalDto> items = await ReviewRenewalQueryHelper.MapAsync(_context, entities, cancellationToken);
            return new PagedResult<ReviewRenewalDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetReviewRenewalByIdQuery : IRequest<ReviewRenewalDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetReviewRenewalByIdQueryHandler : IRequestHandler<GetReviewRenewalByIdQuery, ReviewRenewalDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetReviewRenewalByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewRenewalDto?> Handle(GetReviewRenewalByIdQuery request, CancellationToken cancellationToken)
        {
            ReviewRenewalEntity? entity = await _context.ReviewRenewalEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<ReviewRenewalDto> items = await ReviewRenewalQueryHelper.MapAsync(_context, [entity], cancellationToken);
            return items.FirstOrDefault();
        }
    }
}
