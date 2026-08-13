using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.Common.Services;
using HrmApi.Application.DTOs.TransferEmployee;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.EmployeeMovement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.TransferEmployees.Queries
{
    internal static class TransferEmployeeQueryHelper
    {
        public static async Task<List<TransferEmployeeDto>> MapAsync(
            IApplicationDbContext context,
            List<TransferEmployeeEntity> entities,
            bool includeDetails,
            CancellationToken cancellationToken)
        {
            if (entities.Count == 0)
            {
                return [];
            }

            List<Guid> employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            Dictionary<Guid, (string Code, string? Name)> employees = await context.EmployeeEntities.AsNoTracking()
                .Where(x => employeeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Code, x.FullName), cancellationToken);

            Dictionary<Guid, List<TransferEmployeePositionDto>> detailMap = [];
            if (includeDetails)
            {
                List<Guid> transferIds = entities.Select(x => x.Id).ToList();
                List<TransferEmployeePositionEntity> details = await context.TransferEmployeePositionEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && transferIds.Contains(x.TransferEmployeeId))
                    .OrderBy(x => x.CreatedAt)
                    .ToListAsync(cancellationToken);

                detailMap = await MapDetailsAsync(context, details, cancellationToken);
            }

            return entities.Select(x =>
            {
                employees.TryGetValue(x.EmployeeId, out var emp);
                detailMap.TryGetValue(x.Id, out List<TransferEmployeePositionDto>? details);
                return TransferEmployeeMapper.ToDto(x, emp.Code, emp.Name, details);
            }).ToList();
        }

        public static async Task<Dictionary<Guid, List<TransferEmployeePositionDto>>> MapDetailsAsync(
            IApplicationDbContext context,
            List<TransferEmployeePositionEntity> details,
            CancellationToken cancellationToken)
        {
            if (details.Count == 0)
            {
                return [];
            }

            List<Guid> companyIds = details.SelectMany(x => new[] { x.OldCompanyId, x.NewCompanyId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            List<Guid> branchIds = details.SelectMany(x => new[] { x.OldBranchId, x.NewBranchId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            List<Guid> departmentIds = details.SelectMany(x => new[] { x.OldDepartmentId, x.NewDepartmentId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            List<Guid> partIds = details.SelectMany(x => new[] { x.OldPartId, x.NewPartId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            List<Guid> positionIds = details.SelectMany(x => new[] { x.OldPositionId, x.NewPositionId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();

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

            Dictionary<Guid, string> parts = [];
            if (partIds.Count > 0)
            {
                var partRows = await context.PartEntities.AsNoTracking()
                    .Where(x => partIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.Name, x.PartMasterId })
                    .ToListAsync(cancellationToken);
                List<Guid> masterIds = partRows.Where(x => string.IsNullOrWhiteSpace(x.Name) && x.PartMasterId.HasValue)
                    .Select(x => x.PartMasterId!.Value).Distinct().ToList();
                Dictionary<Guid, string> masters = masterIds.Count == 0
                    ? []
                    : await context.PartMasterEntities.AsNoTracking()
                        .Where(x => masterIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
                foreach (var row in partRows)
                {
                    string? name = row.Name;
                    if (string.IsNullOrWhiteSpace(name) && row.PartMasterId.HasValue
                        && masters.TryGetValue(row.PartMasterId.Value, out string? masterName))
                    {
                        name = masterName;
                    }
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        parts[row.Id] = name!;
                    }
                }
            }

            Dictionary<Guid, string> positions = [];
            if (positionIds.Count > 0)
            {
                var positionRows = await context.PositionEntities.AsNoTracking()
                    .Where(x => positionIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.PositionMasterId })
                    .ToListAsync(cancellationToken);
                List<Guid> masterIds = positionRows.Where(x => x.PositionMasterId.HasValue)
                    .Select(x => x.PositionMasterId!.Value).Distinct().ToList();
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

            string? Name(Dictionary<Guid, string> map, Guid? id)
                => id.HasValue && map.TryGetValue(id.Value, out string? n) ? n : null;

            return details.GroupBy(x => x.TransferEmployeeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => TransferEmployeeMapper.ToDetailDto(
                        x,
                        Name(companies, x.OldCompanyId),
                        Name(companies, x.NewCompanyId),
                        Name(branches, x.OldBranchId),
                        Name(branches, x.NewBranchId),
                        Name(departments, x.OldDepartmentId),
                        Name(departments, x.NewDepartmentId),
                        Name(parts, x.OldPartId),
                        Name(parts, x.NewPartId),
                        Name(positions, x.OldPositionId),
                        Name(positions, x.NewPositionId))).ToList());
        }
    }

    public class GetTransferEmployeesPagedQuery : PagedRequest, IRequest<PagedResult<TransferEmployeeDto>>
    {
        public string? Code { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? TransferType { get; set; }
        public string? Status { get; set; }
        public DateTime? EffectiveDateFrom { get; set; }
        public DateTime? EffectiveDateTo { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetTransferEmployeesPagedQueryHandler : IRequestHandler<GetTransferEmployeesPagedQuery, PagedResult<TransferEmployeeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetTransferEmployeesPagedQueryHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<PagedResult<TransferEmployeeDto>> Handle(GetTransferEmployeesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TransferEmployeeEntity> query = _context.TransferEmployeeEntities.AsNoTracking();
            query = await query.ApplyTransferEmployeeDataScopeAsync(
                _context.EmployeeEntities.AsNoTracking(),
                _dataScope,
                PermissionCodes.HrTransferView,
                cancellationToken);

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
            if (!string.IsNullOrWhiteSpace(request.TransferType))
            {
                string type = request.TransferType.Trim();
                query = query.Where(x => x.TransferType == type);
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string status = request.Status.Trim();
                query = query.Where(x => x.Status == status);
            }
            if (request.EffectiveDateFrom.HasValue)
            {
                query = query.Where(x => x.EffectiveDate >= request.EffectiveDateFrom.Value.Date);
            }
            if (request.EffectiveDateTo.HasValue)
            {
                query = query.Where(x => x.EffectiveDate <= request.EffectiveDateTo.Value.Date);
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
                ? query.OrderBy(x => x.EffectiveDate)
                : query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.CreatedAt);

            List<TransferEmployeeEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<TransferEmployeeDto> items = await TransferEmployeeQueryHelper.MapAsync(_context, entities, includeDetails: false, cancellationToken);
            return new PagedResult<TransferEmployeeDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetTransferEmployeeByIdQuery : IRequest<TransferEmployeeDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetTransferEmployeeByIdQueryHandler : IRequestHandler<GetTransferEmployeeByIdQuery, TransferEmployeeDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetTransferEmployeeByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TransferEmployeeDto?> Handle(GetTransferEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            TransferEmployeeEntity? entity = await _context.TransferEmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<TransferEmployeeDto> items = await TransferEmployeeQueryHelper.MapAsync(
                _context, [entity], includeDetails: true, cancellationToken);
            return items.FirstOrDefault();
        }
    }

    public class GetTransferEmployeeHistoryQuery : IRequest<List<TransferEmployeeDto>>
    {
        public Guid EmployeeId { get; set; }
    }

    public class GetTransferEmployeeHistoryQueryHandler : IRequestHandler<GetTransferEmployeeHistoryQuery, List<TransferEmployeeDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTransferEmployeeHistoryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TransferEmployeeDto>> Handle(GetTransferEmployeeHistoryQuery request, CancellationToken cancellationToken)
        {
            List<TransferEmployeeEntity> entities = await _context.TransferEmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == request.EmployeeId)
                .OrderByDescending(x => x.EffectiveDate)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return await TransferEmployeeQueryHelper.MapAsync(_context, entities, includeDetails: true, cancellationToken);
        }
    }

    public class GetEmployeeOrgSnapshotQuery : IRequest<TransferEmployeePositionDto?>
    {
        public Guid EmployeeId { get; set; }
    }

    public class GetEmployeeOrgSnapshotQueryHandler : IRequestHandler<GetEmployeeOrgSnapshotQuery, TransferEmployeePositionDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetEmployeeOrgSnapshotQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TransferEmployeePositionDto?> Handle(GetEmployeeOrgSnapshotQuery request, CancellationToken cancellationToken)
        {
            var employee = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == request.EmployeeId && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.CompanyId,
                    x.BranchId,
                    x.DepartmentId,
                    x.PartId,
                    x.PositionId
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (employee == null)
            {
                return null;
            }

            TransferEmployeePositionEntity fake = new()
            {
                EmployeeId = employee.Id,
                EffectiveDate = DateTime.UtcNow.Date,
                OldCompanyId = employee.CompanyId,
                NewCompanyId = employee.CompanyId,
                OldBranchId = employee.BranchId,
                NewBranchId = employee.BranchId,
                OldDepartmentId = employee.DepartmentId,
                NewDepartmentId = employee.DepartmentId,
                OldPartId = employee.PartId,
                NewPartId = employee.PartId,
                OldPositionId = employee.PositionId,
                NewPositionId = employee.PositionId,
            };

            var mapped = await TransferEmployeeQueryHelper.MapDetailsAsync(_context, [fake], cancellationToken);
            return mapped.Values.FirstOrDefault()?.FirstOrDefault();
        }
    }
}
