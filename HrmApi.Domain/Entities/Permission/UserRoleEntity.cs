using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Bảng ánh xạ giữa người dùng/nhân viên và vai trò (role) trong hệ thống HRM.
    /// </summary>
    public class UserRoleEntity : BaseEntity
    {
        /// <summary>
        /// ID người dùng từ hệ thống Auth tập trung (JWT sub)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// ID nhân viên được gán vai trò
        /// </summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// Khoá ngoại tới bảng vai trò (RoleEntity)
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Ngày bắt đầu hiệu lực của vai trò
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// Ngày kết thúc hiệu lực của vai trò
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }

        /// <summary>
        /// Navigation property tới vai trò
        /// </summary>
        public virtual RoleEntity Role { get; set; } = null!;
    }
}
