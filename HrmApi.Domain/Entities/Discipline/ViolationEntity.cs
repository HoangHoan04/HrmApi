using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Discipline
{
    public class ViolationEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public Guid ViolationTypeId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Description { get; set; }
        public string? Decision { get; set; }
        public string PenaltyType { get; set; } = Enums.PenaltyType.Warning;
        public string Status { get; set; } = ViolationStatus.Draft;
        public string? Note { get; set; }

        public ViolationTypeEntity? ViolationType { get; set; }
        public EmployeeEntity? Employee { get; set; }
        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
    }
}
