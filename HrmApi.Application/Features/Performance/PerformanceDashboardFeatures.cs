using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Performance
{
    public class GetPerformanceDashboardQuery : IRequest<PerformanceDashboardDto>
    {
        public Guid? CycleId { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class GetPerformanceDashboardQueryHandler : IRequestHandler<GetPerformanceDashboardQuery, PerformanceDashboardDto>
    {
        private static readonly string[] BandLabels = ["0-2", "2-4", "4-6", "6-8", "8-10"];

        private readonly IApplicationDbContext _context;
        public GetPerformanceDashboardQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PerformanceDashboardDto> Handle(GetPerformanceDashboardQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Domain.Entities.Performance.KpiGoalEntity> goalsQuery =
                _context.KpiGoalEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.CycleId.HasValue && request.CycleId != Guid.Empty)
            {
                goalsQuery = goalsQuery.Where(x => x.CycleId == request.CycleId);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                goalsQuery = from g in goalsQuery
                             join c in _context.PerformanceReviewCycleEntities.AsNoTracking().Where(x => !x.IsDeleted)
                                 on g.CycleId equals c.Id
                             where c.CompanyId == request.CompanyId
                             select g;
            }

            var goals = await goalsQuery
                .Select(g => new { g.Id, g.CycleId, g.EmployeeId, g.TargetValue })
                .ToListAsync(cancellationToken);

            List<Guid> goalIds = goals.Select(g => g.Id).ToList();
            var results = goalIds.Count == 0
                ? []
                : await _context.KpiResultEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && goalIds.Contains(x.GoalId))
                    .Select(r => new { r.GoalId, r.ActualValue, r.Score })
                    .ToListAsync(cancellationToken);

            Dictionary<Guid, decimal> goalTargetMap = goals.ToDictionary(g => g.Id, g => g.TargetValue);
            int goalCount = goals.Count;
            int resultCount = results.Count;
            decimal avgScore = resultCount == 0 ? 0 : Math.Round(results.Average(r => r.Score), 2);

            int metTarget = results.Count(r =>
                goalTargetMap.TryGetValue(r.GoalId, out decimal target) && r.ActualValue >= target);
            decimal metTargetPercent = resultCount == 0
                ? 0
                : Math.Round(metTarget * 100m / resultCount, 2);

            var bandCounts = BandLabels.ToDictionary(b => b, _ => 0);
            foreach (var r in results)
            {
                string band = ResolveBand(r.Score);
                bandCounts[band]++;
            }

            List<Guid> employeeIds = goals.Select(g => g.EmployeeId).Distinct().ToList();
            var employees = employeeIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => employeeIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DepartmentId, cancellationToken);

            List<Guid> deptIds = employees.Values.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var deptNames = deptIds.Count == 0
                ? []
                : await _context.DepartmentEntities.AsNoTracking()
                    .Where(x => deptIds.Contains(x.Id) && !x.IsDeleted)
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, Guid> goalEmployeeMap = goals.ToDictionary(g => g.Id, g => g.EmployeeId);
            List<DeptScoreDto> deptScores = results
                .Select(r =>
                {
                    Guid? deptId = null;
                    if (goalEmployeeMap.TryGetValue(r.GoalId, out var empId)
                        && employees.TryGetValue(empId, out var d))
                    {
                        deptId = d;
                    }

                    return new { DepartmentId = deptId, r.Score };
                })
                .GroupBy(x => x.DepartmentId)
                .Select(g => new DeptScoreDto
                {
                    DepartmentId = g.Key,
                    DepartmentName = g.Key.HasValue && deptNames.TryGetValue(g.Key.Value, out string? n)
                        ? n
                        : "Chưa gán phòng ban",
                    AvgScore = Math.Round(g.Average(x => x.Score), 2),
                    Count = g.Count(),
                })
                .OrderByDescending(x => x.AvgScore)
                .ToList();

            return new PerformanceDashboardDto
            {
                CycleId = request.CycleId,
                GoalCount = goalCount,
                ResultCount = resultCount,
                AvgScore = avgScore,
                MetTargetPercent = metTargetPercent,
                ScoreBands = BandLabels.Select(b => new ScoreBandDto { Band = b, Count = bandCounts[b] }).ToList(),
                DeptScores = deptScores,
            };
        }

        private static string ResolveBand(decimal score)
        {
            if (score < 2)
            {
                return "0-2";
            }

            if (score < 4)
            {
                return "2-4";
            }

            return score < 6 ? "4-6" : score < 8 ? "6-8" : "8-10";
        }
    }
}
