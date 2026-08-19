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
    public class GetRecruitmentRequestsPagedQuery : RecruitmentRequestPagedQuery, IRequest<PagedResult<RecruitmentRequestDto>> { }

    public class GetRecruitmentRequestsPagedQueryHandler : IRequestHandler<GetRecruitmentRequestsPagedQuery, PagedResult<RecruitmentRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetRecruitmentRequestsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<RecruitmentRequestDto>> Handle(GetRecruitmentRequestsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<RecruitmentRequestEntity> query = _context.RecruitmentRequestEntities.AsNoTracking().Where(x => !x.IsDeleted);

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

            if (request.PositionId.HasValue && request.PositionId != Guid.Empty)
            {
                query = query.Where(x => x.PositionId == request.PositionId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.RequestLevel))
            {
                query = query.Where(x => x.RequestLevel == request.RequestLevel.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Title.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            List<RecruitmentRequestEntity> rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<RecruitmentRequestDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<RecruitmentRequestDto>> MapManyAsync(List<RecruitmentRequestEntity> rows, CancellationToken cancellationToken)
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
            List<Guid> jdIds = rows.Where(x => x.JobDescriptionId.HasValue).Select(x => x.JobDescriptionId!.Value).Distinct().ToList();
            List<Guid> empIds = rows.SelectMany(x => new[] { x.RequestedByEmployeeId, x.ApprovedByEmployeeId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();

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
            Dictionary<Guid, string> jds = jdIds.Count == 0 ? []
                : await _context.JobDescriptionEntities.AsNoTracking()
                    .Where(x => jdIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
            Dictionary<Guid, string?> emps = empIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => empIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (string?)(x.FullName ?? (x.LastName + " " + x.FirstName).Trim()), cancellationToken);

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

            return rows.Select(e => RecruitmentMapper.ToDto(
                e,
                companies.GetValueOrDefault(e.CompanyId),
                e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                e.DepartmentId.HasValue ? depts.GetValueOrDefault(e.DepartmentId.Value) : null,
                e.PartId.HasValue ? parts.GetValueOrDefault(e.PartId.Value) : null,
                e.PositionId.HasValue ? positionNames.GetValueOrDefault(e.PositionId.Value) : null,
                e.JobDescriptionId.HasValue ? jds.GetValueOrDefault(e.JobDescriptionId.Value) : null,
                e.RequestedByEmployeeId.HasValue ? emps.GetValueOrDefault(e.RequestedByEmployeeId.Value) : null,
                e.ApprovedByEmployeeId.HasValue ? emps.GetValueOrDefault(e.ApprovedByEmployeeId.Value) : null)).ToList();
        }
    }

    public class GetRecruitmentRequestByIdQuery : IdRequest, IRequest<RecruitmentRequestDto?> { }

    public class GetRecruitmentRequestByIdQueryHandler : IRequestHandler<GetRecruitmentRequestByIdQuery, RecruitmentRequestDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetRecruitmentRequestsPagedQueryHandler _mapper;
        public GetRecruitmentRequestByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetRecruitmentRequestsPagedQueryHandler(context);
        }

        public async Task<RecruitmentRequestDto?> Handle(GetRecruitmentRequestByIdQuery request, CancellationToken cancellationToken)
        {
            RecruitmentRequestEntity? entity = await _context.RecruitmentRequestEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<RecruitmentRequestDto> mapped = await _mapper.MapManyAsync([entity], cancellationToken);
            return mapped.FirstOrDefault();
        }
    }

    public class CreateRecruitmentRequestCommand : RecruitmentRequestCommandFields, IRequest<Guid> { }

    public class CreateRecruitmentRequestCommandHandler : IRequestHandler<CreateRecruitmentRequestCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateRecruitmentRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateRecruitmentRequestCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            RecruitmentRequestEntity entity = new()
            {
                Status = RecruitmentRequestStatus.Draft,
                Quantity = request.Quantity ?? 1,
                RequestLevel = string.IsNullOrWhiteSpace(request.RequestLevel)
                    ? RecruitmentRequestLevel.Department
                    : request.RequestLevel.Trim().ToUpperInvariant(),
                RequestedByEmployeeId = RecruitmentMapper.NullIfEmpty(request.RequestedByEmployeeId) ?? _currentUser.EmployeeId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            RecruitmentMapper.Apply(entity, request);

            _ = _context.RecruitmentRequestEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(RecruitmentRequestCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã yêu cầu tuyển dụng là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new InvalidOperationException("Tiêu đề yêu cầu là bắt buộc.");
            }

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException("Công ty là bắt buộc.");
            }

            if (request.Quantity.HasValue && request.Quantity.Value < 1)
            {
                throw new InvalidOperationException("Số lượng phải >= 1.");
            }

            bool companyOk = await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
            if (!companyOk)
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }

            if (request.JobDescriptionId.HasValue && request.JobDescriptionId != Guid.Empty)
            {
                bool jdOk = await _context.JobDescriptionEntities.AnyAsync(x => x.Id == request.JobDescriptionId && !x.IsDeleted, cancellationToken);
                if (!jdOk)
                {
                    throw new InvalidOperationException("JD không tồn tại.");
                }
            }

            bool exists = await _context.RecruitmentRequestEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == request.Code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã yêu cầu tuyển dụng đã tồn tại.");
            }
        }
    }

    public class UpdateRecruitmentRequestCommand : RecruitmentRequestCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateRecruitmentRequestCommandHandler : IRequestHandler<UpdateRecruitmentRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateRecruitmentRequestCommandHandler _create;
        public UpdateRecruitmentRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateRecruitmentRequestCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateRecruitmentRequestCommand request, CancellationToken cancellationToken)
        {
            RecruitmentRequestEntity? entity = await _context.RecruitmentRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is not (RecruitmentRequestStatus.Draft or RecruitmentRequestStatus.Rejected))
            {
                throw new InvalidOperationException("Chỉ được sửa yêu cầu ở trạng thái DRAFT hoặc REJECTED.");
            }

            request.Code ??= entity.Code;
            request.Title ??= entity.Title;
            request.CompanyId ??= entity.CompanyId;
            request.Quantity ??= entity.Quantity;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            RecruitmentMapper.Apply(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class SubmitRecruitmentRequestCommand : IdRequest, IRequest<bool> { }

    public class SubmitRecruitmentRequestCommandHandler : IRequestHandler<SubmitRecruitmentRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IWorkflowEngine _workflow;
        public SubmitRecruitmentRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IWorkflowEngine workflow)
        {
            _context = context;
            _currentUser = currentUser;
            _workflow = workflow;
        }

        public async Task<bool> Handle(SubmitRecruitmentRequestCommand request, CancellationToken cancellationToken)
        {
            RecruitmentRequestEntity? entity = await _context.RecruitmentRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != RecruitmentRequestStatus.Draft)
            {
                throw new InvalidOperationException("Chỉ gửi duyệt yêu cầu ở trạng thái DRAFT.");
            }

            entity.Status = RecruitmentRequestStatus.Pending;
            entity.RequestedByEmployeeId ??= _currentUser.EmployeeId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);

            _ = await _workflow.StartAsync(
                WorkflowEntityType.RecruitmentRequest, entity.Id, entity.CompanyId, cancellationToken);

            return true;
        }
    }

    public class ApproveRecruitmentRequestCommand : RecruitmentRequestDecisionFields, IRequest<bool> { }

    public class ApproveRecruitmentRequestCommandHandler : IRequestHandler<ApproveRecruitmentRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public ApproveRecruitmentRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(ApproveRecruitmentRequestCommand request, CancellationToken cancellationToken)
        {
            RecruitmentRequestEntity? entity = await _context.RecruitmentRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != RecruitmentRequestStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ duyệt yêu cầu ở trạng thái PENDING.");
            }

            entity.Status = RecruitmentRequestStatus.Approved;
            entity.ApprovedByEmployeeId = _currentUser.EmployeeId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApprovalNote = string.IsNullOrWhiteSpace(request.ApprovalNote) ? null : request.ApprovalNote.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class RejectRecruitmentRequestCommand : RecruitmentRequestDecisionFields, IRequest<bool> { }

    public class RejectRecruitmentRequestCommandHandler : IRequestHandler<RejectRecruitmentRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public RejectRecruitmentRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(RejectRecruitmentRequestCommand request, CancellationToken cancellationToken)
        {
            RecruitmentRequestEntity? entity = await _context.RecruitmentRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status != RecruitmentRequestStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ từ chối yêu cầu ở trạng thái PENDING.");
            }

            entity.Status = RecruitmentRequestStatus.Rejected;
            entity.ApprovedByEmployeeId = _currentUser.EmployeeId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApprovalNote = string.IsNullOrWhiteSpace(request.ApprovalNote) ? null : request.ApprovalNote.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class CloseRecruitmentRequestCommand : IdRequest, IRequest<bool> { }

    public class CloseRecruitmentRequestCommandHandler : IRequestHandler<CloseRecruitmentRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CloseRecruitmentRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(CloseRecruitmentRequestCommand request, CancellationToken cancellationToken)
        {
            RecruitmentRequestEntity? entity = await _context.RecruitmentRequestEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is RecruitmentRequestStatus.Closed or RecruitmentRequestStatus.Draft)
            {
                throw new InvalidOperationException("Không thể đóng yêu cầu ở trạng thái hiện tại.");
            }

            entity.Status = RecruitmentRequestStatus.Closed;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
