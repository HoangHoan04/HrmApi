using HrmApi.Domain.Common;
using System;
using System.Collections.Generic;

namespace HrmApi.Domain.Entities.Organization
{
    public class PartMasterEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Type { get; set; }
        public List<PositionMasterEntity> PositionMasters { get; set; } = new List<PositionMasterEntity>();
        public List<PartEntity> Parts { get; set; } = new List<PartEntity>();
    }
}