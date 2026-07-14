using HrmApi.Domain.Common;
using System;

namespace HrmApi.Domain.Entities.Organization
{
    public class PositionEntity : BaseEntity
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? PositionMasterId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
    }
}