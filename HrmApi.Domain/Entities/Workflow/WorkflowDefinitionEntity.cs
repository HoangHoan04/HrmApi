using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Workflow
{
    public class WorkflowDefinitionEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>LEAVE | OT | TRANSFER | DISCIPLINE | RECRUITMENT_REQUEST | COMPLAINT</summary>
        public string EntityType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public Guid? CompanyId { get; set; }

        public virtual CompanyEntity? Company { get; set; }
        public virtual ICollection<WorkflowStepEntity> Steps { get; set; } = new List<WorkflowStepEntity>();
    }
}
