using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Discipline
{
    public class ViolationTypeEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Severity { get; set; } = ViolationSeverity.Medium;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public List<ViolationEntity> Violations { get; set; } = [];
    }
}
