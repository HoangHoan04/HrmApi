using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Gán quyền (theo PermissionCode catalog) cho Role + DataScope.
    /// </summary>
    public class RolePermissionEntity : BaseEntity
    {
        public Guid RoleId { get; set; }

        /// <summary>Mã quyền từ PermissionCodes / RbacPermissionCatalog.</summary>
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>Phạm vi dữ liệu — <see cref="DataScope"/>.</summary>
        public string DataScope { get; set; } = HrmApi.Domain.Enums.DataScope.Own;

        public RoleEntity Role { get; set; } = null!;
    }
}
