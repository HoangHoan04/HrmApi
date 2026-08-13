using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Workflow
{
    public class WorkflowStepEntity : BaseEntity
    {
        public Guid DefinitionId { get; set; }
        public int StepOrder { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>MANAGER | HR | ROLE</summary>
        public string ApproverResolver { get; set; } = string.Empty;

        public string? RequiredRoleCode { get; set; }
        public bool IsFinal { get; set; }

        public virtual WorkflowDefinitionEntity? Definition { get; set; }
    }
}
