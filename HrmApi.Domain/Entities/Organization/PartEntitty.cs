using HrmApi.Domain.Common;
using System;

namespace HrmApi.Domain.Entities.Organization
{
    public class PartEntity : BaseEntity
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }

        public Guid? PartMasterId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}