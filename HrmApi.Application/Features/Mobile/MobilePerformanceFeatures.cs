using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.DTOs.Performance;
using HrmApi.Application.Features.Performance;
using HrmApi.Domain.Entities.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMyKpiGoalsQuery : MobileMyGoalsQuery, IRequest<List<KpiGoalDto>> { }

    public class GetMyKpiGoalsQueryHandler : IRequestHandler<GetMyKpiGoalsQuery, List<KpiGoalDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyKpiGoalsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<KpiGoalDto>> Handle(GetMyKpiGoalsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            IQueryable<KpiGoalEntity> query = _context.KpiGoalEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId);
            if (request.CycleId.HasValue && request.CycleId != Guid.Empty)
            {
                query = query.Where(x => x.CycleId == request.CycleId);
            }

            List<KpiGoalEntity> rows = await query.OrderBy(x => x.Title).Take(500).ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> cycleIds = rows.Select(x => x.CycleId).Distinct().ToList();
            Dictionary<Guid, string> cycles = await _context.PerformanceReviewCycleEntities.AsNoTracking()
                .Where(x => cycleIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId)
                .Select(x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() })
                .FirstOrDefaultAsync(cancellationToken);

            return rows.Select(e => new KpiGoalDto
            {
                Id = e.Id,
                CycleId = e.CycleId,
                CycleName = cycles.GetValueOrDefault(e.CycleId),
                EmployeeId = e.EmployeeId,
                EmployeeCode = emp?.Code,
                EmployeeName = emp?.Name,
                Title = e.Title,
                TargetValue = e.TargetValue,
                Unit = e.Unit,
                Weight = e.Weight,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }
    }

    public class GetMyKpiResultsQuery : IRequest<List<KpiResultDto>>
    {
        public Guid? CycleId { get; set; }
    }

    public class GetMyKpiResultsQueryHandler : IRequestHandler<GetMyKpiResultsQuery, List<KpiResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyKpiResultsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<KpiResultDto>> Handle(GetMyKpiResultsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);

            IQueryable<KpiGoalEntity> goals = _context.KpiGoalEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId);
            if (request.CycleId.HasValue && request.CycleId != Guid.Empty)
            {
                goals = goals.Where(x => x.CycleId == request.CycleId);
            }

            List<Guid> goalIds = await goals.Select(x => x.Id).ToListAsync(cancellationToken);
            if (goalIds.Count == 0)
            {
                return [];
            }

            List<KpiResultEntity> rows = await _context.KpiResultEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && goalIds.Contains(x.GoalId))
                .OrderByDescending(x => x.RatedAt ?? x.CreatedAt)
                .Take(500)
                .ToListAsync(cancellationToken);

            Dictionary<Guid, string> goalTitles = await _context.KpiGoalEntities.AsNoTracking()
                .Where(x => goalIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
            List<Guid> raterIds = rows.Where(x => x.RatedByEmployeeId.HasValue).Select(x => x.RatedByEmployeeId!.Value).Distinct().ToList();
            Dictionary<Guid, string> raters = raterIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => raterIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.FullName ?? (x.LastName + " " + x.FirstName).Trim(), cancellationToken);

            return rows.Select(e => new KpiResultDto
            {
                Id = e.Id,
                GoalId = e.GoalId,
                GoalTitle = goalTitles.GetValueOrDefault(e.GoalId),
                ActualValue = e.ActualValue,
                Score = e.Score,
                Comment = e.Comment,
                RatedByEmployeeId = e.RatedByEmployeeId,
                RatedByEmployeeName = e.RatedByEmployeeId.HasValue ? raters.GetValueOrDefault(e.RatedByEmployeeId.Value) : null,
                RatedAt = e.RatedAt,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }
    }

    public class GetMyPerformance360Query : IRequest<List<Performance360ReviewDto>>
    {
        public Guid? CycleId { get; set; }
    }

    public class GetMyPerformance360QueryHandler : IRequestHandler<GetMyPerformance360Query, List<Performance360ReviewDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyPerformance360QueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<Performance360ReviewDto>> Handle(GetMyPerformance360Query request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            IQueryable<Performance360ReviewEntity> query = _context.Performance360ReviewEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && (x.SubjectEmployeeId == employeeId || x.ReviewerEmployeeId == employeeId));
            if (request.CycleId.HasValue && request.CycleId != Guid.Empty)
            {
                query = query.Where(x => x.CycleId == request.CycleId);
            }

            List<Performance360ReviewEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(500)
                .ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> cycleIds = rows.Select(x => x.CycleId).Distinct().ToList();
            List<Guid> employeeIds = rows.SelectMany(x => new[] { x.SubjectEmployeeId, x.ReviewerEmployeeId }).Distinct().ToList();
            Dictionary<Guid, string> cycles = await _context.PerformanceReviewCycleEntities.AsNoTracking()
                .Where(x => cycleIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => employeeIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() },
                    cancellationToken);

            return rows.Select(e =>
            {
                _ = employees.TryGetValue(e.SubjectEmployeeId, out var subject);
                _ = employees.TryGetValue(e.ReviewerEmployeeId, out var reviewer);
                return new Performance360ReviewDto
                {
                    Id = e.Id,
                    CycleId = e.CycleId,
                    CycleName = cycles.GetValueOrDefault(e.CycleId),
                    SubjectEmployeeId = e.SubjectEmployeeId,
                    SubjectEmployeeCode = subject?.Code,
                    SubjectEmployeeName = subject?.Name,
                    ReviewerEmployeeId = e.ReviewerEmployeeId,
                    ReviewerEmployeeCode = reviewer?.Code,
                    ReviewerEmployeeName = reviewer?.Name,
                    ReviewerType = e.ReviewerType,
                    Score = e.Score,
                    Comment = e.Comment,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }

    public class UpsertMyPerformance360Command : MobileUpsert360Request, IRequest<Guid> { }

    public class UpsertMyPerformance360CommandHandler : IRequestHandler<UpsertMyPerformance360Command, Guid>
    {
        private readonly IMediator _mediator;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public UpsertMyPerformance360CommandHandler(
            IMediator mediator,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(UpsertMyPerformance360Command request, CancellationToken cancellationToken)
        {
            Guid me = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);

            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                Performance360ReviewEntity existing = await _context.Performance360ReviewEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                    ?? throw new InvalidOperationException("Không tìm thấy đánh giá 360.");
                if (existing.ReviewerEmployeeId != me && existing.SubjectEmployeeId != me)
                {
                    throw new InvalidOperationException("Bạn không có quyền sửa đánh giá này.");
                }

                bool ok = await _mediator.Send(new UpdatePerformance360ReviewCommand
                {
                    Id = request.Id.Value,
                    CycleId = request.CycleId,
                    SubjectEmployeeId = request.SubjectEmployeeId,
                    ReviewerEmployeeId = me,
                    ReviewerType = request.ReviewerType,
                    Score = request.Score,
                    Comment = request.Comment,
                    Status = request.Status,
                }, cancellationToken);
                return !ok ? throw new InvalidOperationException("Không cập nhật được đánh giá 360.") : request.Id.Value;
            }

            return await _mediator.Send(new CreatePerformance360ReviewCommand
            {
                CycleId = request.CycleId,
                SubjectEmployeeId = request.SubjectEmployeeId,
                ReviewerEmployeeId = me,
                ReviewerType = request.ReviewerType,
                Score = request.Score,
                Comment = request.Comment,
                Status = request.Status,
            }, cancellationToken);
        }
    }
}
