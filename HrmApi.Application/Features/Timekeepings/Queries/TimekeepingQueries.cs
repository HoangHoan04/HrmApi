using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Timekeepings.Queries
{
    public class GetTimekeepingsPagedQuery : PagedRequest, IRequest<PagedResult<TimekeepingDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? Status { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetTimekeepingsPagedQueryHandler : IRequestHandler<GetTimekeepingsPagedQuery, PagedResult<TimekeepingDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTimekeepingsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<TimekeepingDto>> Handle(GetTimekeepingsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TimekeepingEntities.AsNoTracking();

            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            else
                query = query.Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);
            if (request.FromDate.HasValue)
                query = query.Where(x => x.WorkDate >= request.FromDate.Value);
            if (request.ToDate.HasValue)
                query = query.Where(x => x.WorkDate <= request.ToDate.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && System.Enum.TryParse<HrmApi.Domain.Enums.AttendanceStatus>(request.Status, true, out var parsedStatus))
                query = query.Where(x => x.Status == parsedStatus);

            var totalCount = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortOrder, "ascend", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.WorkDate)
                : query.OrderByDescending(x => x.WorkDate);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = await MapAsync(entities, cancellationToken);
            return new PagedResult<TimekeepingDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private async Task<List<TimekeepingDto>> MapAsync(
            List<Domain.Entities.Timekeeping.TimekeepingEntity> entities,
            CancellationToken cancellationToken)
        {
            var employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var smIds = entities.Where(x => x.ShiftMasterId.HasValue).Select(x => x.ShiftMasterId!.Value).Distinct().ToList();

            var employees = employeeIds.Count == 0
                ? new Dictionary<Guid, (string? Name, string Code)>()
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => employeeIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => (Name: (string?)(x.FullName ?? $"{x.LastName} {x.FirstName}".Trim()), Code: x.Code),
                        cancellationToken);

            var branches = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var shiftMasters = smIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.ShiftMasterEntities.AsNoTracking()
                    .Where(x => smIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return entities.Select(x =>
            {
                employees.TryGetValue(x.EmployeeId, out var emp);
                string? branchName = x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out var bn) ? bn : null;
                string? smName = x.ShiftMasterId.HasValue && shiftMasters.TryGetValue(x.ShiftMasterId.Value, out var sn) ? sn : null;
                return TimekeepingMapper.ToDto(x, emp.Name, emp.Code, branchName, smName);
            }).ToList();
        }
    }

    public class GetTimekeepingByIdQuery : IRequest<TimekeepingDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetTimekeepingByIdQueryHandler : IRequestHandler<GetTimekeepingByIdQuery, TimekeepingDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetTimekeepingByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<TimekeepingDto?> Handle(GetTimekeepingByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.TimekeepingEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return null;

            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == entity.EmployeeId)
                .Select(x => new { x.FullName, x.LastName, x.FirstName, x.Code })
                .FirstOrDefaultAsync(cancellationToken);

            string? branchName = null;
            if (entity.BranchId.HasValue)
            {
                branchName = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == entity.BranchId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? smName = null;
            if (entity.ShiftMasterId.HasValue)
            {
                smName = await _context.ShiftMasterEntities.AsNoTracking()
                    .Where(x => x.Id == entity.ShiftMasterId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return TimekeepingMapper.ToDto(
                entity,
                emp?.FullName ?? $"{emp?.LastName} {emp?.FirstName}".Trim(),
                emp?.Code,
                branchName,
                smName);
        }
    }

    public class GetTimekeepingSummariesPagedQuery : PagedRequest, IRequest<PagedResult<TimekeepingSummaryDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class GetTimekeepingSummariesPagedQueryHandler : IRequestHandler<GetTimekeepingSummariesPagedQuery, PagedResult<TimekeepingSummaryDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTimekeepingSummariesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<TimekeepingSummaryDto>> Handle(GetTimekeepingSummariesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TimekeepingSummaryEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);
            if (request.Year.HasValue)
                query = query.Where(x => x.Year == request.Year.Value);
            if (request.Month.HasValue)
                query = query.Where(x => x.Month == request.Month.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            query = query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            var employees = employeeIds.Count == 0
                ? new Dictionary<Guid, (string? Name, string Code)>()
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => employeeIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => (Name: (string?)(x.FullName ?? $"{x.LastName} {x.FirstName}".Trim()), Code: x.Code),
                        cancellationToken);

            var branches = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x =>
            {
                employees.TryGetValue(x.EmployeeId, out var emp);
                string? branchName = x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out var bn) ? bn : null;
                return TimekeepingMapper.ToSummaryDto(x, emp.Name, emp.Code, branchName);
            }).ToList();

            return new PagedResult<TimekeepingSummaryDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }
}
