namespace HrmApi.Application.DTOs.Organization
{
    public class OrgChartNodeDto
    {
        public Guid Id { get; set; }
        public string NodeType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public int DisplayOrder { get; set; }
        public int? EmployeeCount { get; set; }
        public string? ManagerName { get; set; }
        public List<OrgChartNodeDto> Children { get; set; } = [];
    }

    public static class OrgChartNodeTypes
    {
        public const string Company = "COMPANY";
        public const string Branch = "BRANCH";
        public const string Department = "DEPARTMENT";
        public const string Part = "PART";
    }

    public class GetOrgChartTreeRequest
    {
        public Guid CompanyId { get; set; }
        public bool IncludeParts { get; set; } = true;
    }

    public class ReparentOrgChartNodeRequest
    {
        public string NodeType { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public Guid? NewParentId { get; set; }
        public Guid? NewBranchId { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
