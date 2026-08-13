using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Workflow
{
    public class WorkflowTaskEntity : BaseEntity
    {
        public Guid InstanceId { get; set; }
        public int StepOrder { get; set; }
        public Guid? AssigneeEmployeeId { get; set; }

        /// <summary>PENDING | DONE | SKIPPED</summary>
        public string Status { get; set; } = WorkflowTaskStatus.Pending;

        /// <summary>APPROVE | REJECT</summary>
        public string? Action { get; set; }

        public string? Note { get; set; }
        public DateTime? ActedAt { get; set; }
        public Guid? ActedByUserId { get; set; }

        public virtual WorkflowInstanceEntity? Instance { get; set; }
    }
}
