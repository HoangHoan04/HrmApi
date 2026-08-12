using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Salary;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Salaries.Queries
{
    internal static class SalaryQueryHelper
    {
        public static async Task<List<SalaryDto>> MapAsync(
            IApplicationDbContext context,
            List<SalaryEntity> entities,
            CancellationToken cancellationToken)
        {
            if (entities.Count == 0) return [];

            List<Guid> salaryIds = entities.Select(x => x.Id).ToList();
            List<Guid> employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> configIds = entities.Where(x => x.SalaryConfigId.HasValue).Select(x => x.SalaryConfigId!.Value).Distinct().ToList();
            List<Guid> companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            List<Guid> branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            List<Guid> departmentIds = entities.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            List<Guid> positionIds = entities.Where(x => x.PositionId.HasValue).Select(x => x.PositionId!.Value).Distinct().ToList();

            Dictionary<Guid, List<SalaryLineItemEntity>> lines = await context.SalaryLineItemEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && salaryIds.Contains(x.SalaryId))
                .GroupBy(x => x.SalaryId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

            Dictionary<Guid, (string Code, string? Name)> employees = await context.EmployeeEntities.AsNoTracking()
                .Where(x => employeeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Code, x.FullName), cancellationToken);

            Dictionary<Guid, string> configs = configIds.Count == 0
                ? []
                : await context.SalaryConfigEntities.AsNoTracking()
                    .Where(x => configIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

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
                var rows = await context.PositionEntities.AsNoTracking()
                    .Where(x => positionIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.PositionMasterId })
                    .ToListAsync(cancellationToken);
                List<Guid> masterIds = rows.Where(x => x.PositionMasterId.HasValue)
                    .Select(x => x.PositionMasterId!.Value).Distinct().ToList();
                Dictionary<Guid, string> masters = masterIds.Count == 0
                    ? []
                    : await context.PositionMasterEntities.AsNoTracking()
                        .Where(x => masterIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
                foreach (var row in rows)
                {
                    if (row.PositionMasterId.HasValue && masters.TryGetValue(row.PositionMasterId.Value, out string? name))
                    {
                        positions[row.Id] = name;
                    }
                }
            }

            return entities.Select(x =>
            {
                if (lines.TryGetValue(x.Id, out List<SalaryLineItemEntity>? lineList))
                {
                    x.LineItems = lineList;
                }
                employees.TryGetValue(x.EmployeeId, out var emp);
                return SalaryMapper.ToDto(
                    x,
                    emp.Code,
                    emp.Name,
                    x.SalaryConfigId.HasValue && configs.TryGetValue(x.SalaryConfigId.Value, out string? cn) ? cn : null,
                    x.CompanyId.HasValue && companies.TryGetValue(x.CompanyId.Value, out string? company) ? company : null,
                    x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out string? branch) ? branch : null,
                    x.DepartmentId.HasValue && departments.TryGetValue(x.DepartmentId.Value, out string? dept) ? dept : null,
                    x.PositionId.HasValue && positions.TryGetValue(x.PositionId.Value, out string? pos) ? pos : null);
            }).ToList();
        }
    }

    public class GetSalariesPagedQuery : PagedRequest, IRequest<PagedResult<SalaryDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public string? Status { get; set; }
        public string? PeriodCode { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetSalariesPagedQueryHandler : IRequestHandler<GetSalariesPagedQuery, PagedResult<SalaryDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetSalariesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<SalaryDto>> Handle(GetSalariesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<SalaryEntity> query = _context.SalaryEntities.AsNoTracking();
            query = request.IsDeleted.HasValue
                ? query.Where(x => x.IsDeleted == request.IsDeleted.Value)
                : query.Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);
            if (request.Year.HasValue)
                query = query.Where(x => x.Year == request.Year);
            if (request.Month.HasValue)
                query = query.Where(x => x.Month == request.Month);
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(x => x.Status == request.Status.Trim());
            if (!string.IsNullOrWhiteSpace(request.PeriodCode))
            {
                string code = request.PeriodCode.Trim().ToLower();
                query = query.Where(x => x.PeriodCode.ToLower().Contains(code));
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim().ToLower();
                List<Guid> empIds = await _context.EmployeeEntities.AsNoTracking()
                    .Where(e => !e.IsDeleted && (
                        (e.FullName != null && e.FullName.ToLower().Contains(search))
                        || e.Code.ToLower().Contains(search)))
                    .Select(e => e.Id)
                    .ToListAsync(cancellationToken);
                query = query.Where(x => x.PeriodCode.ToLower().Contains(search) || empIds.Contains(x.EmployeeId));
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ThenByDescending(x => x.CreatedAt);

            List<SalaryEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<SalaryDto> items = await SalaryQueryHelper.MapAsync(_context, entities, cancellationToken);
            return new PagedResult<SalaryDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetSalaryByIdQuery : IRequest<SalaryDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetSalaryByIdQueryHandler : IRequestHandler<GetSalaryByIdQuery, SalaryDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetSalaryByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<SalaryDto?> Handle(GetSalaryByIdQuery request, CancellationToken cancellationToken)
        {
            SalaryEntity? entity = await _context.SalaryEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return null;
            List<SalaryDto> items = await SalaryQueryHelper.MapAsync(_context, [entity], cancellationToken);
            return items.FirstOrDefault();
        }
    }

    public class GetMySalariesQuery : IRequest<List<SalaryDto>>
    {
        public int? Year { get; set; }
        public string? Status { get; set; }
    }

    public class GetMySalariesQueryHandler : IRequestHandler<GetMySalariesQuery, List<SalaryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMySalariesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<SalaryDto>> Handle(GetMySalariesQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = _currentUser.EmployeeId
                ?? throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");

            IQueryable<SalaryEntity> query = _context.SalaryEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.EmployeeId == employeeId
                    && x.Status != SalaryStatus.Cancelled
                    && x.Status != SalaryStatus.Draft);

            if (request.Year.HasValue)
                query = query.Where(x => x.Year == request.Year);
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(x => x.Status == request.Status.Trim());

            List<SalaryEntity> entities = await query
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToListAsync(cancellationToken);

            return await SalaryQueryHelper.MapAsync(_context, entities, cancellationToken);
        }
    }

    public class GetMySalaryDetailQuery : IRequest<SalaryDto?>
    {
        public Guid? Id { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class GetMySalaryDetailQueryHandler : IRequestHandler<GetMySalaryDetailQuery, SalaryDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMySalaryDetailQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<SalaryDto?> Handle(GetMySalaryDetailQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = _currentUser.EmployeeId
                ?? throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");

            IQueryable<SalaryEntity> query = _context.SalaryEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.EmployeeId == employeeId
                    && x.Status != SalaryStatus.Cancelled
                    && x.Status != SalaryStatus.Draft);

            if (request.Id.HasValue && request.Id != Guid.Empty)
                query = query.Where(x => x.Id == request.Id);
            else if (request.Year.HasValue && request.Month.HasValue)
                query = query.Where(x => x.Year == request.Year && x.Month == request.Month);
            else
                return null;

            SalaryEntity? entity = await query.FirstOrDefaultAsync(cancellationToken);
            if (entity == null) return null;
            List<SalaryDto> items = await SalaryQueryHelper.MapAsync(_context, [entity], cancellationToken);
            return items.FirstOrDefault();
        }
    }
}
