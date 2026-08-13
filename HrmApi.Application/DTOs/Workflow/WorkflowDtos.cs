namespace HrmApi.Application.DTOs.Workflow
{
    public class WorkflowIdRequest
    {
        public Guid Id { get; set; }
    }

    public class WorkflowPagedQuery
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? EntityType { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class WorkflowStepDto
    {
        public Guid Id { get; set; }
        public int StepOrder { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApproverResolver { get; set; } = string.Empty;
        public string? RequiredRoleCode { get; set; }
        public bool IsFinal { get; set; }
    }

    public class WorkflowDefinitionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid? CompanyId { get; set; }
        public List<WorkflowStepDto> Steps { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class WorkflowDefinitionCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? EntityType { get; set; }
        public bool? IsActive { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class WorkflowStepInputDto
    {
        public int StepOrder { get; set; }
        public string? Name { get; set; }
        public string? ApproverResolver { get; set; }
        public string? RequiredRoleCode { get; set; }
        public bool IsFinal { get; set; }
    }

    public class SetWorkflowStepsRequest
    {
        public Guid DefinitionId { get; set; }
        public List<WorkflowStepInputDto> Steps { get; set; } = [];
    }

    public class WorkflowInboxItemDto
    {
        public Guid TaskId { get; set; }
        public Guid InstanceId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string ApproverResolver { get; set; } = string.Empty;
        public string InstanceStatus { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public Guid? AssigneeEmployeeId { get; set; }
    }

    public class WorkflowDashboardSummaryDto
    {
        public List<WorkflowStatusCountDto> ByStatus { get; set; } = [];
        public List<WorkflowEntityTypeCountDto> ByEntityType { get; set; } = [];
        public int PendingTaskCount { get; set; }
    }

    public class WorkflowStatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class WorkflowEntityTypeCountDto
    {
        public string EntityType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AdvanceWorkflowTaskRequest
    {
        public Guid TaskId { get; set; }
        public string? Note { get; set; }
    }

    public class RejectWorkflowTaskRequest
    {
        public Guid TaskId { get; set; }
        public string? Note { get; set; }
    }

    public class WorkflowFormTemplateDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SchemaJson { get; set; } = "{}";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class WorkflowFormTemplateCommandFields
    {
        public string? EntityType { get; set; }
        public string? Name { get; set; }
        public string? SchemaJson { get; set; }
    }
}
