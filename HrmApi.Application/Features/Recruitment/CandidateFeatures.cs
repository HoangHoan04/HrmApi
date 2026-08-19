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
    public class GetCandidatesPagedQuery : CandidatePagedQuery, IRequest<PagedResult<CandidateDto>> { }

    public class GetCandidatesPagedQueryHandler : IRequestHandler<GetCandidatesPagedQuery, PagedResult<CandidateDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetCandidatesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CandidateDto>> Handle(GetCandidatesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CandidateEntity> query = _context.CandidateEntities.AsNoTracking().Where(x => !x.IsDeleted);

            if (request.HiringPlanId.HasValue && request.HiringPlanId != Guid.Empty)
            {
                query = query.Where(x => x.HiringPlanId == request.HiringPlanId);
            }

            if (request.RecruitmentRequestId.HasValue && request.RecruitmentRequestId != Guid.Empty)
            {
                query = query.Where(x => x.RecruitmentRequestId == request.RecruitmentRequestId);
            }

            if (request.HiringSourceId.HasValue && request.HiringSourceId != Guid.Empty)
            {
                query = query.Where(x => x.HiringSourceId == request.HiringSourceId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }
            else if (request.Statuses is { Count: > 0 })
            {
                List<string> statuses = request.Statuses
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();
                if (statuses.Count > 0)
                {
                    query = query.Where(x => statuses.Contains(x.Status));
                }
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(s)
                    || x.FullName.ToLower().Contains(s)
                    || (x.Email != null && x.Email.ToLower().Contains(s))
                    || (x.Phone != null && x.Phone.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<CandidateEntity> rows = await query
                .OrderByDescending(x => x.AppliedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<CandidateDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<CandidateDto>> MapManyAsync(List<CandidateEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> planIds = rows.Where(x => x.HiringPlanId.HasValue).Select(x => x.HiringPlanId!.Value).Distinct().ToList();
            List<Guid> reqIds = rows.Where(x => x.RecruitmentRequestId.HasValue).Select(x => x.RecruitmentRequestId!.Value).Distinct().ToList();
            List<Guid> sourceIds = rows.Where(x => x.HiringSourceId.HasValue).Select(x => x.HiringSourceId!.Value).Distinct().ToList();
            List<Guid> empIds = rows.Where(x => x.EmployeeId.HasValue).Select(x => x.EmployeeId!.Value).Distinct().ToList();

            Dictionary<Guid, string> plans = planIds.Count == 0 ? []
                : await _context.HiringPlanEntities.AsNoTracking()
                    .Where(x => planIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> reqs = reqIds.Count == 0 ? []
                : await _context.RecruitmentRequestEntities.AsNoTracking()
                    .Where(x => reqIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            Dictionary<Guid, string> sources = sourceIds.Count == 0 ? []
                : await _context.HiringSourceEntities.AsNoTracking()
                    .Where(x => sourceIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string?> emps = empIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => empIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (string?)(x.FullName ?? (x.LastName + " " + x.FirstName).Trim()), cancellationToken);

            return rows.Select(e => RecruitmentMapper.ToDto(
                e,
                e.HiringPlanId.HasValue ? plans.GetValueOrDefault(e.HiringPlanId.Value) : null,
                e.RecruitmentRequestId.HasValue ? reqs.GetValueOrDefault(e.RecruitmentRequestId.Value) : null,
                e.HiringSourceId.HasValue ? sources.GetValueOrDefault(e.HiringSourceId.Value) : null,
                e.EmployeeId.HasValue ? emps.GetValueOrDefault(e.EmployeeId.Value) : null)).ToList();
        }
    }

    public class GetCandidateByIdQuery : IdRequest, IRequest<CandidateDto?> { }

    public class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, CandidateDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetCandidatesPagedQueryHandler _mapper;
        public GetCandidateByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetCandidatesPagedQueryHandler(context);
        }

        public async Task<CandidateDto?> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
        {
            CandidateEntity? entity = await _context.CandidateEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            List<CandidateDto> mapped = await _mapper.MapManyAsync([entity], cancellationToken);
            return mapped.FirstOrDefault();
        }
    }

    public class CreateCandidateCommand : CandidateCommandFields, IRequest<Guid> { }

    public class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateCandidateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateCandidateCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);

            CandidateEntity entity = new()
            {
                Status = string.IsNullOrWhiteSpace(request.Status) ? CandidateStatus.New : request.Status.Trim().ToUpperInvariant(),
                AppliedAt = request.AppliedAt ?? DateTime.UtcNow,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            RecruitmentMapper.Apply(entity, request);

            _ = _context.CandidateEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(CandidateCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã ứng viên là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new InvalidOperationException("Họ tên ứng viên là bắt buộc.");
            }

            if (!string.IsNullOrWhiteSpace(request.Status)
                && !RecruitmentMapper.IsValidCandidateStatus(request.Status.Trim().ToUpperInvariant()))
            {
                throw new InvalidOperationException("Trạng thái ứng viên không hợp lệ.");
            }

            if (request.HiringPlanId.HasValue && request.HiringPlanId != Guid.Empty)
            {
                bool ok = await _context.HiringPlanEntities.AnyAsync(x => x.Id == request.HiringPlanId && !x.IsDeleted, cancellationToken);
                if (!ok)
                {
                    throw new InvalidOperationException("Kế hoạch tuyển dụng không tồn tại.");
                }
            }
            if (request.RecruitmentRequestId.HasValue && request.RecruitmentRequestId != Guid.Empty)
            {
                bool ok = await _context.RecruitmentRequestEntities.AnyAsync(x => x.Id == request.RecruitmentRequestId && !x.IsDeleted, cancellationToken);
                if (!ok)
                {
                    throw new InvalidOperationException("Yêu cầu tuyển dụng không tồn tại.");
                }
            }
            if (request.HiringSourceId.HasValue && request.HiringSourceId != Guid.Empty)
            {
                bool ok = await _context.HiringSourceEntities.AnyAsync(x => x.Id == request.HiringSourceId && !x.IsDeleted, cancellationToken);
                if (!ok)
                {
                    throw new InvalidOperationException("Nguồn tuyển không tồn tại.");
                }
            }

            bool exists = await _context.CandidateEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.Code.ToLower() == request.Code.Trim().ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Mã ứng viên đã tồn tại.");
            }
        }
    }

    public class UpdateCandidateCommand : CandidateCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateCandidateCommandHandler : IRequestHandler<UpdateCandidateCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateCandidateCommandHandler _create;
        public UpdateCandidateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateCandidateCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateCandidateCommand request, CancellationToken cancellationToken)
        {
            CandidateEntity? entity = await _context.CandidateEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            request.Code ??= entity.Code;
            request.FullName ??= entity.FullName;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            RecruitmentMapper.Apply(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class ChangeCandidateStatusCommand : ChangeCandidateStatusFields, IRequest<bool> { }

    public class ChangeCandidateStatusCommandHandler : IRequestHandler<ChangeCandidateStatusCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public ChangeCandidateStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(ChangeCandidateStatusCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                throw new InvalidOperationException("Trạng thái là bắt buộc.");
            }

            string status = request.Status.Trim().ToUpperInvariant();
            if (!RecruitmentMapper.IsValidCandidateStatus(status))
            {
                throw new InvalidOperationException("Trạng thái ứng viên không hợp lệ.");
            }

            CandidateEntity? entity = await _context.CandidateEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class GetCandidateHirePrefillQuery : IdRequest, IRequest<CandidateHirePrefillDto?> { }

    public class GetCandidateHirePrefillQueryHandler : IRequestHandler<GetCandidateHirePrefillQuery, CandidateHirePrefillDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetCandidateHirePrefillQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateHirePrefillDto?> Handle(GetCandidateHirePrefillQuery request, CancellationToken cancellationToken)
        {
            CandidateEntity? candidate = await _context.CandidateEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (candidate == null)
            {
                return null;
            }

            HiringPlanEntity? plan = null;
            if (candidate.HiringPlanId.HasValue)
            {
                plan = await _context.HiringPlanEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == candidate.HiringPlanId.Value && !x.IsDeleted, cancellationToken);
            }

            (string first, string last) = SplitName(candidate.FullName);
            string suggestedCode = !string.IsNullOrWhiteSpace(candidate.Code)
                ? $"NV-{candidate.Code.Trim()}"
                : $"NV-{DateTime.UtcNow:yyMMddHHmm}";

            return new CandidateHirePrefillDto
            {
                CandidateId = candidate.Id,
                CandidateCode = candidate.Code,
                FullName = candidate.FullName,
                FirstName = first,
                LastName = last,
                Email = candidate.Email,
                Phone = candidate.Phone,
                Gender = candidate.Gender,
                DateOfBirth = candidate.DateOfBirth,
                CvUrl = candidate.CvUrl,
                EmployeeId = candidate.EmployeeId,
                Status = candidate.Status,
                HiringPlanId = candidate.HiringPlanId,
                HiringPlanName = plan?.Name,
                CompanyId = plan?.CompanyId,
                BranchId = plan?.BranchId,
                DepartmentId = plan?.DepartmentId,
                PartId = plan?.PartId,
                PositionId = plan?.PositionId,
                SuggestedEmployeeCode = suggestedCode,
            };
        }

        private static (string First, string Last) SplitName(string fullName)
        {
            string name = (fullName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return ("", "");
            }

            string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return (parts[0], parts[0]);
            }

            return (string.Join(' ', parts.Take(parts.Length - 1)), parts[^1]);
        }
    }

    public class LinkCandidateEmployeeCommand : LinkCandidateEmployeeFields, IRequest<bool> { }

    public class LinkCandidateEmployeeCommandHandler : IRequestHandler<LinkCandidateEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public LinkCandidateEmployeeCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(LinkCandidateEmployeeCommand request, CancellationToken cancellationToken)
        {
            if (request.CandidateId == Guid.Empty || request.EmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("CandidateId và EmployeeId là bắt buộc.");
            }

            CandidateEntity? candidate = await _context.CandidateEntities
                .FirstOrDefaultAsync(x => x.Id == request.CandidateId && !x.IsDeleted, cancellationToken);
            if (candidate == null)
            {
                return false;
            }

            bool empOk = await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (!empOk)
            {
                throw new InvalidOperationException("Nhân viên không tồn tại.");
            }

            bool linkedOther = await _context.CandidateEntities.AnyAsync(x =>
                !x.IsDeleted && x.EmployeeId == request.EmployeeId && x.Id != request.CandidateId, cancellationToken);
            if (linkedOther)
            {
                throw new InvalidOperationException("Nhân viên này đã gắn với ứng viên khác.");
            }

            var old = new { candidate.Status, candidate.EmployeeId };
            candidate.EmployeeId = request.EmployeeId;
            if (request.SetStatusHired)
            {
                candidate.Status = CandidateStatus.Hired;
            }

            candidate.UpdatedAt = DateTime.UtcNow;
            candidate.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "CandidateEntity",
                candidate.Id,
                old,
                new { candidate.Status, candidate.EmployeeId },
                "Gắn ứng viên với nhân viên sau khi Hired");

            return true;
        }
    }

    public class GetCandidateStatusSummaryQuery : CandidateStatusSummaryQuery, IRequest<List<CandidateStatusSummaryDto>> { }

    public class GetCandidateStatusSummaryQueryHandler : IRequestHandler<GetCandidateStatusSummaryQuery, List<CandidateStatusSummaryDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetCandidateStatusSummaryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CandidateStatusSummaryDto>> Handle(GetCandidateStatusSummaryQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CandidateEntity> query = _context.CandidateEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.HiringPlanId.HasValue && request.HiringPlanId != Guid.Empty)
            {
                query = query.Where(x => x.HiringPlanId == request.HiringPlanId);
            }

            if (request.RecruitmentRequestId.HasValue && request.RecruitmentRequestId != Guid.Empty)
            {
                query = query.Where(x => x.RecruitmentRequestId == request.RecruitmentRequestId);
            }

            List<CandidateStatusSummaryDto> groups = await query
                .GroupBy(x => x.Status)
                .Select(g => new CandidateStatusSummaryDto { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return groups.OrderBy(x => x.Status).ToList();
        }
    }
}
