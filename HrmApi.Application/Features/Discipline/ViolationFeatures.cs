using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Discipline;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Domain.Entities.Discipline;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Discipline
{
    public class GetViolationsPagedQuery : ViolationPagedQuery, IRequest<PagedResult<ViolationDto>> { }

    public class GetViolationsPagedQueryHandler : IRequestHandler<GetViolationsPagedQuery, PagedResult<ViolationDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetViolationsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ViolationDto>> Handle(GetViolationsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ViolationEntity> query = _context.ViolationEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.ViolationTypeId.HasValue && request.ViolationTypeId != Guid.Empty)
            {
                query = query.Where(x => x.ViolationTypeId == request.ViolationTypeId);
            }

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s)
                    || (x.Description != null && x.Description.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);
            List<ViolationEntity> rows = await query
                .OrderByDescending(x => x.OccurredAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ViolationDto>(await MapManyAsync(rows, cancellationToken), total, request.PageIndex, request.PageSize);
        }

        internal async Task<List<ViolationDto>> MapManyAsync(List<ViolationEntity> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            List<Guid> typeIds = rows.Select(x => x.ViolationTypeId).Distinct().ToList();
            List<Guid> empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> companyIds = rows.Select(x => x.CompanyId).Distinct().ToList();
            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            Dictionary<Guid, string> types = await _context.ViolationTypeEntities.AsNoTracking()
                .Where(x => typeIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var emps = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Code, Name = x.FullName ?? (x.LastName + " " + x.FirstName).Trim() }, cancellationToken);
            Dictionary<Guid, string> companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            Dictionary<Guid, string> branches = branchIds.Count == 0 ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(e =>
            {
                _ = emps.TryGetValue(e.EmployeeId, out var emp);
                return new ViolationDto
                {
                    Id = e.Id,
                    Code = e.Code,
                    ViolationTypeId = e.ViolationTypeId,
                    ViolationTypeName = types.GetValueOrDefault(e.ViolationTypeId),
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = emp?.Code,
                    EmployeeName = emp?.Name,
                    CompanyId = e.CompanyId,
                    CompanyName = companies.GetValueOrDefault(e.CompanyId),
                    BranchId = e.BranchId,
                    BranchName = e.BranchId.HasValue ? branches.GetValueOrDefault(e.BranchId.Value) : null,
                    OccurredAt = e.OccurredAt,
                    Description = e.Description,
                    Decision = e.Decision,
                    PenaltyType = e.PenaltyType,
                    Status = e.Status,
                    Note = e.Note,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                };
            }).ToList();
        }
    }

    public class GetViolationByIdQuery : IdRequest, IRequest<ViolationDto?> { }

    public class GetViolationByIdQueryHandler : IRequestHandler<GetViolationByIdQuery, ViolationDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly GetViolationsPagedQueryHandler _mapper;
        public GetViolationByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
            _mapper = new GetViolationsPagedQueryHandler(context);
        }

        public async Task<ViolationDto?> Handle(GetViolationByIdQuery request, CancellationToken cancellationToken)
        {
            ViolationEntity? e = await _context.ViolationEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : (await _mapper.MapManyAsync([e], cancellationToken)).FirstOrDefault();
        }
    }

    public class CreateViolationCommand : ViolationCommandFields, IRequest<Guid> { }

    public class CreateViolationCommandHandler : IRequestHandler<CreateViolationCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateViolationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateViolationCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, null, cancellationToken);
            ViolationEntity entity = new()
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                ViolationTypeId = request.ViolationTypeId!.Value,
                EmployeeId = request.EmployeeId!.Value,
                CompanyId = request.CompanyId!.Value,
                BranchId = request.BranchId,
                OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
                Description = request.Description,
                Decision = request.Decision,
                PenaltyType = string.IsNullOrWhiteSpace(request.PenaltyType) ? PenaltyType.Warning : request.PenaltyType.Trim().ToUpperInvariant(),
                Status = string.IsNullOrWhiteSpace(request.Status) ? ViolationStatus.Draft : request.Status.Trim().ToUpperInvariant(),
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.ViolationEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal async Task ValidateAsync(ViolationCommandFields request, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã biên bản là bắt buộc.");
            }

            if (!request.ViolationTypeId.HasValue || request.ViolationTypeId == Guid.Empty)
            {
                throw new InvalidOperationException("Loại vi phạm (parent) là bắt buộc.");
            }

            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            }

            if (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty)
            {
                throw new InvalidOperationException("Công ty là bắt buộc.");
            }

            string code = request.Code.Trim().ToUpperInvariant();
            if (await _context.ViolationEntities.AnyAsync(x => !x.IsDeleted && x.Code == code && (!excludeId.HasValue || x.Id != excludeId), cancellationToken))
            {
                throw new InvalidOperationException("Mã biên bản đã tồn tại.");
            }

            if (!await _context.ViolationTypeEntities.AnyAsync(x => x.Id == request.ViolationTypeId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Loại vi phạm không tồn tại.");
            }

            if (!await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Nhân viên không tồn tại.");
            }

            if (!await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken))
            {
                throw new InvalidOperationException("Công ty không tồn tại.");
            }
        }
    }

    public class UpdateViolationCommand : ViolationCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateViolationCommandHandler : IRequestHandler<UpdateViolationCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly CreateViolationCommandHandler _create;
        public UpdateViolationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            _create = new CreateViolationCommandHandler(context, currentUser);
        }

        public async Task<bool> Handle(UpdateViolationCommand request, CancellationToken cancellationToken)
        {
            ViolationEntity? entity = await _context.ViolationEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == ViolationStatus.Confirmed)
            {
                throw new InvalidOperationException("Biên bản đã xác nhận — không sửa.");
            }

            request.Code ??= entity.Code;
            request.ViolationTypeId ??= entity.ViolationTypeId;
            request.EmployeeId ??= entity.EmployeeId;
            request.CompanyId ??= entity.CompanyId;
            await _create.ValidateAsync(request, request.Id, cancellationToken);

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.ViolationTypeId = request.ViolationTypeId!.Value;
            entity.EmployeeId = request.EmployeeId!.Value;
            entity.CompanyId = request.CompanyId!.Value;
            entity.BranchId = request.BranchId;
            entity.OccurredAt = request.OccurredAt ?? entity.OccurredAt;
            entity.Description = request.Description;
            entity.Decision = request.Decision;
            entity.PenaltyType = string.IsNullOrWhiteSpace(request.PenaltyType) ? entity.PenaltyType : request.PenaltyType.Trim().ToUpperInvariant();
            entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToUpperInvariant();
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteViolationCommand : IdRequest, IRequest<bool> { }

    public class DeleteViolationCommandHandler : IRequestHandler<DeleteViolationCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteViolationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteViolationCommand request, CancellationToken cancellationToken)
        {
            ViolationEntity? entity = await _context.ViolationEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == ViolationStatus.Confirmed)
            {
                throw new InvalidOperationException("Biên bản đã xác nhận — không sửa.");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class ConfirmViolationCommand : IdRequest, IRequest<bool> { }

    public class ConfirmViolationCommandHandler : IRequestHandler<ConfirmViolationCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IWorkflowEngine _workflow;
        public ConfirmViolationCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IWorkflowEngine workflow)
        {
            _context = context;
            _currentUser = currentUser;
            _workflow = workflow;
        }

        public async Task<bool> Handle(ConfirmViolationCommand request, CancellationToken cancellationToken)
        {
            ViolationEntity? entity = await _context.ViolationEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == ViolationStatus.Cancelled)
            {
                throw new InvalidOperationException("Biên bản đã hủy — không xác nhận.");
            }

            if (entity.Status != ViolationStatus.Draft)
            {
                throw new InvalidOperationException("Chỉ xác nhận biên bản ở trạng thái DRAFT.");
            }

            entity.Status = ViolationStatus.Confirmed;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);

            _ = await _workflow.StartAsync(
                WorkflowEntityType.Discipline, entity.Id, entity.CompanyId, cancellationToken);

            return true;
        }
    }

    public class CancelViolationCommand : IdRequest, IRequest<bool> { }

    public class CancelViolationCommandHandler : IRequestHandler<CancelViolationCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CancelViolationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(CancelViolationCommand request, CancellationToken cancellationToken)
        {
            ViolationEntity? entity = await _context.ViolationEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is not (ViolationStatus.Draft or ViolationStatus.Confirmed))
            {
                throw new InvalidOperationException("Chỉ hủy biên bản ở trạng thái DRAFT hoặc CONFIRMED.");
            }

            entity.Status = ViolationStatus.Cancelled;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
