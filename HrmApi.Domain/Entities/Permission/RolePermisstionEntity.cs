using HrmApi.Domain.Common;
using System;

namespace HrmApi.Domain.Entities.Permission
{
    /* Bảng nối Role <-> Permission, quyết định 1 Role được làm gì và ở phạm vi dữ liệu nào */
    public class RolePermissionEntity : BaseEntity
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }

        /* Phạm vi dữ liệu được áp dụng: ALL / BRANCH / DEPARTMENT / OWN
           - ALL: toàn công ty
           - BRANCH: chỉ chi nhánh của user
           - DEPARTMENT: chỉ phòng ban của user
           - OWN: chỉ dữ liệu của chính user đó */
        public string DataScope { get; set; } = "OWN";
        public RoleEntity Role { get; set; } = null!;
        public PermissionEntity Permission { get; set; } = null!;
    }
}