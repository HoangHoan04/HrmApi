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
    public class GetReviewCyclesPagedQuery : PerformanceReviewCyclePagedQuery, IRequest<PagedResult<PerformanceReviewCycleDto>> { }

    public class GetReviewCyclesPagedQueryHandler : IRequestHandler<GetReviewCyclesPagedQuery, PagedResult<PerformanceReviewCycleDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetReviewCyclesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PerformanceReviewCycleDto>> Handle(GetReviewCyclesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PerformanceReviewCycleEntity> query = _context.PerformanceReviewCycleEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<PerformanceReviewCycleEntity> rows = await query
                .OrderByDescending(x => x.PeriodFrom)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<PerformanceReviewCycleDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<PerformanceReviewCycleDto>> MapManyAsync(List<PerformanceReviewCycleEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();
            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            Dictionary<Guid, string> companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> branches = branchIds.Count == 0 ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e => new PerformanceReviewCycleDto
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                CompanyId = e.CompanyId,
                CompanyName = companies.GetValueOrDefault(e.CompanyId),
                BranchId = e.BranchId,
                BranchName = e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                PeriodFrom = e.PeriodFrom,
                PeriodTo = e.PeriodTo,
                Status = e.Status,
                Note = e.Note,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            }).ToList();
        }
    }

    public class GetReviewCycleByIdQuery : IdRequest, IRequest<PerformanceReviewCycleDto?> { }

    public class GetReviewCycleByIdQueryHandler : IRequestHandler<GetReviewCycleByIdQuery, PerformanceReviewCycleDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetReviewCyclesPagedQueryHandler _mapper;
        public GetReviewCycleByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetReviewCyclesPagedQueryHandler(context);
        }

        public async Task<PerformanceReviewCycleDto?> Handle(GetReviewCycleByIdQuery request, CancellationToken cancellationToken)
        {
            PerformanceReviewCycleEntity? e = await _context.PerformanceReviewCycleEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateReviewCycleCommand : PerformanceReviewCycleCommandFields, IRequest<Guid> { }

    public class CreateReviewCycleCommandHandler : IRequestHandler<CreateReviewCycleCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateReviewCycleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateReviewCycleCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            PerformanceReviewCycleEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                Name = request.Name!.Trim(),
                CompanyId = request.CompanyId!.Value,
                BranchId = request.BranchId,
                PeriodFrom = request.PeriodFrom!.Value,
                PeriodTo = request.PeriodTo!.Value,
                Status = string.IsNullOrWhiteSpace(request.Status) ? ReviewCycleStatus.Draft : request.Status.Trim().ToUpperInvariant(),
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.PerformanceReviewCycleEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(PerformanceReviewCycleCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã chu kỳ đánh giá là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên chu kỳ đánh giá là bắt buộc.");
            }

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException("Công ty là bắt buộc.");
            }

            if (!request.PeriodFrom.HasValue)
            {
                throw new InvalidOperationException("Ngày bắt đầu là bắt buộc.");
            }

            if (!request.PeriodTo.HasValue)
            {
                throw new InvalidOperationException("Ngày kết thúc là bắt buộc.");
            }

            if (request.PeriodTo < request.PeriodFrom)
            {
                throw new InvalidOperationException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
            }

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.PerformanceReviewCycleEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Mã chu kỳ đánh giá đã tồn tại.");
            }

            if (!await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }
        }
    }

    public class UpdateReviewCycleCommand : PerformanceReviewCycleCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateReviewCycleCommandHandler : IRequestHandler<UpdateReviewCycleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateReviewCycleCommandHandler _create;
        public UpdateReviewCycleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateReviewCycleCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateReviewCycleCommand request, CancellationToken cancellationToken)
        {
            PerformanceReviewCycleEntity? entity = await _context.PerformanceReviewCycleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.Code ??= entity.Code;
            request.Name ??= entity.Name;
            request.CompanyId ??= entity.CompanyId;
            request.PeriodFrom ??= entity.PeriodFrom;
            request.PeriodTo ??= entity.PeriodTo;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.Name = request.Name!.Trim();
            entity.CompanyId = request.CompanyId!.Value;
            entity.BranchId = request.BranchId;
            entity.PeriodFrom = request.PeriodFrom!.Value;
            entity.PeriodTo = request.PeriodTo!.Value;
            entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToUpperInvariant();
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteReviewCycleCommand : IdRequest, IRequest<bool> { }

    public class DeleteReviewCycleCommandHandler : IRequestHandler<DeleteReviewCycleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteReviewCycleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteReviewCycleCommand request, CancellationToken cancellationToken)
        {
            PerformanceReviewCycleEntity? entity = await _context.PerformanceReviewCycleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (await _context.KpiGoalEntities.AnyAsync(x => !x.IsDeleted && x.CycleId == request.Id, cancellationToken))
            {
                throw new InvalidOperationException("Chu kỳ đang có mục tiêu KPI — không thể xóa.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
