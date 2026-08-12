using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Vai trò - nhóm các quyền lại thành 1 bộ để gán cho user
    /// </summary>
    public class RoleEntity : BaseEntity
    {
        /// <summary>
        /// Id công ty (khóa ngoại tới CompanyEntity)
        /// </summary>
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// Id chi nhánh (khóa ngoại tới BranchEntity)
        /// </summary>
        public Guid? BranchId { get; set; }
        /// <summary>
        /// Mã vai trò (Admin, Employee, Manager, ...) 
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Tên vai trò (Admin, Nhân viên, Quản lý, ...)
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Mô tả vai trò
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Role mặc định của hệ thống (Admin, Employee...), không cho xóa
        /// </summary>
        public bool IsSystem { get; set; } = false;
        /// <summary>
        /// Role có đang được sử dụng/kích hoạt không
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Danh sách quyền được gán cho vai trò này
        /// </summary>
        public List<RolePermissionEntity> RolePermissions { get; set; } = [];
        /// <summary>
        /// Danh sách người dùng được gán vai trò này
        /// </summary>
        public List<UserRoleEntity> UserRoles { get; set; } = [];
        /// <summary>
        /// Navigation property tới công ty sở hữu vai trò này (CompanyEntity)
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh sở hữu vai trò này (BranchEntity)
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }
    }
}
