using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Recruitment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Recruitment
{
    public class GetEvaluationCriteriasPagedQuery : EvaluationCriteriaPagedQuery, IRequest<PagedResult<EvaluationCriteriaDto>> { }

    public class GetEvaluationCriteriasPagedQueryHandler : IRequestHandler<GetEvaluationCriteriasPagedQuery, PagedResult<EvaluationCriteriaDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetEvaluationCriteriasPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<EvaluationCriteriaDto>> Handle(GetEvaluationCriteriasPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<EvaluationCriteriaEntity> query = _context.EvaluationCriteriaEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(x => x.Category == request.Category);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<EvaluationCriteriaEntity> rows = await query
                .OrderBy(x => x.Code)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<Guid> companyIds = rows.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            Dictionary<Guid, string> companies = companyIds.Count == 0
                ? []
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<EvaluationCriteriaDto> items = rows.Select(e =>
                RecruitmentMapper.ToDto(e, e.CompanyId.HasValue ? companies.GetValueOrDefault(e.CompanyId.Value) : null)).ToList();
            return new PagedResult<EvaluationCriteriaDto>(items, total, request.PageIndex, request.PageSize);
        }
    }

    public class GetEvaluationCriteriaByIdQuery : IdRequest, IRequest<EvaluationCriteriaDto?> { }

    public class GetEvaluationCriteriaByIdQueryHandler : IRequestHandler<GetEvaluationCriteriaByIdQuery, EvaluationCriteriaDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetEvaluationCriteriaByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EvaluationCriteriaDto?> Handle(GetEvaluationCriteriaByIdQuery request, CancellationToken cancellationToken)
        {
            EvaluationCriteriaEntity? entity = await _context.EvaluationCriteriaEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            return RecruitmentMapper.ToDto(entity, companyName);
        }
    }

    public class CreateEvaluationCriteriaCommand : EvaluationCriteriaCommandFields, IRequest<Guid> { }

    public class CreateEvaluationCriteriaCommandHandler : IRequestHandler<CreateEvaluationCriteriaCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateEvaluationCriteriaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateEvaluationCriteriaCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            EvaluationCriteriaEntity entity = new()
            {
                IsDeleted = false,
                IsActive = request.IsActive ?? true,
                DefaultWeight = request.DefaultWeight ?? 1,
                MaxScore = request.MaxScore ?? 10,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            RecruitmentMapper.Apply(entity, request);

            _ = _context.EvaluationCriteriaEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(EvaluationCriteriaCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã tiêu chí là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên tiêu chí là bắt buộc.");
            }

            if (request.DefaultWeight.HasValue && request.DefaultWeight.Value < 0)
            {
                throw new InvalidOperationException("Trọng số phải >= 0.");
            }

            if (request.MaxScore.HasValue && request.MaxScore.Value <= 0)
            {
                throw new InvalidOperationException("Điểm tối đa phải > 0.");
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                bool companyOk = await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
                if (!companyOk)
                {
                    throw new InvalidOperationException("Công ty không tồn tại.");
                }
            }

            bool exists = await _context.EvaluationCriteriaEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == request.Code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã tiêu chí đã tồn tại.");
            }
        }
    }

    public class UpdateEvaluationCriteriaCommand : EvaluationCriteriaCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateEvaluationCriteriaCommandHandler : IRequestHandler<UpdateEvaluationCriteriaCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateEvaluationCriteriaCommandHandler _create;
        public UpdateEvaluationCriteriaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateEvaluationCriteriaCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateEvaluationCriteriaCommand request, CancellationToken cancellationToken)
        {
            EvaluationCriteriaEntity? entity = await _context.EvaluationCriteriaEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.Code ??= entity.Code;
            request.Name ??= entity.Name;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            RecruitmentMapper.Apply(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteEvaluationCriteriaCommand : IdRequest, IRequest<bool> { }

    public class DeleteEvaluationCriteriaCommandHandler : IRequestHandler<DeleteEvaluationCriteriaCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteEvaluationCriteriaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteEvaluationCriteriaCommand request, CancellationToken cancellationToken)
        {
            EvaluationCriteriaEntity? entity = await _context.EvaluationCriteriaEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
