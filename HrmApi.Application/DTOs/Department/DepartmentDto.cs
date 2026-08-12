namespace HrmApi.Application.DTOs.Department
{
    public class DepartmentDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? ParentDepartmentId { get; set; }
        public string? ParentDepartmentName { get; set; }
        public int Level { get; set; }
        public int Limit { get; set; }
        public int? CurrentHeadCount { get; set; }
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public Guid? DeputyManagerId { get; set; }
        public string? DeputyManagerName { get; set; }
        public string? Email { get; set; }
        public string? PhoneExtension { get; set; }
        public string? CostCenterCode { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public DateTime? DissolvedDate { get; set; }
        public bool IsNotifyMarketing { get; set; }
    }

    public class DepartmentSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }
}
