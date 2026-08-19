using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Recruitment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMyInterviewSchedulesQuery : IRequest<List<InterviewScheduleDto>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class GetMyInterviewSchedulesQueryHandler
        : IRequestHandler<GetMyInterviewSchedulesQuery, List<InterviewScheduleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyInterviewSchedulesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<InterviewScheduleDto>> Handle(
            GetMyInterviewSchedulesQuery request,
            CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);

            List<Guid> scheduleIds = await _context.InterviewInterviewerEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId)
                .Select(x => x.InterviewScheduleId)
                .Distinct()
                .ToListAsync(cancellationToken);
            if (scheduleIds.Count == 0)
            {
                return [];
            }

            IQueryable<InterviewScheduleEntity> query = _context.InterviewScheduleEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && scheduleIds.Contains(x.Id));
            if (request.From.HasValue)
            {
                query = query.Where(x => x.EndAt >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                query = query.Where(x => x.StartAt <= request.To.Value);
            }

            List<InterviewScheduleEntity> rows = await query
                .OrderBy(x => x.StartAt)
                .Take(200)
                .ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> candidateIds = rows.Select(x => x.CandidateId).Distinct().ToList();
            List<Guid> planIds = rows.Where(x => x.HiringPlanId.HasValue).Select(x => x.HiringPlanId!.Value).Distinct().ToList();
            var candidates = await _context.CandidateEntities.AsNoTracking()
                .Where(x => candidateIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, x.FullName }, cancellationToken);
            Dictionary<Guid, string> plans = planIds.Count == 0
                ? []
                : await _context.HiringPlanEntities.AsNoTracking()
                    .Where(x => planIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e =>
            {
                _ = candidates.TryGetValue(e.CandidateId, out var c);
                return new InterviewScheduleDto
                {
                    Id = e.Id,
                    CandidateId = e.CandidateId,
                    CandidateCode = c?.Code,
                    CandidateName = c?.FullName,
                    HiringPlanId = e.HiringPlanId,
                    HiringPlanName = e.HiringPlanId.HasValue ? plans.GetValueOrDefault(e.HiringPlanId.Value) : null,
                    Round = e.Round,
                    StartAt = e.StartAt,
                    EndAt = e.EndAt,
                    Location = e.Location,
                    MeetingUrl = e.MeetingUrl,
                    Status = e.Status,
                    Notes = e.Notes,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }
}
