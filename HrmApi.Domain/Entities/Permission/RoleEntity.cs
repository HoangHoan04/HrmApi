using HrmApi.Domain.Common;
using System;
using System.Collections.Generic;

namespace HrmApi.Domain.Entities.Permission
{
    /* Vai trò - nhóm các quyền lại thành 1 bộ để gán cho user, vd: HR Manager, Chief Accountant */
    public class RoleEntity : BaseEntity
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        /* Role mặc định của hệ thống (Admin, Employee...), không cho xóa */
        public bool IsSystem { get; set; } = false;
        /* Role có đang được sử dụng/kích hoạt không */
        public bool IsActive { get; set; } = true;
        public List<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
        public List<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
    }
}