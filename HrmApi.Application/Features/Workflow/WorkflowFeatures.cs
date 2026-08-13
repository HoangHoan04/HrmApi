using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Workflow;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Workflow
{
    public class GetWorkflowInboxQuery : IRequest<List<WorkflowInboxItemDto>>
    {
        public Guid? EmployeeId { get; set; }
    }

    public class GetWorkflowInboxQueryHandler : IRequestHandler<GetWorkflowInboxQuery, List<WorkflowInboxItemDto>>
    {
        private readonly IWorkflowEngine _engine;
        private readonly ICurrentUserService _currentUser;
        public GetWorkflowInboxQueryHandler(IWorkflowEngine engine, ICurrentUserService currentUser)
        {
            _engine = engine;
            _currentUser = currentUser;
        }

        public async Task<List<WorkflowInboxItemDto>> Handle(
            GetWorkflowInboxQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = request.EmployeeId is { } eid && eid != Guid.Empty
                ? eid
                : _currentUser.EmployeeId
                  ?? throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
            return await _engine.GetInboxAsync(employeeId, cancellationToken);
        }
    }

    public class GetWorkflowDashboardSummaryQuery : IRequest<WorkflowDashboardSummaryDto> { }

    public class GetWorkflowDashboardSummaryQueryHandler
        : IRequestHandler<GetWorkflowDashboardSummaryQuery, WorkflowDashboardSummaryDto>
    {
        private readonly IApplicationDbContext _context;
        public GetWorkflowDashboardSummaryQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<WorkflowDashboardSummaryDto> Handle(
            GetWorkflowDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var byStatus = await _context.WorkflowInstanceEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.Status)
                .Select(g => new WorkflowStatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var byEntityType = await _context.WorkflowInstanceEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.EntityType)
                .Select(g => new WorkflowEntityTypeCountDto { EntityType = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int pendingTasks = await _context.WorkflowTaskEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.Status == WorkflowTaskStatus.Pending, cancellationToken);

            return new WorkflowDashboardSummaryDto
            {
                ByStatus = byStatus.OrderBy(x => x.Status).ToList(),
                ByEntityType = byEntityType.OrderBy(x => x.EntityType).ToList(),
                PendingTaskCount = pendingTasks,
            };
        }
    }

    public class AdvanceWorkflowTaskCommand : AdvanceWorkflowTaskRequest, IRequest<bool> { }

    public class AdvanceWorkflowTaskCommandHandler : IRequestHandler<AdvanceWorkflowTaskCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IWorkflowEngine _engine;
        private readonly ICurrentUserService _currentUser;

        public AdvanceWorkflowTaskCommandHandler(
            IApplicationDbContext context,
            IWorkflowEngine engine,
            ICurrentUserService currentUser)
        {
            _context = context;
            _engine = engine;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(AdvanceWorkflowTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.WorkflowTaskEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.TaskId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy task.");
            if (task.Status != WorkflowTaskStatus.Pending)
                throw new InvalidOperationException("Task không còn PENDING.");

            return await _engine.AdvanceAsync(
                task.InstanceId,
                approve: true,
                request.Note,
                _currentUser.UserId ?? Guid.Empty,
                _currentUser.EmployeeId,
                cancellationToken);
        }
    }

    public class RejectWorkflowTaskCommand : RejectWorkflowTaskRequest, IRequest<bool> { }

    public class RejectWorkflowTaskCommandHandler : IRequestHandler<RejectWorkflowTaskCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IWorkflowEngine _engine;
        private readonly ICurrentUserService _currentUser;

        public RejectWorkflowTaskCommandHandler(
            IApplicationDbContext context,
            IWorkflowEngine engine,
            ICurrentUserService currentUser)
        {
            _context = context;
            _engine = engine;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(RejectWorkflowTaskCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Note))
                throw new InvalidOperationException("Vui lòng nhập lý do từ chối.");

            var task = await _context.WorkflowTaskEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.TaskId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy task.");
            if (task.Status != WorkflowTaskStatus.Pending)
                throw new InvalidOperationException("Task không còn PENDING.");

            return await _engine.AdvanceAsync(
                task.InstanceId,
                approve: false,
                request.Note,
                _currentUser.UserId ?? Guid.Empty,
                _currentUser.EmployeeId,
                cancellationToken);
        }
    }
}
