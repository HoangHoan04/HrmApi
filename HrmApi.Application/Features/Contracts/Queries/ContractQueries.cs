using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Contract;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Contracts.Queries
{
    internal static class ContractQueryHelper
    {
        public static async Task<List<ContractDto>> MapAsync(
            IApplicationDbContext context,
            List<ContractEntity> entities,
            CancellationToken cancellationToken)
        {
            if (entities.Count == 0)
            {
                return [];
            }

            List<Guid> employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> typeIds = entities.Where(x => x.ContractTypeId.HasValue).Select(x => x.ContractTypeId!.Value).Distinct().ToList();
            List<Guid> companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            List<Guid> branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            List<Guid> departmentIds = entities.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            List<Guid> positionIds = entities.Where(x => x.PositionId.HasValue).Select(x => x.PositionId!.Value).Distinct().ToList();
            List<Guid> previousIds = entities.Where(x => x.PreviousContractId.HasValue).Select(x => x.PreviousContractId!.Value).Distinct().ToList();

            Dictionary<Guid, (string Code, string? Name)> employees = await context.EmployeeEntities.AsNoTracking()
                .Where(x => employeeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Code, x.FullName), cancellationToken);

            Dictionary<Guid, (string Code, string Name, int? Notify)> types = typeIds.Count == 0
                ? []
                : await context.ContractTypeEntities.AsNoTracking()
                    .Where(x => typeIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (x.Code, x.Name, x.NotifyBeforeExpiryDays), cancellationToken);

            Dictionary<Guid, string> companies = companyIds.Count == 0
                ? []
                : await context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> branches = branchIds.Count == 0
                ? []
                : await context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> departments = departmentIds.Count == 0
                ? []
                : await context.DepartmentEntities.AsNoTracking()
                    .Where(x => departmentIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> positions = [];
            if (positionIds.Count > 0)
            {
                var positionRows = await context.PositionEntities.AsNoTracking()
                    .Where(x => positionIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.PositionMasterId })
                    .ToListAsync(cancellationToken);
                List<Guid> masterIds = positionRows
                    .Where(x => x.PositionMasterId.HasValue)
                    .Select(x => x.PositionMasterId!.Value)
                    .Distinct()
                    .ToList();
                Dictionary<Guid, string> masters = masterIds.Count == 0
                    ? []
                    : await context.PositionMasterEntities.AsNoTracking()
                        .Where(x => masterIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
                foreach (var row in positionRows)
                {
                    if (row.PositionMasterId.HasValue
                        && masters.TryGetValue(row.PositionMasterId.Value, out string? masterName))
                    {
                        positions[row.Id] = masterName;
                    }
                }
            }

            Dictionary<Guid, string> previousCodes = previousIds.Count == 0
                ? []
                : await context.ContractEntities.AsNoTracking()
                    .Where(x => previousIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            return entities.Select(x =>
            {
                employees.TryGetValue(x.EmployeeId, out var emp);
                (string Code, string Name, int? Notify)? type = null;
                if (x.ContractTypeId.HasValue && types.TryGetValue(x.ContractTypeId.Value, out var t))
                {
                    type = t;
                }
                return ContractMapper.ToDto(
                    x,
                    emp.Code,
                    emp.Name,
                    type?.Code,
                    type?.Name,
                    x.CompanyId.HasValue && companies.TryGetValue(x.CompanyId.Value, out string? cn) ? cn : null,
                    x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out string? bn) ? bn : null,
                    x.DepartmentId.HasValue && departments.TryGetValue(x.DepartmentId.Value, out string? dn) ? dn : null,
                    x.PositionId.HasValue && positions.TryGetValue(x.PositionId.Value, out string? pn) ? pn : null,
                    x.PreviousContractId.HasValue && previousCodes.TryGetValue(x.PreviousContractId.Value, out string? pc) ? pc : null,
                    type?.Notify);
            }).ToList();
        }
    }

    public class GetContractsPagedQuery : PagedRequest, IRequest<PagedResult<ContractDto>>
    {
        public string? Code { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Status { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetContractsPagedQueryHandler : IRequestHandler<GetContractsPagedQuery, PagedResult<ContractDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetContractsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ContractDto>> Handle(GetContractsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ContractEntity> query = _context.ContractEntities.AsNoTracking();

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }
            else
            {
                query = query.Where(x => !x.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                string code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }
            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            }
            if (request.ContractTypeId.HasValue && request.ContractTypeId != Guid.Empty)
            {
                query = query.Where(x => x.ContractTypeId == request.ContractTypeId);
            }
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string status = request.Status.Trim();
                query = query.Where(x => x.Status == status);
            }
            if (request.EndDateFrom.HasValue)
            {
                query = query.Where(x => x.EndDate >= request.EndDateFrom.Value.Date);
            }
            if (request.EndDateTo.HasValue)
            {
                query = query.Where(x => x.EndDate <= request.EndDateTo.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                List<Guid> employeeIds = await _context.EmployeeEntities.AsNoTracking()
                    .Where(e => !e.IsDeleted && (
                        (e.FullName != null && e.FullName.ToLower().Contains(search))
                        || e.Code.ToLower().Contains(search)))
                    .Select(e => e.Id)
                    .ToListAsync(cancellationToken);
                query = query.Where(x => x.Code.ToLower().Contains(search) || employeeIds.Contains(x.EmployeeId));
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortOrder, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(request.SortOrder, "ascend", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.StartDate)
                : query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CreatedAt);

            List<ContractEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<ContractDto> items = await ContractQueryHelper.MapAsync(_context, entities, cancellationToken);
            return new PagedResult<ContractDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetContractByIdQuery : IRequest<ContractDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetContractByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
        {
            ContractEntity? entity = await _context.ContractEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<ContractDto> items = await ContractQueryHelper.MapAsync(_context, [entity], cancellationToken);
            return items.FirstOrDefault();
        }
    }

    public class GetContractHistoryQuery : IRequest<List<ContractDto>>
    {
        public Guid EmployeeId { get; set; }
    }

    public class GetContractHistoryQueryHandler : IRequestHandler<GetContractHistoryQuery, List<ContractDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetContractHistoryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContractDto>> Handle(GetContractHistoryQuery request, CancellationToken cancellationToken)
        {
            List<ContractEntity> entities = await _context.ContractEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == request.EmployeeId)
                .OrderBy(x => x.StartDate)
                .ToListAsync(cancellationToken);
            return await ContractQueryHelper.MapAsync(_context, entities, cancellationToken);
        }
    }

    public class GetExpiringSoonContractsQuery : IRequest<List<ContractDto>>
    {
        public int? WithinDays { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class GetExpiringSoonContractsQueryHandler : IRequestHandler<GetExpiringSoonContractsQuery, List<ContractDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetExpiringSoonContractsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContractDto>> Handle(GetExpiringSoonContractsQuery request, CancellationToken cancellationToken)
        {
            DateTime today = DateTime.UtcNow.Date;
            int withinDays = request.WithinDays is > 0 ? request.WithinDays.Value : 30;
            DateTime maxDate = today.AddDays(withinDays);

            IQueryable<ContractEntity> query = _context.ContractEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && (x.Status == ContractStatus.Active || x.Status == ContractStatus.ExpiringSoon)
                    && x.EndDate.HasValue
                    && x.EndDate.Value.Date >= today
                    && x.EndDate.Value.Date <= maxDate);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            List<ContractEntity> entities = await query.OrderBy(x => x.EndDate).ToListAsync(cancellationToken);
            return await ContractQueryHelper.MapAsync(_context, entities, cancellationToken);
        }
    }
}
