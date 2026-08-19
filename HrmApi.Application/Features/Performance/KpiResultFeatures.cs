using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Performance;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Performance
{
    public class GetKpiResultsPagedQuery : KpiResultPagedQuery, IRequest<PagedResult<KpiResultDto>> { }

    public class GetKpiResultsPagedQueryHandler : IRequestHandler<GetKpiResultsPagedQuery, PagedResult<KpiResultDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetKpiResultsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<KpiResultDto>> Handle(GetKpiResultsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<KpiResultEntity> query = _context.KpiResultEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.GoalId.HasValue && request.GoalId != Guid.Empty)
            {
                query = query.Where(x => x.GoalId == request.GoalId);
            }

            if (request.CycleId.HasValue && request.CycleId != Guid.Empty)
            {
                query = from r in query
                        join g in _context.KpiGoalEntities.AsNoTracking().Where(x => !x.IsDeleted)
                            on r.GoalId equals g.Id
                        where g.CycleId == request.CycleId
                        select r;
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Comment != null && x.Comment.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<KpiResultEntity> rows = await query
                .OrderByDescending(x => x.RatedAt ?? x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<KpiResultDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<KpiResultDto>> MapManyAsync(List<KpiResultEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> goalIds = rows.Select(x => x.GoalId).Distinct().ToList();
            List<Guid> raterIds = rows.Where(x => x.RatedByEmployeeId.HasValue).Select(x => x.RatedByEmployeeId!.Value).Distinct().ToList();

            Dictionary<Guid, string> goals = await _context.KpiGoalEntities.AsNoTracking()
                .Where(x => goalIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
            Dictionary<Guid, string> raters = raterIds.Count == 0 ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => raterIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.FullName ?? (x.LastName + " " + x.FirstName).Trim(), cancellationToken);

            return rows.Select(e => new KpiResultDto
            {
                Id = e.Id,
                GoalId = e.GoalId,
                GoalTitle = goals.GetValueOrDefault(e.GoalId),
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

    public class GetKpiResultByIdQuery : IdRequest, IRequest<KpiResultDto?> { }

    public class GetKpiResultByIdQueryHandler : IRequestHandler<GetKpiResultByIdQuery, KpiResultDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetKpiResultsPagedQueryHandler _mapper;
        public GetKpiResultByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetKpiResultsPagedQueryHandler(context);
        }

        public async Task<KpiResultDto?> Handle(GetKpiResultByIdQuery request, CancellationToken cancellationToken)
        {
            KpiResultEntity? e = await _context.KpiResultEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateKpiResultCommand : KpiResultCommandFields, IRequest<Guid> { }

    public class CreateKpiResultCommandHandler : IRequestHandler<CreateKpiResultCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateKpiResultCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateKpiResultCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, cancellationToken);
            KpiResultEntity entity = new()
            {
                GoalId = request.GoalId!.Value,
                ActualValue = request.ActualValue ?? 0,
                Score = request.Score ?? 0,
                Comment = request.Comment,
                RatedByEmployeeId = request.RatedByEmployeeId,
                RatedAt = request.RatedAt ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.KpiResultEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(KpiResultCommandFields request, CancellationToken cancellationToken)
        {
            if (!request.GoalId.HasValue || request.GoalId == Guid.Empty)
            {
                throw new InvalidOperationException("Mục tiêu KPI (parent) là bắt buộc.");
            }

            if (!await _context.KpiGoalEntities.AnyAsync(x => x.Id == request.GoalId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Mục tiêu KPI không tồn tại.");
            }

            if (request.RatedByEmployeeId.HasValue && request.RatedByEmployeeId != Guid.Empty
                && !await _context.EmployeeEntities.AnyAsync(x => x.Id == request.RatedByEmployeeId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Người đánh giá không tồn tại.");
            }
        }
    }

    public class UpdateKpiResultCommand : KpiResultCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateKpiResultCommandHandler : IRequestHandler<UpdateKpiResultCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateKpiResultCommandHandler _create;
        public UpdateKpiResultCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateKpiResultCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateKpiResultCommand request, CancellationToken cancellationToken)
        {
            KpiResultEntity? entity = await _context.KpiResultEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.GoalId ??= entity.GoalId;
            await _create.ValidateAsync(request, cancellationToken);

            entity.GoalId = request.GoalId!.Value;
            entity.ActualValue = request.ActualValue ?? entity.ActualValue;
            entity.Score = request.Score ?? entity.Score;
            entity.Comment = request.Comment;
            entity.RatedByEmployeeId = request.RatedByEmployeeId;
            entity.RatedAt = request.RatedAt ?? entity.RatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteKpiResultCommand : IdRequest, IRequest<bool> { }

    public class DeleteKpiResultCommandHandler : IRequestHandler<DeleteKpiResultCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteKpiResultCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteKpiResultCommand request, CancellationToken cancellationToken)
        {
            KpiResultEntity? entity = await _context.KpiResultEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
