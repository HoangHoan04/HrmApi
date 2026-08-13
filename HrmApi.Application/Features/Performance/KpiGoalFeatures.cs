using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Performance;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Performance
{
    public class GetKpiGoalsPagedQuery : KpiGoalPagedQuery, IRequest<PagedResult<KpiGoalDto>> { }

    public class GetKpiGoalsPagedQueryHandler : IRequestHandler<GetKpiGoalsPagedQuery, PagedResult<KpiGoalDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetKpiGoalsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<KpiGoalDto>> Handle(GetKpiGoalsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<KpiGoalEntity> query = _context.KpiGoalEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CycleId.HasValue && request.CycleId != Guid.Empty)
                query = query.Where(x => x.CycleId == request.CycleId);
            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<KpiGoalEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<KpiGoalDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<KpiGoalDto>> MapManyAsync(List<KpiGoalEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return [];
            var cycleIds = rows.Select(x => x.CycleId).Distinct().ToList();
            var empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();

            var cycles = await _context.PerformanceReviewCycleEntities.AsNoTracking()
                .Where(x => cycleIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var emps = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() }, cancellationToken);

            return rows.Select(e =>
            {
                emps.TryGetValue(e.EmployeeId, out var emp);
                return new KpiGoalDto
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
                };
            }).ToList();
        }
    }

    public class GetKpiGoalByIdQuery : IdRequest, IRequest<KpiGoalDto?> { }

    public class GetKpiGoalByIdQueryHandler : IRequestHandler<GetKpiGoalByIdQuery, KpiGoalDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetKpiGoalsPagedQueryHandler _mapper;
        public GetKpiGoalByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetKpiGoalsPagedQueryHandler(context);
        }

        public async Task<KpiGoalDto?> Handle(GetKpiGoalByIdQuery request, CancellationToken cancellationToken)
        {
            KpiGoalEntity? e = await _context.KpiGoalEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (e == null) return null;
            return (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateKpiGoalCommand : KpiGoalCommandFields, IRequest<Guid> { }

    public class CreateKpiGoalCommandHandler : IRequestHandler<CreateKpiGoalCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateKpiGoalCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateKpiGoalCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, cancellationToken);
            KpiGoalEntity entity = new()
            {
                CycleId = request.CycleId!.Value,
                EmployeeId = request.EmployeeId!.Value,
                Title = request.Title!.Trim(),
                TargetValue = request.TargetValue ?? 0,
                Unit = request.Unit,
                Weight = request.Weight ?? 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.KpiGoalEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(KpiGoalCommandFields request, CancellationToken cancellationToken)
        {
            if (!request.CycleId.HasValue || request.CycleId == Guid.Empty)
                throw new InvalidOperationException("Chu kỳ đánh giá (parent) là bắt buộc.");
            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new InvalidOperationException("Tiêu đề mục tiêu là bắt buộc.");
            if (!await _context.PerformanceReviewCycleEntities.AnyAsync(x => x.Id == request.CycleId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Chu kỳ đánh giá không tồn tại.");
            if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Nhân viên không tồn tại.");
        }
    }

    public class UpdateKpiGoalCommand : KpiGoalCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateKpiGoalCommandHandler : IRequestHandler<UpdateKpiGoalCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateKpiGoalCommandHandler _create;
        public UpdateKpiGoalCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateKpiGoalCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateKpiGoalCommand request, CancellationToken cancellationToken)
        {
            KpiGoalEntity? entity = await _context.KpiGoalEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            request.CycleId ??= entity.CycleId;
            request.EmployeeId ??= entity.EmployeeId;
            request.Title ??= entity.Title;
            await _create.ValidateAsync(request, cancellationToken);

            entity.CycleId = request.CycleId!.Value;
            entity.EmployeeId = request.EmployeeId!.Value;
            entity.Title = request.Title!.Trim();
            entity.TargetValue = request.TargetValue ?? entity.TargetValue;
            entity.Unit = request.Unit;
            entity.Weight = request.Weight ?? entity.Weight;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteKpiGoalCommand : IdRequest, IRequest<bool> { }

    public class DeleteKpiGoalCommandHandler : IRequestHandler<DeleteKpiGoalCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteKpiGoalCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteKpiGoalCommand request, CancellationToken cancellationToken)
        {
            KpiGoalEntity? entity = await _context.KpiGoalEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
