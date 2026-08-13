using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Workflow
{
    public class WorkflowInstanceEntity : BaseEntity
    {
        public Guid DefinitionId { get; set; }

        /// <summary>LEAVE | OT | TRANSFER | DISCIPLINE | RECRUITMENT_REQUEST | COMPLAINT</summary>
        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        /// <summary>RUNNING | APPROVED | REJECTED | CANCELLED</summary>
        public string Status { get; set; } = WorkflowInstanceStatus.Running;

        public int CurrentStepOrder { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public virtual WorkflowDefinitionEntity? Definition { get; set; }
        public virtual ICollection<WorkflowTaskEntity> Tasks { get; set; } = new List<WorkflowTaskEntity>();
    }
}
