using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Performance;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Performance;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Performance
{
    public class GetPerformance360ReviewsPagedQuery : Performance360ReviewPagedQuery, IRequest<PagedResult<Performance360ReviewDto>> { }

    public class GetPerformance360ReviewsPagedQueryHandler : IRequestHandler<GetPerformance360ReviewsPagedQuery, PagedResult<Performance360ReviewDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetPerformance360ReviewsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Performance360ReviewDto>> Handle(GetPerformance360ReviewsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Performance360ReviewEntity> query = _context.Performance360ReviewEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CycleId.HasValue && request.CycleId != Guid.Empty)
            {
                query = query.Where(x => x.CycleId == request.CycleId);
            }

            if (request.SubjectEmployeeId.HasValue && request.SubjectEmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.SubjectEmployeeId == request.SubjectEmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(request.ReviewerType))
            {
                query = query.Where(x => x.ReviewerType == request.ReviewerType.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Comment != null && x.Comment.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<Performance360ReviewEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Performance360ReviewDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<Performance360ReviewDto>> MapManyAsync(List<Performance360ReviewEntity> rows, CancellationToken cancellationToken)
        {
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

    public class GetPerformance360ReviewByIdQuery : IdRequest, IRequest<Performance360ReviewDto?> { }

    public class GetPerformance360ReviewByIdQueryHandler : IRequestHandler<GetPerformance360ReviewByIdQuery, Performance360ReviewDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetPerformance360ReviewsPagedQueryHandler _mapper;
        public GetPerformance360ReviewByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetPerformance360ReviewsPagedQueryHandler(context);
        }

        public async Task<Performance360ReviewDto?> Handle(GetPerformance360ReviewByIdQuery request, CancellationToken cancellationToken)
        {
            Performance360ReviewEntity? e = await _context.Performance360ReviewEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreatePerformance360ReviewCommand : Performance360ReviewCommandFields, IRequest<Guid> { }

    public class CreatePerformance360ReviewCommandHandler : IRequestHandler<CreatePerformance360ReviewCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreatePerformance360ReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreatePerformance360ReviewCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, cancellationToken);
            Performance360ReviewEntity entity = new()
            {
                CycleId = request.CycleId!.Value,
                SubjectEmployeeId = request.SubjectEmployeeId!.Value,
                ReviewerEmployeeId = request.ReviewerEmployeeId!.Value,
                ReviewerType = NormalizeReviewerType(request.ReviewerType),
                Score = request.Score ?? 0,
                Comment = request.Comment,
                Status = NormalizeStatus(request.Status),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.Performance360ReviewEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(Performance360ReviewCommandFields request, CancellationToken cancellationToken)
        {
            if (!request.CycleId.HasValue || request.CycleId == Guid.Empty)
            {
                throw new InvalidOperationException("Chu kỳ đánh giá là bắt buộc.");
            }

            if (!await _context.PerformanceReviewCycleEntities.AnyAsync(x => x.Id == request.CycleId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Chu kỳ đánh giá không tồn tại.");
            }

            if (!request.SubjectEmployeeId.HasValue || request.SubjectEmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Nhân viên được đánh giá là bắt buộc.");
            }

            if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.SubjectEmployeeId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Nhân viên được đánh giá không tồn tại.");
            }

            if (!request.ReviewerEmployeeId.HasValue || request.ReviewerEmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Người đánh giá là bắt buộc.");
            }

            if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.ReviewerEmployeeId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Người đánh giá không tồn tại.");
            }

            string reviewerType = NormalizeReviewerType(request.ReviewerType);
            if (reviewerType is not (Performance360ReviewerType.Self or Performance360ReviewerType.Peer or Performance360ReviewerType.Manager))
            {
                throw new InvalidOperationException("Loại người đánh giá không hợp lệ (SELF/PEER/MANAGER).");
            }

            string status = NormalizeStatus(request.Status);
            if (status is not (Performance360Status.Draft or Performance360Status.Submitted))
            {
                throw new InvalidOperationException("Trạng thái không hợp lệ (DRAFT/SUBMITTED).");
            }
        }

        internal static string NormalizeReviewerType(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? Performance360ReviewerType.Self : value.Trim().ToUpperInvariant();
        }

        internal static string NormalizeStatus(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? Performance360Status.Draft : value.Trim().ToUpperInvariant();
        }
    }

    public class UpdatePerformance360ReviewCommand : Performance360ReviewCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdatePerformance360ReviewCommandHandler : IRequestHandler<UpdatePerformance360ReviewCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreatePerformance360ReviewCommandHandler _create;
        public UpdatePerformance360ReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreatePerformance360ReviewCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdatePerformance360ReviewCommand request, CancellationToken cancellationToken)
        {
            Performance360ReviewEntity? entity = await _context.Performance360ReviewEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.CycleId ??= entity.CycleId;
            request.SubjectEmployeeId ??= entity.SubjectEmployeeId;
            request.ReviewerEmployeeId ??= entity.ReviewerEmployeeId;
            request.ReviewerType ??= entity.ReviewerType;
            request.Status ??= entity.Status;
            await _create.ValidateAsync(request, cancellationToken);

            entity.CycleId = request.CycleId!.Value;
            entity.SubjectEmployeeId = request.SubjectEmployeeId!.Value;
            entity.ReviewerEmployeeId = request.ReviewerEmployeeId!.Value;
            entity.ReviewerType = CreatePerformance360ReviewCommandHandler.NormalizeReviewerType(request.ReviewerType);
            entity.Score = request.Score ?? entity.Score;
            entity.Comment = request.Comment;
            entity.Status = CreatePerformance360ReviewCommandHandler.NormalizeStatus(request.Status);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeletePerformance360ReviewCommand : IdRequest, IRequest<bool> { }

    public class DeletePerformance360ReviewCommandHandler : IRequestHandler<DeletePerformance360ReviewCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeletePerformance360ReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeletePerformance360ReviewCommand request, CancellationToken cancellationToken)
        {
            Performance360ReviewEntity? entity = await _context.Performance360ReviewEntities
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
