using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Recruitment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Recruitment
{
    public class GetJobDescriptionsPagedQuery : JobDescriptionPagedQuery, IRequest<PagedResult<JobDescriptionDto>> { }

    public class GetJobDescriptionsPagedQueryHandler : IRequestHandler<GetJobDescriptionsPagedQuery, PagedResult<JobDescriptionDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetJobDescriptionsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<JobDescriptionDto>> Handle(GetJobDescriptionsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<JobDescriptionEntity> query = _context.JobDescriptionEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);
            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId);
            if (request.PartId.HasValue && request.PartId != Guid.Empty)
                query = query.Where(x => x.PartId == request.PartId);
            if (request.PositionId.HasValue && request.PositionId != Guid.Empty)
                query = query.Where(x => x.PositionId == request.PositionId);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Title.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<JobDescriptionEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<JobDescriptionDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<JobDescriptionDto>> MapManyAsync(List<JobDescriptionEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return [];

            var companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();
            var branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var deptIds = rows.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            var partIds = rows.Where(x => x.PartId.HasValue).Select(x => x.PartId!.Value).Distinct().ToList();
            var positionIds = rows.Where(x => x.PositionId.HasValue).Select(x => x.PositionId!.Value).Distinct().ToList();
            var masterIds = rows.Where(x => x.PositionMasterId.HasValue).Select(x => x.PositionMasterId!.Value).Distinct().ToList();

            var companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var branches = branchIds.Count == 0 ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var depts = deptIds.Count == 0 ? new Dictionary<Guid, string>()
                : await _context.DepartmentEntities.AsNoTracking()
                    .Where(x => deptIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var parts = partIds.Count == 0 ? new Dictionary<Guid, string?>()
                : await _context.PartEntities.AsNoTracking()
                    .Where(x => partIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, Guid?> positionMasterByPos = [];
            if (positionIds.Count > 0)
            {
                positionMasterByPos = await _context.PositionEntities.AsNoTracking()
                    .Where(x => positionIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.PositionMasterId, cancellationToken);
                foreach (Guid mid in positionMasterByPos.Values.Where(x => x.HasValue).Select(x => x!.Value))
                {
                    if (!masterIds.Contains(mid)) masterIds.Add(mid);
                }
            }

            var masters = masterIds.Count == 0 ? new Dictionary<Guid, string>()
                : await _context.PositionMasterEntities.AsNoTracking()
                    .Where(x => masterIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e =>
            {
                string? posName = null;
                if (e.PositionId.HasValue && positionMasterByPos.TryGetValue(e.PositionId.Value, out Guid? pmId) && pmId.HasValue)
                    posName = masters.GetValueOrDefault(pmId.Value);

                return RecruitmentMapper.ToDto(
                    e,
                    companies.GetValueOrDefault(e.CompanyId),
                    e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                    e.DepartmentId.HasValue ? depts.GetValueOrDefault(e.DepartmentId.Value) : null,
                    e.PartId.HasValue ? parts.GetValueOrDefault(e.PartId.Value) : null,
                    posName,
                    e.PositionMasterId.HasValue ? masters.GetValueOrDefault(e.PositionMasterId.Value) : null);
            }).ToList();
        }
    }

    public class GetJobDescriptionByIdQuery : IdRequest, IRequest<JobDescriptionDto?> { }

    public class GetJobDescriptionByIdQueryHandler : IRequestHandler<GetJobDescriptionByIdQuery, JobDescriptionDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetJobDescriptionsPagedQueryHandler _mapper;
        public GetJobDescriptionByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetJobDescriptionsPagedQueryHandler(context);
        }

        public async Task<JobDescriptionDto?> Handle(GetJobDescriptionByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.JobDescriptionEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return null;
            List<JobDescriptionDto> mapped = await _mapper.MapManyAsync([entity], cancellationToken);
            return mapped.FirstOrDefault();
        }
    }

    public class CreateJobDescriptionCommand : JobDescriptionCommandFields, IRequest<Guid> { }

    public class CreateJobDescriptionCommandHandler : IRequestHandler<CreateJobDescriptionCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateJobDescriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateJobDescriptionCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            JobDescriptionEntity entity = new()
            {
                IsDeleted = false,
                IsActive = request.IsActive ?? true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            RecruitmentMapper.Apply(entity, request);

            _ = _context.JobDescriptionEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(JobDescriptionCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("Mã JD là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new InvalidOperationException("Tiêu đề JD là bắt buộc.");
            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
                throw new InvalidOperationException("Công ty là bắt buộc.");

            bool companyOk = await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
            if (!companyOk) throw new InvalidOperationException("Công ty không tồn tại.");

            bool exists = await _context.JobDescriptionEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == request.Code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists) throw new InvalidOperationException("Mã JD đã tồn tại.");
        }
    }

    public class UpdateJobDescriptionCommand : JobDescriptionCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateJobDescriptionCommandHandler : IRequestHandler<UpdateJobDescriptionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateJobDescriptionCommandHandler _create;
        public UpdateJobDescriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateJobDescriptionCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateJobDescriptionCommand request, CancellationToken cancellationToken)
        {
            JobDescriptionEntity? entity = await _context.JobDescriptionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            request.CompanyId ??= entity.CompanyId;
            request.Code ??= entity.Code;
            request.Title ??= entity.Title;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            RecruitmentMapper.Apply(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteJobDescriptionCommand : IdRequest, IRequest<bool> { }

    public class DeleteJobDescriptionCommandHandler : IRequestHandler<DeleteJobDescriptionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteJobDescriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteJobDescriptionCommand request, CancellationToken cancellationToken)
        {
            JobDescriptionEntity? entity = await _context.JobDescriptionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
