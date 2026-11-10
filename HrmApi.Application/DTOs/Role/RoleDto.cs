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
        public string DataScope { get; set; } = HrmApi.Domain.Enums.DataScope.Own;
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
        public string DataScope { get; set; } = HrmApi.Domain.Enums.DataScope.Own;
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
        /// <summary>EMPLOYEE = gán theo nhân viên (sửa được ở màn phân quyền theo nhân viên); USER = gán trực tiếp cho tài khoản.</summary>
        public string Source { get; set; } = "EMPLOYEE";
    }

    /// <summary>Tóm tắt quyền truy cập của một nhân viên cho màn phân quyền theo nhân viên.</summary>
    public class EmployeeAccessSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? CompanyEmail { get; set; }
        public string? Status { get; set; }
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }

        /// <summary>Tài khoản đăng nhập; null nếu nhân viên chưa có tài khoản.</summary>
        public EmployeeAccountInfoDto? Account { get; set; }

        public List<UserRoleItemDto> Roles { get; set; } = [];

        /// <summary>Không có vai trò nào -> hệ thống tự áp dụng quyền của vai trò EMPLOYEE.</summary>
        public bool UsesDefaultEmployeeRole { get; set; }
    }

    public class EmployeeAccountInfoDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Type { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
