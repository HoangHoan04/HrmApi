using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Recruitment;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Recruitment
{
    public class GetInterviewSchedulesPagedQuery : InterviewSchedulePagedQuery, IRequest<PagedResult<InterviewScheduleDto>> { }

    public class GetInterviewSchedulesPagedQueryHandler : IRequestHandler<GetInterviewSchedulesPagedQuery, PagedResult<InterviewScheduleDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetInterviewSchedulesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<InterviewScheduleDto>> Handle(GetInterviewSchedulesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<InterviewScheduleEntity> query = _context.InterviewScheduleEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.CandidateId.HasValue && request.CandidateId != Guid.Empty)
            {
                query = query.Where(x => x.CandidateId == request.CandidateId);
            }

            if (request.HiringPlanId.HasValue && request.HiringPlanId != Guid.Empty)
            {
                query = query.Where(x => x.HiringPlanId == request.HiringPlanId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (request.From.HasValue)
            {
                query = query.Where(x => x.EndAt >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                query = query.Where(x => x.StartAt <= request.To.Value);
            }

            int total = await query.CountAsync(cancellationToken);
            List<InterviewScheduleEntity> rows = await query
                .OrderBy(x => x.StartAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<InterviewScheduleDto>(
                await MapManyAsync(rows, includeChildren: false, cancellationToken),
                total,
                request.PageIndex,
                request.PageSize);
        }

        internal async Task<List<InterviewScheduleDto>> MapManyAsync(
            List<InterviewScheduleEntity> rows,
            bool includeChildren,
            CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> candidateIds = rows.Select(x => x.CandidateId).Distinct().ToList();
            List<Guid> planIds = rows.Where(x => x.HiringPlanId.HasValue).Select(x => x.HiringPlanId!.Value).Distinct().ToList();
            List<Guid> scheduleIds = rows.Select(x => x.Id).ToList();

            var candidates = await _context.CandidateEntities.AsNoTracking()
                .Where(x => candidateIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, x.FullName }, cancellationToken);
            Dictionary<Guid, string> plans = planIds.Count == 0 ? []
                : await _context.HiringPlanEntities.AsNoTracking()
                    .Where(x => planIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, List<InterviewerDto>> interviewerMap = [];
            Dictionary<Guid, List<EvaluationDto>> evaluationMap = [];

            if (includeChildren)
            {
                List<InterviewInterviewerEntity> interviewers = await _context.InterviewInterviewerEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && scheduleIds.Contains(x.InterviewScheduleId))
                    .ToListAsync(cancellationToken);
                List<Guid> empIds = interviewers.Select(x => x.EmployeeId).Distinct().ToList();
                Dictionary<Guid, (string? Code, string? Name)> emps = empIds.Count == 0
                    ? []
                    : await _context.EmployeeEntities.AsNoTracking()
                        .Where(x => empIds.Contains(x.Id))
                        .ToDictionaryAsync(
                            x => x.Id,
                            x => ((string?)x.Code, (string?)(x.FullName ?? (x.LastName + " " + x.FirstName).Trim())),
                            cancellationToken);

                interviewerMap = interviewers
                    .GroupBy(x => x.InterviewScheduleId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(i =>
                        {
                            _ = emps.TryGetValue(i.EmployeeId, out (string? Code, string? Name) emp);
                            return RecruitmentMapper.ToDto(i, emp.Code, emp.Name);
                        }).ToList());

                List<InterviewEvaluationEntity> evaluations = await _context.InterviewEvaluationEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && scheduleIds.Contains(x.InterviewScheduleId))
                    .ToListAsync(cancellationToken);
                List<Guid> interviewerEmpIds = evaluations.Select(x => x.InterviewerEmployeeId).Distinct().ToList();
                List<Guid> criteriaIds = evaluations.Select(x => x.EvaluationCriteriaId).Distinct().ToList();
                Dictionary<Guid, string?> interviewerNames = interviewerEmpIds.Count == 0
                    ? []
                    : await _context.EmployeeEntities.AsNoTracking()
                        .Where(x => interviewerEmpIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => (string?)(x.FullName ?? (x.LastName + " " + x.FirstName).Trim()), cancellationToken);
                Dictionary<Guid, (string Code, string Name)> criteriaMeta = criteriaIds.Count == 0 ? []
                    : await _context.EvaluationCriteriaEntities.AsNoTracking()
                        .Where(x => criteriaIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => (x.Code, x.Name), cancellationToken);

                evaluationMap = evaluations
                    .GroupBy(x => x.InterviewScheduleId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(ev =>
                        {
                            _ = interviewerNames.TryGetValue(ev.InterviewerEmployeeId, out var iname);
                            _ = criteriaMeta.TryGetValue(ev.EvaluationCriteriaId, out (string Code, string Name) cmeta);
                            return RecruitmentMapper.ToDto(ev, iname, cmeta.Code, cmeta.Name);
                        }).ToList());
            }

            return rows.Select(e =>
            {
                _ = candidates.TryGetValue(e.CandidateId, out var c);
                return RecruitmentMapper.ToDto(
                    e,
                    c?.Code,
                    c?.FullName,
                    e.HiringPlanId.HasValue ? plans.GetValueOrDefault(e.HiringPlanId.Value) : null,
                    includeChildren ? interviewerMap.GetValueOrDefault(e.Id) : null,
                    includeChildren ? evaluationMap.GetValueOrDefault(e.Id) : null);
            }).ToList();
        }
    }

    public class GetInterviewScheduleByIdQuery : IdRequest, IRequest<InterviewScheduleDto?> { }

    public class GetInterviewScheduleByIdQueryHandler : IRequestHandler<GetInterviewScheduleByIdQuery, InterviewScheduleDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetInterviewSchedulesPagedQueryHandler _mapper;
        public GetInterviewScheduleByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetInterviewSchedulesPagedQueryHandler(context);
        }

        public async Task<InterviewScheduleDto?> Handle(GetInterviewScheduleByIdQuery request, CancellationToken cancellationToken)
        {
            InterviewScheduleEntity? entity = await _context.InterviewScheduleEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<InterviewScheduleDto> mapped = await _mapper.MapManyAsync([entity], includeChildren: true, cancellationToken);
            return mapped.FirstOrDefault();
        }
    }

    public class CreateInterviewScheduleCommand : InterviewScheduleCommandFields, IRequest<Guid> { }

    public class CreateInterviewScheduleCommandHandler : IRequestHandler<CreateInterviewScheduleCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateInterviewScheduleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateInterviewScheduleCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            InterviewScheduleEntity entity = new()
            {
                Round = request.Round ?? 1,
                Status = string.IsNullOrWhiteSpace(request.Status) ? InterviewStatus.Scheduled : request.Status.Trim().ToUpperInvariant(),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            RecruitmentMapper.Apply(entity, request);

            _ = _context.InterviewScheduleEntities.Add(entity);

            CandidateEntity? candidate = await _context.CandidateEntities
                .FirstOrDefaultAsync(x => x.Id == entity.CandidateId && !x.IsDeleted, cancellationToken);
            if (candidate != null
                && candidate.Status is CandidateStatus.New or CandidateStatus.Screening
                    or CandidateStatus.Waitlist or CandidateStatus.Interview)
            {
                candidate.Status = CandidateStatus.Interview;
                candidate.UpdatedAt = DateTime.UtcNow;
                candidate.UpdatedBy = _currentUser.UserId;
                if (entity.HiringPlanId.HasValue && !candidate.HiringPlanId.HasValue)
                {
                    candidate.HiringPlanId = entity.HiringPlanId;
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(InterviewScheduleCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (!request.CandidateId.HasValue || request.CandidateId == Guid.Empty)
            {
                throw new InvalidOperationException("Ứng viên là bắt buộc.");
            }

            if (!request.StartAt.HasValue || !request.EndAt.HasValue)
            {
                throw new InvalidOperationException("Thời gian bắt đầu/kết thúc là bắt buộc.");
            }

            if (request.EndAt < request.StartAt)
            {
                throw new InvalidOperationException("Thời gian kết thúc phải >= thời gian bắt đầu.");
            }

            if (request.Round.HasValue && request.Round.Value < 1)
            {
                throw new InvalidOperationException("Vòng phỏng vấn phải >= 1.");
            }

            bool candidateOk = await _context.CandidateEntities.AnyAsync(x => x.Id == request.CandidateId && !x.IsDeleted, cancellationToken);
            if (!candidateOk)
            {
                throw new InvalidOperationException("Ứng viên không tồn tại.");
            }

            Guid? planId = RecruitmentMapper.NullIfEmpty(request.HiringPlanId);
            if (!planId.HasValue)
            {
                planId = await _context.CandidateEntities.AsNoTracking()
                    .Where(x => x.Id == request.CandidateId)
                    .Select(x => x.HiringPlanId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (planId.HasValue)
            {
                bool planOk = await _context.HiringPlanEntities.AnyAsync(x => x.Id == planId && !x.IsDeleted, cancellationToken);
                if (!planOk)
                {
                    throw new InvalidOperationException("Kế hoạch tuyển dụng không tồn tại.");
                }

                _ = await _context.HiringPlanEntities.AsNoTracking()
                    .Where(x => x.Id == planId)
                    .Select(x => x.PositionId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            request.HiringPlanId = planId;
        }
    }

    public class UpdateInterviewScheduleCommand : InterviewScheduleCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateInterviewScheduleCommandHandler : IRequestHandler<UpdateInterviewScheduleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateInterviewScheduleCommandHandler _create;
        public UpdateInterviewScheduleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateInterviewScheduleCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateInterviewScheduleCommand request, CancellationToken cancellationToken)
        {
            InterviewScheduleEntity? entity = await _context.InterviewScheduleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.CandidateId ??= entity.CandidateId;
            request.StartAt ??= entity.StartAt;
            request.EndAt ??= entity.EndAt;
            request.Round ??= entity.Round;
            request.HiringPlanId ??= entity.HiringPlanId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            RecruitmentMapper.Apply(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class CancelInterviewScheduleCommand : IdRequest, IRequest<bool> { }

    public class CancelInterviewScheduleCommandHandler : IRequestHandler<CancelInterviewScheduleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CancelInterviewScheduleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(CancelInterviewScheduleCommand request, CancellationToken cancellationToken)
        {
            InterviewScheduleEntity? entity = await _context.InterviewScheduleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == InterviewStatus.Cancelled)
            {
                throw new InvalidOperationException("Lịch phỏng vấn đã bị hủy.");
            }

            entity.Status = InterviewStatus.Cancelled;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class CompleteInterviewScheduleCommand : IdRequest, IRequest<bool>
    {
        public bool MoveCandidateToWaitlist { get; set; } = true;
    }

    public class CompleteInterviewScheduleCommandHandler : IRequestHandler<CompleteInterviewScheduleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CompleteInterviewScheduleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(CompleteInterviewScheduleCommand request, CancellationToken cancellationToken)
        {
            InterviewScheduleEntity? entity = await _context.InterviewScheduleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == InterviewStatus.Cancelled)
            {
                throw new InvalidOperationException("Lịch đã hủy — không thể hoàn tất.");
            }

            if (entity.Status == InterviewStatus.Completed)
            {
                throw new InvalidOperationException("Lịch đã hoàn tất.");
            }

            entity.Status = InterviewStatus.Completed;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            if (request.MoveCandidateToWaitlist)
            {
                CandidateEntity? candidate = await _context.CandidateEntities
                    .FirstOrDefaultAsync(x => x.Id == entity.CandidateId && !x.IsDeleted, cancellationToken);
                if (candidate != null && candidate.Status == CandidateStatus.Interview)
                {
                    candidate.Status = CandidateStatus.Waitlist;
                    candidate.UpdatedAt = DateTime.UtcNow;
                    candidate.UpdatedBy = _currentUser.UserId;
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class SetInterviewersCommand : SetInterviewersFields, IRequest<bool> { }

    public class SetInterviewersCommandHandler : IRequestHandler<SetInterviewersCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public SetInterviewersCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(SetInterviewersCommand request, CancellationToken cancellationToken)
        {
            InterviewScheduleEntity? schedule = await _context.InterviewScheduleEntities
                .FirstOrDefaultAsync(x => x.Id == request.InterviewScheduleId && !x.IsDeleted, cancellationToken);
            if (schedule == null)
            {
                return false;
            }

            request.Interviewers ??= [];
            List<Guid> empIds = request.Interviewers.Select(x => x.EmployeeId).Distinct().ToList();
            if (empIds.Count != request.Interviewers.Count)
            {
                throw new InvalidOperationException("Không được trùng người phỏng vấn.");
            }

            if (empIds.Count > 0)
            {
                int found = await _context.EmployeeEntities.CountAsync(x => !x.IsDeleted && empIds.Contains(x.Id), cancellationToken);
                if (found != empIds.Count)
                {
                    throw new InvalidOperationException("Một hoặc nhiều nhân viên phỏng vấn không tồn tại.");
                }
            }

            List<InterviewInterviewerEntity> existing = await _context.InterviewInterviewerEntities
                .Where(x => x.InterviewScheduleId == request.InterviewScheduleId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            DateTime now = DateTime.UtcNow;
            foreach (InterviewInterviewerEntity row in existing)
            {
                row.IsDeleted = true;
                row.UpdatedAt = now;
                row.UpdatedBy = _currentUser.UserId;
            }

            foreach (InterviewerInputDto item in request.Interviewers)
            {
                _ = _context.InterviewInterviewerEntities.Add(new InterviewInterviewerEntity
                {
                    InterviewScheduleId = request.InterviewScheduleId,
                    EmployeeId = item.EmployeeId,
                    IsPrimary = item.IsPrimary,
                    IsDeleted = false,
                    CreatedAt = now,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                });
            }

            schedule.UpdatedAt = now;
            schedule.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class GetInterviewCalendarRangeQuery : InterviewCalendarRangeQuery, IRequest<List<InterviewScheduleDto>> { }

    public class GetInterviewCalendarRangeQueryHandler : IRequestHandler<GetInterviewCalendarRangeQuery, List<InterviewScheduleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetInterviewSchedulesPagedQueryHandler _mapper;
        public GetInterviewCalendarRangeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetInterviewSchedulesPagedQueryHandler(context);
        }

        public async Task<List<InterviewScheduleDto>> Handle(GetInterviewCalendarRangeQuery request, CancellationToken cancellationToken)
        {
            if (request.To < request.From)
            {
                throw new InvalidOperationException("Thời gian kết thúc phải >= thời gian bắt đầu.");
            }

            IQueryable<InterviewScheduleEntity> query = _context.InterviewScheduleEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EndAt >= request.From && x.StartAt <= request.To);

            if (request.HiringPlanId.HasValue && request.HiringPlanId != Guid.Empty)
            {
                query = query.Where(x => x.HiringPlanId == request.HiringPlanId);
            }

            if (request.CandidateId.HasValue && request.CandidateId != Guid.Empty)
            {
                query = query.Where(x => x.CandidateId == request.CandidateId);
            }

            List<InterviewScheduleEntity> rows = await query.OrderBy(x => x.StartAt).ToListAsync(cancellationToken);
            return await _mapper.MapManyAsync(rows, includeChildren: true, cancellationToken);
        }
    }

    public class UpsertInterviewEvaluationsCommand : UpsertInterviewEvaluationsFields, IRequest<bool> { }

    public class UpsertInterviewEvaluationsCommandHandler : IRequestHandler<UpsertInterviewEvaluationsCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpsertInterviewEvaluationsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpsertInterviewEvaluationsCommand request, CancellationToken cancellationToken)
        {
            InterviewScheduleEntity? schedule = await _context.InterviewScheduleEntities
                .FirstOrDefaultAsync(x => x.Id == request.InterviewScheduleId && !x.IsDeleted, cancellationToken);
            if (schedule == null)
            {
                return false;
            }

            if (request.InterviewerEmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Người đánh giá là bắt buộc.");
            }

            bool empOk = await _context.EmployeeEntities.AnyAsync(x => x.Id == request.InterviewerEmployeeId && !x.IsDeleted, cancellationToken);
            if (!empOk)
            {
                throw new InvalidOperationException("Người đánh giá không tồn tại.");
            }

            request.Evaluations ??= [];
            List<Guid> criteriaIds = request.Evaluations.Select(x => x.EvaluationCriteriaId).Distinct().ToList();
            if (criteriaIds.Count != request.Evaluations.Count)
            {
                throw new InvalidOperationException("Không được trùng tiêu chí đánh giá.");
            }

            Dictionary<Guid, decimal> maxScoreByCriteria = [];
            if (criteriaIds.Count > 0)
            {
                int found = await _context.EvaluationCriteriaEntities.CountAsync(x =>
                    !x.IsDeleted && criteriaIds.Contains(x.Id), cancellationToken);
                if (found != criteriaIds.Count)
                {
                    throw new InvalidOperationException("Một hoặc nhiều tiêu chí không tồn tại.");
                }

                if (schedule.HiringPlanId.HasValue)
                {
                    maxScoreByCriteria = await _context.HiringPlanCriteriaEntities.AsNoTracking()
                        .Where(x => x.HiringPlanId == schedule.HiringPlanId && !x.IsDeleted
                            && criteriaIds.Contains(x.EvaluationCriteriaId))
                        .ToDictionaryAsync(x => x.EvaluationCriteriaId, x => x.MaxScore, cancellationToken);
                }

                if (maxScoreByCriteria.Count == 0)
                {
                    maxScoreByCriteria = await _context.EvaluationCriteriaEntities.AsNoTracking()
                        .Where(x => !x.IsDeleted && criteriaIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.MaxScore > 0 ? x.MaxScore : 10m, cancellationToken);
                }
            }

            List<InterviewEvaluationEntity> existing = await _context.InterviewEvaluationEntities
                .Where(x =>
                    x.InterviewScheduleId == request.InterviewScheduleId
                    && x.InterviewerEmployeeId == request.InterviewerEmployeeId
                    && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            DateTime now = DateTime.UtcNow;
            foreach (EvaluationInputDto item in request.Evaluations)
            {
                if (item.Score < 0)
                {
                    throw new InvalidOperationException("Điểm đánh giá phải >= 0.");
                }

                if (maxScoreByCriteria.TryGetValue(item.EvaluationCriteriaId, out decimal max) && item.Score > max)
                {
                    throw new InvalidOperationException($"Điểm vượt MaxScore ({max}) của tiêu chí.");
                }

                InterviewEvaluationEntity? row = null;
                if (item.Id.HasValue && item.Id != Guid.Empty)
                {
                    row = existing.FirstOrDefault(x => x.Id == item.Id);
                }

                row ??= existing.FirstOrDefault(x => x.EvaluationCriteriaId == item.EvaluationCriteriaId);

                if (row == null)
                {
                    row = new InterviewEvaluationEntity
                    {
                        InterviewScheduleId = request.InterviewScheduleId,
                        InterviewerEmployeeId = request.InterviewerEmployeeId,
                        EvaluationCriteriaId = item.EvaluationCriteriaId,
                        IsDeleted = false,
                        CreatedAt = now,
                        CreatedBy = _currentUser.UserId ?? Guid.Empty,
                    };
                    _ = _context.InterviewEvaluationEntities.Add(row);
                    existing.Add(row);
                }

                row.Score = item.Score;
                row.Comment = string.IsNullOrWhiteSpace(item.Comment) ? null : item.Comment.Trim();
                row.UpdatedAt = now;
                row.UpdatedBy = _currentUser.UserId;
            }

            HashSet<Guid> keepCriteria = request.Evaluations.Select(x => x.EvaluationCriteriaId).ToHashSet();
            foreach (InterviewEvaluationEntity row in existing.Where(x => !keepCriteria.Contains(x.EvaluationCriteriaId)))
            {
                row.IsDeleted = true;
                row.UpdatedAt = now;
                row.UpdatedBy = _currentUser.UserId;
            }

            schedule.UpdatedAt = now;
            schedule.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class GetInterviewEvaluationsQuery : IdRequest, IRequest<List<EvaluationDto>>
    {
        // Id = InterviewScheduleId
    }

    public class GetInterviewEvaluationsQueryHandler : IRequestHandler<GetInterviewEvaluationsQuery, List<EvaluationDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetInterviewEvaluationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EvaluationDto>> Handle(GetInterviewEvaluationsQuery request, CancellationToken cancellationToken)
        {
            bool exists = await _context.InterviewScheduleEntities.AsNoTracking()
                .AnyAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (!exists)
            {
                return [];
            }

            List<InterviewEvaluationEntity> evaluations = await _context.InterviewEvaluationEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.InterviewScheduleId == request.Id)
                .ToListAsync(cancellationToken);

            List<Guid> empIds = evaluations.Select(x => x.InterviewerEmployeeId).Distinct().ToList();
            List<Guid> criteriaIds = evaluations.Select(x => x.EvaluationCriteriaId).Distinct().ToList();
            Dictionary<Guid, string?> emps = empIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => empIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (string?)(x.FullName ?? (x.LastName + " " + x.FirstName).Trim()), cancellationToken);
            Dictionary<Guid, (string Code, string Name)> criteria = criteriaIds.Count == 0 ? []
                : await _context.EvaluationCriteriaEntities.AsNoTracking()
                    .Where(x => criteriaIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (x.Code, x.Name), cancellationToken);

            return evaluations.Select(ev =>
            {
                _ = emps.TryGetValue(ev.InterviewerEmployeeId, out var iname);
                _ = criteria.TryGetValue(ev.EvaluationCriteriaId, out (string Code, string Name) cmeta);
                return RecruitmentMapper.ToDto(ev, iname, cmeta.Code, cmeta.Name);
            }).ToList();
        }
    }
}
