using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Workflow
{
    public class WorkflowFormTemplateEntity : BaseEntity
    {
        /// <summary>LEAVE | OT | TRANSFER | DISCIPLINE | RECRUITMENT_REQUEST | COMPLAINT</summary>
        public string EntityType { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string SchemaJson { get; set; } = "{}";
    }
}
