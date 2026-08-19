using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.WorkSchedule;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.WorkSchedules.Queries
{
    public class GetWorkSchedulesPagedQuery : PagedRequest, IRequest<PagedResult<WorkScheduleDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetWorkSchedulesPagedQueryHandler : IRequestHandler<GetWorkSchedulesPagedQuery, PagedResult<WorkScheduleDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetWorkSchedulesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<WorkScheduleDto>> Handle(GetWorkSchedulesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<WorkScheduledEmployeeEntity> query = _context.WorkScheduledEmployeeEntities.AsNoTracking();

            query = request.IsDeleted.HasValue ? query.Where(x => x.IsDeleted == request.IsDeleted.Value) : query.Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.ShiftMasterId.HasValue && request.ShiftMasterId != Guid.Empty)
            {
                query = query.Where(x => x.ShiftMasterId == request.ShiftMasterId);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.WorkDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(x => x.WorkDate <= request.ToDate.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortOrder, "ascend", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.WorkDate)
                : query.OrderByDescending(x => x.WorkDate);

            List<WorkScheduledEmployeeEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<WorkScheduleDto> items = await MapDtosAsync(entities, cancellationToken);
            return new PagedResult<WorkScheduleDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private async Task<List<WorkScheduleDto>> MapDtosAsync(
            List<Domain.Entities.Timekeeping.WorkScheduledEmployeeEntity> entities,
            CancellationToken cancellationToken)
        {
            var employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            var shiftMasterIds = entities.Where(x => x.ShiftMasterId.HasValue).Select(x => x.ShiftMasterId!.Value).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            Dictionary<Guid, (string? Name, string Code)> employees = employeeIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => employeeIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => (Name: (string?)(x.FullName ?? $"{x.LastName} {x.FirstName}".Trim()), x.Code),
                        cancellationToken);

            Dictionary<Guid, (string Name, string Code)> shiftMasters = shiftMasterIds.Count == 0
                ? []
                : await _context.ShiftMasterEntities.AsNoTracking()
                    .Where(x => shiftMasterIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (x.Name, x.Code), cancellationToken);

            Dictionary<Guid, string> branches = branchIds.Count == 0
                ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return entities.Select(x =>
            {
                _ = employees.TryGetValue(x.EmployeeId, out (string? Name, string Code) emp);
                string? smName = null, smCode = null;
                if (x.ShiftMasterId.HasValue && shiftMasters.TryGetValue(x.ShiftMasterId.Value, out (string Name, string Code) sm))
                {
                    smName = sm.Name;
                    smCode = sm.Code;
                }
                string? branchName = null;
                if (x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out string? bn))
                {
                    branchName = bn;
                }

                return WorkScheduleMapper.ToDto(x, emp.Name, emp.Code, smName, smCode, branchName);
            }).ToList();
        }
    }

    public class GetWorkScheduleByIdQuery : IRequest<WorkScheduleDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetWorkScheduleByIdQueryHandler : IRequestHandler<GetWorkScheduleByIdQuery, WorkScheduleDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetWorkScheduleByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WorkScheduleDto?> Handle(GetWorkScheduleByIdQuery request, CancellationToken cancellationToken)
        {
            WorkScheduledEmployeeEntity? entity = await _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == entity.EmployeeId)
                .Select(x => new { x.FullName, x.LastName, x.FirstName, x.Code })
                .FirstOrDefaultAsync(cancellationToken);

            string? smName = null, smCode = null;
            if (entity.ShiftMasterId.HasValue)
            {
                var sm = await _context.ShiftMasterEntities.AsNoTracking()
                    .Where(x => x.Id == entity.ShiftMasterId)
                    .Select(x => new { x.Name, x.Code })
                    .FirstOrDefaultAsync(cancellationToken);
                smName = sm?.Name;
                smCode = sm?.Code;
            }

            string? branchName = null;
            if (entity.BranchId.HasValue)
            {
                branchName = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == entity.BranchId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return WorkScheduleMapper.ToDto(
                entity,
                emp?.FullName ?? $"{emp?.LastName} {emp?.FirstName}".Trim(),
                emp?.Code,
                smName,
                smCode,
                branchName);
        }
    }
}
