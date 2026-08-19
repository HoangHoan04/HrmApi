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
    public class GetHiringPlansPagedQuery : HiringPlanPagedQuery, IRequest<PagedResult<HiringPlanDto>> { }

    public class GetHiringPlansPagedQueryHandler : IRequestHandler<GetHiringPlansPagedQuery, PagedResult<HiringPlanDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetHiringPlansPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<HiringPlanDto>> Handle(GetHiringPlansPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<HiringPlanEntity> query = _context.HiringPlanEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
            {
                query = query.Where(x => x.DepartmentId == request.DepartmentId);
            }

            if (request.RecruitmentRequestId.HasValue && request.RecruitmentRequestId != Guid.Empty)
            {
                query = query.Where(x => x.RecruitmentRequestId == request.RecruitmentRequestId);
            }

            if (request.JobDescriptionId.HasValue && request.JobDescriptionId != Guid.Empty)
            {
                query = query.Where(x => x.JobDescriptionId == request.JobDescriptionId);
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
            List<HiringPlanEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<HiringPlanDto>(await MapManyAsync(rows, includeCriteria: false, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<HiringPlanDto>> MapManyAsync(List<HiringPlanEntity> rows, bool includeCriteria, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();
            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            List<Guid> deptIds = rows.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            List<Guid> partIds = rows.Where(x => x.PartId.HasValue).Select(x => x.PartId!.Value).Distinct().ToList();
            List<Guid> positionIds = rows.Where(x => x.PositionId.HasValue).Select(x => x.PositionId!.Value).Distinct().ToList();
            List<Guid> jdIds = rows.Select(x => x.JobDescriptionId).Distinct().ToList();
            List<Guid> reqIds = rows.Where(x => x.RecruitmentRequestId.HasValue).Select(x => x.RecruitmentRequestId!.Value).Distinct().ToList();
            List<Guid> planIds = rows.Select(x => x.Id).ToList();

            Dictionary<Guid, string> companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> branches = branchIds.Count == 0 ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> depts = deptIds.Count == 0 ? []
                : await _context.DepartmentEntities.AsNoTracking()
                    .Where(x => deptIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string?> parts = partIds.Count == 0 ? []
                : await _context.PartEntities.AsNoTracking()
                    .Where(x => partIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> jds = await _context.JobDescriptionEntities.AsNoTracking()
                .Where(x => jdIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
            Dictionary<Guid, string> reqs = reqIds.Count == 0 ? []
                : await _context.RecruitmentRequestEntities.AsNoTracking()
                    .Where(x => reqIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            Dictionary<Guid, string> positionNames = [];
            if (positionIds.Count > 0)
            {
                var pos = await _context.PositionEntities.AsNoTracking()
                    .Where(x => positionIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.PositionMasterId })
                    .ToListAsync(cancellationToken);
                List<Guid> masterIds = pos.Where(x => x.PositionMasterId.HasValue).Select(x => x.PositionMasterId!.Value).Distinct().ToList();
                Dictionary<Guid, string> masters = masterIds.Count == 0 ? []
                    : await _context.PositionMasterEntities.AsNoTracking()
                        .Where(x => masterIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
                foreach (var p in pos)
                {
                    positionNames[p.Id] = p.PositionMasterId.HasValue && masters.TryGetValue(p.PositionMasterId.Value, out var n)
                        ? n : p.Id.ToString("N")[..8];
                }
            }

            Dictionary<Guid, List<PlanCriteriaDto>> criteriaMap = [];
            if (includeCriteria)
            {
                List<HiringPlanCriteriaEntity> criteriaRows = await _context.HiringPlanCriteriaEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && planIds.Contains(x.HiringPlanId))
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync(cancellationToken);
                List<Guid> criteriaIds = criteriaRows.Select(x => x.EvaluationCriteriaId).Distinct().ToList();
                Dictionary<Guid, (string Code, string Name)> criteriaMeta = criteriaIds.Count == 0 ? []
                    : await _context.EvaluationCriteriaEntities.AsNoTracking()
                        .Where(x => criteriaIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => (x.Code, x.Name), cancellationToken);

                criteriaMap = criteriaRows
                    .GroupBy(x => x.HiringPlanId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(c =>
                        {
                            _ = criteriaMeta.TryGetValue(c.EvaluationCriteriaId, out (string Code, string Name) meta);
                            return RecruitmentMapper.ToDto(c, meta.Code, meta.Name);
                        }).ToList());
            }

            return rows.Select(e => RecruitmentMapper.ToDto(
                e,
                e.RecruitmentRequestId.HasValue ? reqs.GetValueOrDefault(e.RecruitmentRequestId.Value) : null,
                jds.GetValueOrDefault(e.JobDescriptionId),
                companies.GetValueOrDefault(e.CompanyId),
                e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                e.DepartmentId.HasValue ? depts.GetValueOrDefault(e.DepartmentId.Value) : null,
                e.PartId.HasValue ? parts.GetValueOrDefault(e.PartId.Value) : null,
                e.PositionId.HasValue ? positionNames.GetValueOrDefault(e.PositionId.Value) : null,
                includeCriteria ? criteriaMap.GetValueOrDefault(e.Id) : null)).ToList();
        }
    }

    public class GetHiringPlanByIdQuery : IdRequest, IRequest<HiringPlanDto?> { }

    public class GetHiringPlanByIdQueryHandler : IRequestHandler<GetHiringPlanByIdQuery, HiringPlanDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetHiringPlansPagedQueryHandler _mapper;
        public GetHiringPlanByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetHiringPlansPagedQueryHandler(context);
        }

        public async Task<HiringPlanDto?> Handle(GetHiringPlanByIdQuery request, CancellationToken cancellationToken)
        {
            HiringPlanEntity? entity = await _context.HiringPlanEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<HiringPlanDto> mapped = await _mapper.MapManyAsync([entity], includeCriteria: true, cancellationToken);
            return mapped.FirstOrDefault();
        }
    }

    public class CreateHiringPlanCommand : HiringPlanCommandFields, IRequest<Guid> { }

    public class CreateHiringPlanCommandHandler : IRequestHandler<CreateHiringPlanCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateHiringPlanCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateHiringPlanCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            HiringPlanEntity entity = new()
            {
                Status = string.IsNullOrWhiteSpace(request.Status) ? HiringPlanStatus.Draft : request.Status.Trim().ToUpperInvariant(),
                TargetQuantity = request.TargetQuantity ?? 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            RecruitmentMapper.Apply(entity, request);

            _ = _context.HiringPlanEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(HiringPlanCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã kế hoạch tuyển dụng là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên kế hoạch tuyển dụng là bắt buộc.");
            }

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException("Công ty là bắt buộc.");
            }

            if (!request.JobDescriptionId.HasValue || request.JobDescriptionId == Guid.Empty)
            {
                throw new InvalidOperationException("JD là bắt buộc.");
            }

            if (request.TargetQuantity.HasValue && request.TargetQuantity.Value < 1)
            {
                throw new InvalidOperationException("Số lượng mục tiêu phải >= 1.");
            }

            if (request.OpenFrom.HasValue && request.OpenTo.HasValue && request.OpenTo < request.OpenFrom)
            {
                throw new InvalidOperationException("Ngày kết thúc phải >= ngày bắt đầu.");
            }

            bool companyOk = await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
            if (!companyOk)
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }

            bool jdOk = await _context.JobDescriptionEntities.AnyAsync(x => x.Id == request.JobDescriptionId && !x.IsDeleted, cancellationToken);
            if (!jdOk)
            {
                throw new InvalidOperationException("JD không tồn tại.");
            }

            if (request.RecruitmentRequestId.HasValue && request.RecruitmentRequestId != Guid.Empty)
            {
                bool reqOk = await _context.RecruitmentRequestEntities.AnyAsync(x => x.Id == request.RecruitmentRequestId && !x.IsDeleted, cancellationToken);
                if (!reqOk)
                {
                    throw new InvalidOperationException("Yêu cầu tuyển dụng không tồn tại.");
                }
            }

            bool exists = await _context.HiringPlanEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == request.Code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã kế hoạch tuyển dụng đã tồn tại.");
            }
        }
    }

    public class UpdateHiringPlanCommand : HiringPlanCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateHiringPlanCommandHandler : IRequestHandler<UpdateHiringPlanCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateHiringPlanCommandHandler _create;
        public UpdateHiringPlanCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateHiringPlanCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateHiringPlanCommand request, CancellationToken cancellationToken)
        {
            HiringPlanEntity? entity = await _context.HiringPlanEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.Code ??= entity.Code;
            request.Name ??= entity.Name;
            request.CompanyId ??= entity.CompanyId;
            request.JobDescriptionId ??= entity.JobDescriptionId;
            request.TargetQuantity ??= entity.TargetQuantity;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            RecruitmentMapper.Apply(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class SetHiringPlanCriteriaCommand : SetHiringPlanCriteriaFields, IRequest<bool> { }

    public class SetHiringPlanCriteriaCommandHandler : IRequestHandler<SetHiringPlanCriteriaCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public SetHiringPlanCriteriaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(SetHiringPlanCriteriaCommand request, CancellationToken cancellationToken)
        {
            HiringPlanEntity? plan = await _context.HiringPlanEntities
                .FirstOrDefaultAsync(x => x.Id == request.HiringPlanId && !x.IsDeleted, cancellationToken);
            if (plan == null)
            {
                return false;
            }

            request.Criteria ??= [];
            List<Guid> criteriaIds = request.Criteria.Select(x => x.EvaluationCriteriaId).Distinct().ToList();
            if (criteriaIds.Count != request.Criteria.Count)
            {
                throw new InvalidOperationException("Không được trùng tiêu chí trong kế hoạch.");
            }

            if (criteriaIds.Count > 0)
            {
                int found = await _context.EvaluationCriteriaEntities.CountAsync(x =>
                    !x.IsDeleted && criteriaIds.Contains(x.Id), cancellationToken);
                if (found != criteriaIds.Count)
                {
                    throw new InvalidOperationException("Một hoặc nhiều tiêu chí không tồn tại.");
                }
            }

            List<HiringPlanCriteriaEntity> existing = await _context.HiringPlanCriteriaEntities
                .Where(x => x.HiringPlanId == request.HiringPlanId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            DateTime now = DateTime.UtcNow;
            foreach (HiringPlanCriteriaEntity row in existing)
            {
                row.IsDeleted = true;
                row.UpdatedAt = now;
                row.UpdatedBy = _currentUser.UserId;
            }

            int order = 0;
            foreach (PlanCriteriaInputDto item in request.Criteria.OrderBy(x => x.DisplayOrder ?? int.MaxValue))
            {
                _ = _context.HiringPlanCriteriaEntities.Add(new HiringPlanCriteriaEntity
                {
                    HiringPlanId = request.HiringPlanId,
                    EvaluationCriteriaId = item.EvaluationCriteriaId,
                    Weight = item.Weight ?? 1,
                    MaxScore = item.MaxScore ?? 10,
                    DisplayOrder = item.DisplayOrder ?? order,
                    IsDeleted = false,
                    CreatedAt = now,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                });
                order++;
            }

            plan.UpdatedAt = now;
            plan.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
