using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.Common.Services;
using HrmApi.Application.DTOs.Workflow;
using HrmApi.Domain.Entities.Workflow;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Workflow
{
    public class GetWorkflowDefinitionsPagedQuery : WorkflowPagedQuery, IRequest<PagedResult<WorkflowDefinitionDto>> { }

    public class GetWorkflowDefinitionsPagedQueryHandler
        : IRequestHandler<GetWorkflowDefinitionsPagedQuery, PagedResult<WorkflowDefinitionDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetWorkflowDefinitionsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<WorkflowDefinitionDto>> Handle(
            GetWorkflowDefinitionsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<WorkflowDefinitionEntity> query = _context.WorkflowDefinitionEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (request.CompanyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (!string.IsNullOrWhiteSpace(request.EntityType))
            {
                string et = WorkflowEngine.NormalizeEntityType(request.EntityType);
                if (!string.IsNullOrEmpty(et))
                {
                    query = query.Where(x => x.EntityType == et);
                }
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<WorkflowDefinitionEntity> rows = await query.OrderBy(x => x.Code)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            List<Guid> ids = rows.Select(x => x.Id).ToList();
            List<WorkflowStepEntity> steps = await _context.WorkflowStepEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && ids.Contains(x.DefinitionId))
                .OrderBy(x => x.StepOrder)
                .ToListAsync(cancellationToken);
            Dictionary<Guid, List<WorkflowStepEntity>> stepMap = steps
                .GroupBy(x => x.DefinitionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            List<WorkflowDefinitionDto> dtos = rows.Select(e => ToDto(e, stepMap.GetValueOrDefault(e.Id))).ToList();
            return new PagedResult<WorkflowDefinitionDto>(dtos, total, pageIndex, pageSize);
        }

        internal static WorkflowDefinitionDto ToDto(WorkflowDefinitionEntity e, List<WorkflowStepEntity>? steps)
        {
            return new()
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                EntityType = e.EntityType,
                IsActive = e.IsActive,
                CompanyId = e.CompanyId,
                Steps = (steps ?? []).Select(s => new WorkflowStepDto
                {
                    Id = s.Id,
                    StepOrder = s.StepOrder,
                    Name = s.Name,
                    ApproverResolver = s.ApproverResolver,
                    RequiredRoleCode = s.RequiredRoleCode,
                    IsFinal = s.IsFinal,
                }).ToList(),
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            };
        }
    }

    public class GetWorkflowDefinitionByIdQuery : WorkflowIdRequest, IRequest<WorkflowDefinitionDto?> { }

    public class GetWorkflowDefinitionByIdQueryHandler
        : IRequestHandler<GetWorkflowDefinitionByIdQuery, WorkflowDefinitionDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetWorkflowDefinitionByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WorkflowDefinitionDto?> Handle(
            GetWorkflowDefinitionByIdQuery request, CancellationToken cancellationToken)
        {
            WorkflowDefinitionEntity? e = await _context.WorkflowDefinitionEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (e == null)
            {
                return null;
            }

            List<WorkflowStepEntity> steps = await _context.WorkflowStepEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.DefinitionId == e.Id)
                .OrderBy(x => x.StepOrder)
                .ToListAsync(cancellationToken);
            return GetWorkflowDefinitionsPagedQueryHandler.ToDto(e, steps);
        }
    }

    public class CreateWorkflowDefinitionCommand : WorkflowDefinitionCommandFields, IRequest<Guid> { }

    public class CreateWorkflowDefinitionCommandHandler : IRequestHandler<CreateWorkflowDefinitionCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateWorkflowDefinitionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            string entityType = NormalizeEntityType(request.EntityType);
            if (await _context.WorkflowDefinitionEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code, cancellationToken))
            {
                throw new InvalidOperationException("Mã workflow đã tồn tại.");
            }

            WorkflowDefinitionEntity entity = new()
            {
                Code = code,
                Name = request.Name!.Trim(),
                EntityType = entityType,
                IsActive = request.IsActive ?? true,
                CompanyId = request.CompanyId == Guid.Empty ? null : request.CompanyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.WorkflowDefinitionEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(WorkflowDefinitionCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã workflow là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên workflow là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.EntityType))
            {
                throw new InvalidOperationException("EntityType là bắt buộc.");
            }
        }

        internal static string NormalizeEntityType(string? value)
        {
            string t = WorkflowEngine.NormalizeEntityType(value);
            return string.IsNullOrEmpty(t)
                ? throw new InvalidOperationException(
                    "EntityType không hợp lệ (LEAVE/OT/TRANSFER/DISCIPLINE/RECRUITMENT_REQUEST/COMPLAINT).")
                : t;
        }
    }

    public class UpdateWorkflowDefinitionCommand : WorkflowDefinitionCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateWorkflowDefinitionCommandHandler : IRequestHandler<UpdateWorkflowDefinitionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateWorkflowDefinitionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
        {
            WorkflowDefinitionEntity? entity = await _context.WorkflowDefinitionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            CreateWorkflowDefinitionCommandHandler.Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            if (await _context.WorkflowDefinitionEntities.AnyAsync(
                    x => !x.IsDeleted && x.Code == code && x.Id != request.Id, cancellationToken))
            {
                throw new InvalidOperationException("Mã workflow đã tồn tại.");
            }

            entity.Code = code;
            entity.Name = request.Name!.Trim();
            entity.EntityType = CreateWorkflowDefinitionCommandHandler.NormalizeEntityType(request.EntityType);
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.CompanyId = request.CompanyId == Guid.Empty ? null : request.CompanyId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteWorkflowDefinitionCommand : WorkflowIdRequest, IRequest<bool> { }

    public class DeleteWorkflowDefinitionCommandHandler : IRequestHandler<DeleteWorkflowDefinitionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteWorkflowDefinitionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteWorkflowDefinitionCommand request, CancellationToken cancellationToken)
        {
            WorkflowDefinitionEntity? entity = await _context.WorkflowDefinitionEntities
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

    public class SetWorkflowStepsCommand : SetWorkflowStepsRequest, IRequest<bool> { }

    public class SetWorkflowStepsCommandHandler : IRequestHandler<SetWorkflowStepsCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public SetWorkflowStepsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(SetWorkflowStepsCommand request, CancellationToken cancellationToken)
        {
            WorkflowDefinitionEntity? def = await _context.WorkflowDefinitionEntities
                .FirstOrDefaultAsync(x => x.Id == request.DefinitionId && !x.IsDeleted, cancellationToken);
            if (def == null)
            {
                return false;
            }

            if (request.Steps == null || request.Steps.Count == 0)
            {
                throw new InvalidOperationException("Cần ít nhất một bước workflow.");
            }

            List<WorkflowStepInputDto> ordered = request.Steps.OrderBy(x => x.StepOrder).ToList();
            if (ordered.Count(x => x.IsFinal) != 1)
            {
                throw new InvalidOperationException("Phải có đúng một bước IsFinal.");
            }

            if (!ordered[^1].IsFinal)
            {
                throw new InvalidOperationException("Bước cuối phải là IsFinal.");
            }

            DateTime now = DateTime.UtcNow;
            List<WorkflowStepEntity> existing = await _context.WorkflowStepEntities
                .Where(x => !x.IsDeleted && x.DefinitionId == def.Id)
                .ToListAsync(cancellationToken);
            foreach (WorkflowStepEntity old in existing)
            {
                old.IsDeleted = true;
                old.UpdatedAt = now;
                old.UpdatedBy = _currentUser.UserId;
            }

            foreach (WorkflowStepInputDto input in ordered)
            {
                string resolver = (input.ApproverResolver ?? string.Empty).Trim().ToUpperInvariant();
                if (resolver is not (
                    WorkflowApproverResolver.Manager
                    or WorkflowApproverResolver.Hr
                    or WorkflowApproverResolver.Role))
                {
                    throw new InvalidOperationException("ApproverResolver không hợp lệ (MANAGER/HR/ROLE).");
                }

                if (resolver == WorkflowApproverResolver.Role && string.IsNullOrWhiteSpace(input.RequiredRoleCode))
                {
                    throw new InvalidOperationException("ROLE resolver yêu cầu RequiredRoleCode.");
                }

                if (string.IsNullOrWhiteSpace(input.Name))
                {
                    throw new InvalidOperationException("Tên bước là bắt buộc.");
                }

                _ = _context.WorkflowStepEntities.Add(new WorkflowStepEntity
                {
                    DefinitionId = def.Id,
                    StepOrder = input.StepOrder,
                    Name = input.Name.Trim(),
                    ApproverResolver = resolver,
                    RequiredRoleCode = string.IsNullOrWhiteSpace(input.RequiredRoleCode)
                        ? null
                        : input.RequiredRoleCode.Trim().ToUpperInvariant(),
                    IsFinal = input.IsFinal,
                    CreatedAt = now,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                });
            }

            def.UpdatedAt = now;
            def.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
