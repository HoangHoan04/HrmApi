using HrmApi.Domain.Common;
using System;
using System.Collections.Generic;

namespace HrmApi.Domain.Entities.Organization
{
    /* Công ty */
    public class CompanyEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? Hotline { get; set; }
        public string? PrefixMaleCode { get; set; }
        public string? PrefixFemaleCode { get; set; }
        public string? PrefixFullTimeCode { get; set; }
        public string? PrefixPartTimeCode { get; set; }
        public Guid? ParentId { get; set; } = null;
        public DateTime? DayComputeSalary { get; set; }
        public bool? IsComputePrevMonth { get; set; }
        public Guid? TimeKeepingStandardId { get; set; }
        public List<CompanyEntity> ChildCompanies { get; set; } = new List<CompanyEntity>();
        public List<BranchEntity> Branches { get; set; } = new List<BranchEntity>();
    }
}