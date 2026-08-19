using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.RegisterDayOffs.Queries
{
    public class GetLeaveCalendarRangeQuery : IRequest<List<LeaveCalendarEventDto>>
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public DayOffStatus? Status { get; set; }
        public bool IncludeHolidays { get; set; } = true;
        public bool IncludePending { get; set; } = true;
    }

    public class LeaveCalendarEventDto
    {
        public string EventType { get; set; } = LeaveCalendarEventType.Leave;
        public Guid? LeaveId { get; set; }
        public Guid? HolidayId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Status { get; set; }
        public Guid? DayOffConfigId { get; set; }
        public string? DayOffConfigName { get; set; }
        public LeaveSession? Session { get; set; }
        public decimal? TotalDays { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Reason { get; set; }
    }

    public class GetLeaveCalendarRangeQueryHandler
        : IRequestHandler<GetLeaveCalendarRangeQuery, List<LeaveCalendarEventDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetLeaveCalendarRangeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LeaveCalendarEventDto>> Handle(
            GetLeaveCalendarRangeQuery request,
            CancellationToken cancellationToken)
        {
            if (request.ToDate < request.FromDate)
            {
                throw new InvalidOperationException("Đến ngày phải >= Từ ngày.");
            }

            List<LeaveCalendarEventDto> events = [];

            IQueryable<RegisterDayOffEntity> leaveQuery = _context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.ToDate >= request.FromDate
                    && x.FromDate <= request.ToDate);

            if (request.Status.HasValue)
            {
                leaveQuery = leaveQuery.Where(x => x.Status == request.Status.Value);
            }
            else
            {
                List<DayOffStatus> statuses = [DayOffStatus.APPROVED];
                if (request.IncludePending)
                {
                    statuses.Add(DayOffStatus.PENDING);
                }

                leaveQuery = leaveQuery.Where(x => statuses.Contains(x.Status));
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                leaveQuery = leaveQuery.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                leaveQuery = leaveQuery.Where(x => x.BranchId == request.BranchId);
            }

            List<RegisterDayOffEntity> leaves = await leaveQuery
                .OrderBy(x => x.FromDate)
                .Take(2000)
                .ToListAsync(cancellationToken);

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty && leaves.Count > 0)
            {
                HashSet<Guid> deptEmpIds = (await _context.EmployeeEntities.AsNoTracking()
                    .Where(e => e.DepartmentId == request.DepartmentId && !e.IsDeleted)
                    .Select(e => e.Id)
                    .ToListAsync(cancellationToken)).ToHashSet();
                leaves = leaves.Where(x => deptEmpIds.Contains(x.EmployeeId)).ToList();
            }

            if (leaves.Count > 0)
            {
                List<Guid> empIds = leaves.Select(x => x.EmployeeId).Distinct().ToList();
                List<Guid> branchIds = leaves.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
                List<Guid> cfgIds = leaves.Where(x => x.DayOffConfigId.HasValue).Select(x => x.DayOffConfigId!.Value).Distinct().ToList();

                var empMap = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => empIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => new { x.Code, Name = x.FullName ?? ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim() },
                        cancellationToken);

                Dictionary<Guid, string> branchMap = branchIds.Count == 0
                    ? []
                    : await _context.BranchEntities.AsNoTracking()
                        .Where(x => branchIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

                Dictionary<Guid, string> cfgMap = cfgIds.Count == 0
                    ? []
                    : await _context.DayOffConfigEntities.AsNoTracking()
                        .Where(x => cfgIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

                foreach (RegisterDayOffEntity leave in leaves)
                {
                    _ = empMap.TryGetValue(leave.EmployeeId, out var emp);
                    string? branchName = leave.BranchId.HasValue && branchMap.TryGetValue(leave.BranchId.Value, out var bn) ? bn : null;
                    string? cfgName = leave.DayOffConfigId.HasValue && cfgMap.TryGetValue(leave.DayOffConfigId.Value, out var cn) ? cn : null;
                    string empLabel = emp != null
                        ? (!string.IsNullOrWhiteSpace(emp.Code) ? $"{emp.Code} - {emp.Name}" : emp.Name)
                        : "NV";

                    events.Add(new LeaveCalendarEventDto
                    {
                        EventType = LeaveCalendarEventType.Leave,
                        LeaveId = leave.Id,
                        Title = $"{empLabel} · {cfgName ?? "Nghỉ phép"}",
                        StartDate = leave.FromDate,
                        EndDate = leave.ToDate,
                        Status = leave.Status.ToString(),
                        DayOffConfigId = leave.DayOffConfigId,
                        Session = leave.Session,
                        TotalDays = leave.TotalDays,
                        EmployeeId = leave.EmployeeId,
                        EmployeeName = emp?.Name,
                        EmployeeCode = emp?.Code,
                        CompanyId = leave.CompanyId,
                        BranchId = leave.BranchId,
                        BranchName = branchName,
                        DayOffConfigName = cfgName,
                        Reason = leave.Reason,
                    });
                }
            }

            if (request.IncludeHolidays)
            {
                List<PublicHolidayEntity> holidays = await _context.PublicHolidayEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive)
                    .ToListAsync(cancellationToken);

                for (DateOnly d = request.FromDate; d <= request.ToDate; d = d.AddDays(1))
                {
                    foreach (PublicHolidayEntity? h in holidays)
                    {
                        if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty
                            && h.CompanyId.HasValue && h.CompanyId != request.CompanyId)
                        {
                            continue;
                        }

                        bool match = h.IsRecurringYearly
                            ? h.HolidayDate.Month == d.Month && h.HolidayDate.Day == d.Day
                            : h.HolidayDate == d;
                        if (!match)
                        {
                            continue;
                        }

                        events.Add(new LeaveCalendarEventDto
                        {
                            EventType = LeaveCalendarEventType.Holiday,
                            HolidayId = h.Id,
                            Title = h.Name,
                            StartDate = d,
                            EndDate = d,
                            CompanyId = h.CompanyId,
                            Status = LeaveCalendarEventType.Holiday,
                        });
                    }
                }
            }

            return events
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.EventType)
                .ThenBy(x => x.Title)
                .ToList();
        }
    }
}
