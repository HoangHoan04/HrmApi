using HrmApi.Domain.Common;
using System;
using System.Collections.Generic;

namespace HrmApi.Domain.Entities.Organization
{
    /* Chi nhánh của công ty */
    public class BranchEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string GroupSalary { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; } = null;
        public bool IsUsingHrm { get; set; }
        public List<PartMasterEntity> PartMasters { get; set; } = new List<PartMasterEntity>();
        public List<DepartmentEntity> Departments { get; set; } = new List<DepartmentEntity>();
    }
}