using HrmApi.Domain.Common;
using System;
using System.Collections.Generic;

namespace HrmApi.Domain.Entities.Permission
{
    /* Danh mục quyền hạn - định nghĩa từng hành động cụ thể trong hệ thống */
    public class PermissionEntity : BaseEntity
    {
        /* Mã quyền dạng MODULE:ACTION, vd: EMPLOYEE:CREATE, SALARY:APPROVE */
        public string Code { get; set; } = string.Empty;
        /* Tên hiển thị, vd: "Tạo nhân viên" */
        public string Name { get; set; } = string.Empty;
        /* Nhóm module cha để hiển thị theo cây trên UI, vd: EMPLOYEE, SALARY, ASSET */
        public string Module { get; set; } = string.Empty;
        /* Hành động cụ thể: CREATE/READ/UPDATE/DELETE/APPROVE/EXPORT... */
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        /* Có cho phép cấu hình DataScope hay không (1 số quyền chỉ có ON/OFF, không cần scope) */
        public bool IsScopable { get; set; } = true;
        /* Quyền hệ thống, không cho phép xóa qua UI (vd SYSTEM:ADMIN) */
        public bool IsSystem { get; set; } = false;
        public List<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
    }
}