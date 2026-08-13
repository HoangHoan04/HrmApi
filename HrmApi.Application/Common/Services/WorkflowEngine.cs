using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Workflow;
using HrmApi.Domain.Entities.Workflow;
using HrmApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrmApi.Application.Common.Services
{
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<WorkflowEngine> _logger;

        public WorkflowEngine(IApplicationDbContext context, ILogger<WorkflowEngine> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid?> StartAsync(
            string entityType,
            Guid entityId,
            Guid? companyId = null,
            CancellationToken cancellationToken = default)
        {
            string type = NormalizeEntityType(entityType);
            if (string.IsNullOrEmpty(type) || entityId == Guid.Empty)
                return null;

            bool alreadyRunning = await _context.WorkflowInstanceEntities.AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDeleted
                    && x.EntityType == type
                    && x.EntityId == entityId
                    && x.Status == WorkflowInstanceStatus.Running,
                    cancellationToken);
            if (alreadyRunning)
                return null;

            WorkflowDefinitionEntity? definition = await ResolveActiveDefinitionAsync(type, companyId, cancellationToken);
            if (definition == null)
                return null;

            List<WorkflowStepEntity> steps = await _context.WorkflowStepEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.DefinitionId == definition.Id)
                .OrderBy(x => x.StepOrder)
                .ToListAsync(cancellationToken);
            if (steps.Count == 0)
            {
                _logger.LogWarning("Workflow definition {Code} has no steps; skip start.", definition.Code);
                return null;
            }

            WorkflowStepEntity first = steps[0];
            Guid? subjectEmployeeId = await ResolveSubjectEmployeeIdAsync(type, entityId, cancellationToken);
            Guid? assigneeId = await ResolveAssigneeAsync(first, subjectEmployeeId, cancellationToken);

            DateTime now = DateTime.UtcNow;
            var instance = new WorkflowInstanceEntity
            {
                DefinitionId = definition.Id,
                EntityType = type,
                EntityId = entityId,
                Status = WorkflowInstanceStatus.Running,
                CurrentStepOrder = first.StepOrder,
                StartedAt = now,
                CreatedAt = now,
            };
            _ = _context.WorkflowInstanceEntities.Add(instance);

            _ = _context.WorkflowTaskEntities.Add(new WorkflowTaskEntity
            {
                InstanceId = instance.Id,
                StepOrder = first.StepOrder,
                AssigneeEmployeeId = assigneeId,
                Status = WorkflowTaskStatus.Pending,
                CreatedAt = now,
            });

            _ = await _context.SaveChangesAsync(cancellationToken);
            return instance.Id;
        }

        public async Task<bool> AdvanceAsync(
            Guid instanceId,
            bool approve,
            string? note,
            Guid actorUserId,
            Guid? actorEmployeeId = null,
            CancellationToken cancellationToken = default)
        {
            WorkflowInstanceEntity? instance = await _context.WorkflowInstanceEntities
                .FirstOrDefaultAsync(x => x.Id == instanceId && !x.IsDeleted, cancellationToken);
            if (instance == null) return false;
            if (instance.Status != WorkflowInstanceStatus.Running)
                throw new InvalidOperationException("Workflow instance không còn RUNNING.");

            WorkflowTaskEntity? task = await _context.WorkflowTaskEntities
                .Where(x =>
                    !x.IsDeleted
                    && x.InstanceId == instanceId
                    && x.StepOrder == instance.CurrentStepOrder
                    && x.Status == WorkflowTaskStatus.Pending)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy task đang chờ duyệt.");

            WorkflowStepEntity? step = await _context.WorkflowStepEntities.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted
                    && x.DefinitionId == instance.DefinitionId
                    && x.StepOrder == instance.CurrentStepOrder,
                    cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy bước workflow hiện tại.");

            DateTime now = DateTime.UtcNow;
            task.Status = WorkflowTaskStatus.Done;
            task.Action = approve ? WorkflowTaskAction.Approve : WorkflowTaskAction.Reject;
            task.Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            task.ActedAt = now;
            task.ActedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
            task.UpdatedAt = now;
            task.UpdatedBy = actorUserId == Guid.Empty ? null : actorUserId;

            if (!approve)
            {
                instance.Status = WorkflowInstanceStatus.Rejected;
                instance.CompletedAt = now;
                instance.UpdatedAt = now;
                instance.UpdatedBy = actorUserId == Guid.Empty ? null : actorUserId;
                _ = await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            if (step.IsFinal)
            {
                instance.Status = WorkflowInstanceStatus.Approved;
                instance.CompletedAt = now;
                instance.UpdatedAt = now;
                instance.UpdatedBy = actorUserId == Guid.Empty ? null : actorUserId;
                _ = await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            WorkflowStepEntity? next = await _context.WorkflowStepEntities.AsNoTracking()
                .Where(x =>
                    !x.IsDeleted
                    && x.DefinitionId == instance.DefinitionId
                    && x.StepOrder > instance.CurrentStepOrder)
                .OrderBy(x => x.StepOrder)
                .FirstOrDefaultAsync(cancellationToken);

            if (next == null)
            {
                instance.Status = WorkflowInstanceStatus.Approved;
                instance.CompletedAt = now;
                instance.UpdatedAt = now;
                instance.UpdatedBy = actorUserId == Guid.Empty ? null : actorUserId;
                _ = await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            Guid? subjectEmployeeId = await ResolveSubjectEmployeeIdAsync(
                instance.EntityType, instance.EntityId, cancellationToken);
            Guid? assigneeId = await ResolveAssigneeAsync(next, subjectEmployeeId, cancellationToken);

            instance.CurrentStepOrder = next.StepOrder;
            instance.UpdatedAt = now;
            instance.UpdatedBy = actorUserId == Guid.Empty ? null : actorUserId;

            _ = _context.WorkflowTaskEntities.Add(new WorkflowTaskEntity
            {
                InstanceId = instance.Id,
                StepOrder = next.StepOrder,
                AssigneeEmployeeId = assigneeId,
                Status = WorkflowTaskStatus.Pending,
                CreatedAt = now,
            });

            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AdvanceForEntityAsync(
            string entityType,
            Guid entityId,
            bool approve,
            string? note,
            Guid actorUserId,
            Guid? actorEmployeeId = null,
            CancellationToken cancellationToken = default)
        {
            string type = NormalizeEntityType(entityType);
            WorkflowInstanceEntity? instance = await _context.WorkflowInstanceEntities.AsNoTracking()
                .Where(x =>
                    !x.IsDeleted
                    && x.EntityType == type
                    && x.EntityId == entityId
                    && x.Status == WorkflowInstanceStatus.Running)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (instance == null) return false;

            try
            {
                return await AdvanceAsync(
                    instance.Id, approve, note, actorUserId, actorEmployeeId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "AdvanceForEntityAsync failed for {EntityType}/{EntityId}; hard-wire flow continues.",
                    type, entityId);
                return false;
            }
        }

        public async Task<List<WorkflowInboxItemDto>> GetInboxAsync(
            Guid employeeId,
            CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty) return [];

            List<string> roleCodes = await GetEmployeeRoleCodesAsync(employeeId, cancellationToken);
            bool isHr = roleCodes.Any(c =>
                string.Equals(c, RoleCodes.Hr, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c, "HR", StringComparison.OrdinalIgnoreCase));

            var pending = await (
                from t in _context.WorkflowTaskEntities.AsNoTracking()
                join i in _context.WorkflowInstanceEntities.AsNoTracking() on t.InstanceId equals i.Id
                join s in _context.WorkflowStepEntities.AsNoTracking()
                    on new { i.DefinitionId, t.StepOrder } equals new { DefinitionId = s.DefinitionId, StepOrder = s.StepOrder }
                where !t.IsDeleted && !i.IsDeleted && !s.IsDeleted
                    && t.Status == WorkflowTaskStatus.Pending
                    && i.Status == WorkflowInstanceStatus.Running
                select new { Task = t, Instance = i, Step = s }
            ).ToListAsync(cancellationToken);

            List<WorkflowInboxItemDto> result = [];
            foreach (var row in pending)
            {
                bool match =
                    row.Task.AssigneeEmployeeId == employeeId
                    || (row.Task.AssigneeEmployeeId == null
                        && row.Step.ApproverResolver == WorkflowApproverResolver.Hr
                        && isHr)
                    || (row.Task.AssigneeEmployeeId == null
                        && row.Step.ApproverResolver == WorkflowApproverResolver.Role
                        && !string.IsNullOrWhiteSpace(row.Step.RequiredRoleCode)
                        && roleCodes.Contains(row.Step.RequiredRoleCode.Trim().ToUpperInvariant()));

                if (!match) continue;

                result.Add(new WorkflowInboxItemDto
                {
                    TaskId = row.Task.Id,
                    InstanceId = row.Instance.Id,
                    EntityType = row.Instance.EntityType,
                    EntityId = row.Instance.EntityId,
                    StepOrder = row.Step.StepOrder,
                    StepName = row.Step.Name,
                    ApproverResolver = row.Step.ApproverResolver,
                    InstanceStatus = row.Instance.Status,
                    StartedAt = row.Instance.StartedAt,
                    AssigneeEmployeeId = row.Task.AssigneeEmployeeId,
                });
            }

            return result.OrderByDescending(x => x.StartedAt).ToList();
        }

        private async Task<WorkflowDefinitionEntity?> ResolveActiveDefinitionAsync(
            string entityType,
            Guid? companyId,
            CancellationToken cancellationToken)
        {
            IQueryable<WorkflowDefinitionEntity> q = _context.WorkflowDefinitionEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive && x.EntityType == entityType);

            if (companyId.HasValue && companyId != Guid.Empty)
            {
                WorkflowDefinitionEntity? companyDef = await q
                    .Where(x => x.CompanyId == companyId)
                    .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (companyDef != null) return companyDef;
            }

            return await q
                .Where(x => x.CompanyId == null)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<Guid?> ResolveAssigneeAsync(
            WorkflowStepEntity step,
            Guid? subjectEmployeeId,
            CancellationToken cancellationToken)
        {
            string resolver = (step.ApproverResolver ?? string.Empty).Trim().ToUpperInvariant();
            if (resolver == WorkflowApproverResolver.Manager && subjectEmployeeId.HasValue)
                return await LeaveApproverResolver.ResolveAsync(_context, subjectEmployeeId.Value, cancellationToken);

            return null;
        }

        private async Task<Guid?> ResolveSubjectEmployeeIdAsync(
            string entityType,
            Guid entityId,
            CancellationToken cancellationToken)
        {
            return entityType switch
            {
                WorkflowEntityType.Leave => await _context.RegisterDayOffEntities.AsNoTracking()
                    .Where(x => x.Id == entityId && !x.IsDeleted)
                    .Select(x => (Guid?)x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken),
                WorkflowEntityType.Ot => await _context.OvertimeRequestEntities.AsNoTracking()
                    .Where(x => x.Id == entityId && !x.IsDeleted)
                    .Select(x => (Guid?)x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken),
                WorkflowEntityType.Complaint => await _context.AttendanceComplaintEntities.AsNoTracking()
                    .Where(x => x.Id == entityId && !x.IsDeleted)
                    .Select(x => (Guid?)x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken),
                WorkflowEntityType.Discipline => await _context.ViolationEntities.AsNoTracking()
                    .Where(x => x.Id == entityId && !x.IsDeleted)
                    .Select(x => (Guid?)x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken),
                WorkflowEntityType.Transfer => await _context.TransferEmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == entityId && !x.IsDeleted)
                    .Select(x => (Guid?)x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken),
                WorkflowEntityType.RecruitmentRequest => await _context.RecruitmentRequestEntities.AsNoTracking()
                    .Where(x => x.Id == entityId && !x.IsDeleted)
                    .Select(x => x.RequestedByEmployeeId)
                    .FirstOrDefaultAsync(cancellationToken),
                _ => null,
            };
        }

        private async Task<List<string>> GetEmployeeRoleCodesAsync(
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            Guid? userId = await _context.UserEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (!userId.HasValue) return [];

            return await (
                from ur in _context.UserRoleEntities.AsNoTracking()
                join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                where !ur.IsDeleted && !r.IsDeleted && ur.UserId == userId.Value && r.IsActive
                select r.Code.ToUpper()
            ).Distinct().ToListAsync(cancellationToken);
        }

        internal static string NormalizeEntityType(string? entityType)
        {
            string t = (entityType ?? string.Empty).Trim().ToUpperInvariant();
            return t is WorkflowEntityType.Leave
                or WorkflowEntityType.Ot
                or WorkflowEntityType.Transfer
                or WorkflowEntityType.Discipline
                or WorkflowEntityType.RecruitmentRequest
                or WorkflowEntityType.Complaint
                ? t
                : string.Empty;
        }
    }
}
