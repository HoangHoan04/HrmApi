using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.Common.Services;
using HrmApi.Application.DTOs.Workflow;
using HrmApi.Domain.Entities.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Workflow
{
    public class GetWorkflowFormTemplatesPagedQuery : WorkflowPagedQuery, IRequest<PagedResult<WorkflowFormTemplateDto>> { }

    public class GetWorkflowFormTemplatesPagedQueryHandler
        : IRequestHandler<GetWorkflowFormTemplatesPagedQuery, PagedResult<WorkflowFormTemplateDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetWorkflowFormTemplatesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<WorkflowFormTemplateDto>> Handle(
            GetWorkflowFormTemplatesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<WorkflowFormTemplateEntity> query = _context.WorkflowFormTemplateEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.EntityType))
            {
                string et = WorkflowEngine.NormalizeEntityType(request.EntityType);
                if (!string.IsNullOrEmpty(et)) query = query.Where(x => x.EntityType == et);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s) || x.EntityType.ToLower().Contains(s));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<WorkflowFormTemplateEntity> rows = await query.OrderBy(x => x.EntityType).ThenBy(x => x.Name)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<WorkflowFormTemplateDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static WorkflowFormTemplateDto ToDto(WorkflowFormTemplateEntity e) => new()
        {
            Id = e.Id,
            EntityType = e.EntityType,
            Name = e.Name,
            SchemaJson = e.SchemaJson,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };
    }

    public class GetWorkflowFormTemplateByIdQuery : WorkflowIdRequest, IRequest<WorkflowFormTemplateDto?> { }

    public class GetWorkflowFormTemplateByIdQueryHandler
        : IRequestHandler<GetWorkflowFormTemplateByIdQuery, WorkflowFormTemplateDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetWorkflowFormTemplateByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<WorkflowFormTemplateDto?> Handle(
            GetWorkflowFormTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            WorkflowFormTemplateEntity? e = await _context.WorkflowFormTemplateEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetWorkflowFormTemplatesPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateWorkflowFormTemplateCommand : WorkflowFormTemplateCommandFields, IRequest<Guid> { }

    public class CreateWorkflowFormTemplateCommandHandler : IRequestHandler<CreateWorkflowFormTemplateCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateWorkflowFormTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateWorkflowFormTemplateCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            string entityType = CreateWorkflowDefinitionCommandHandler.NormalizeEntityType(request.EntityType);
            var entity = new WorkflowFormTemplateEntity
            {
                EntityType = entityType,
                Name = request.Name!.Trim(),
                SchemaJson = string.IsNullOrWhiteSpace(request.SchemaJson) ? "{}" : request.SchemaJson.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.WorkflowFormTemplateEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(WorkflowFormTemplateCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Tên form template là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new InvalidOperationException("EntityType là bắt buộc.");
        }
    }

    public class UpdateWorkflowFormTemplateCommand : WorkflowFormTemplateCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateWorkflowFormTemplateCommandHandler : IRequestHandler<UpdateWorkflowFormTemplateCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateWorkflowFormTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateWorkflowFormTemplateCommand request, CancellationToken cancellationToken)
        {
            WorkflowFormTemplateEntity? entity = await _context.WorkflowFormTemplateEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            CreateWorkflowFormTemplateCommandHandler.Validate(request);
            entity.EntityType = CreateWorkflowDefinitionCommandHandler.NormalizeEntityType(request.EntityType);
            entity.Name = request.Name!.Trim();
            entity.SchemaJson = string.IsNullOrWhiteSpace(request.SchemaJson) ? entity.SchemaJson : request.SchemaJson.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteWorkflowFormTemplateCommand : WorkflowIdRequest, IRequest<bool> { }

    public class DeleteWorkflowFormTemplateCommandHandler : IRequestHandler<DeleteWorkflowFormTemplateCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteWorkflowFormTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteWorkflowFormTemplateCommand request, CancellationToken cancellationToken)
        {
            WorkflowFormTemplateEntity? entity = await _context.WorkflowFormTemplateEntities
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
