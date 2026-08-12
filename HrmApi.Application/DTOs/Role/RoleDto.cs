using System.Collections.Generic;

namespace HrmApi.Application.DTOs.Role
{
    public class RoleListItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public int PermissionCount { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RolePermissionItemDto
    {
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string DataScope { get; set; } = "OWN";
    }

    public class RoleDetailDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public List<RolePermissionItemDto> Permissions { get; set; } = [];
    }

    public class RolePermissionSetItem
    {
        public string PermissionCode { get; set; } = string.Empty;
        public string DataScope { get; set; } = "OWN";
    }

    public class UserListItemDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public List<Guid> RoleIds { get; set; } = [];
        public List<string> RoleCodes { get; set; } = [];
    }

    public class UserDetailDto : UserListItemDto
    {
        public Guid? BranchId { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class RoleSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UserRoleItemDto
    {
        public Guid RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsSystem { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
