using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Bảng nối Role <-> Permission, quyết định 1 Role được làm gì và ở phạm vi dữ liệu nào
    /// </summary>
    public class RolePermissionEntity : BaseEntity
    {
        /// <summary>
        /// Id Role (khóa ngoại tới RoleEntity)
        /// </summary>
        public Guid RoleId { get; set; }
        /// <summary>
        /// Id Permission (khóa ngoại tới PermissionEntity)
        /// </summary>
        public Guid PermissionId { get; set; }
        /// <summary>
        /// Phạm vi dữ liệu được áp dụng cho Role này khi có quyền Permission này (ALL / BRANCH / DEPARTMENT / OWN)
        /// </summary>
        public string DataScope { get; set; } = "OWN";
        /// <summary>
        /// Navigation property tới Role
        /// </summary>
        public RoleEntity Role { get; set; } = null!;
        /// <summary>
        /// Navigation property tới Permission
        /// </summary>
        public PermissionEntity Permission { get; set; } = null!;
    }
}