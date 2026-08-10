using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Bảng ánh xạ giữa người dùng và vai trò (role) trong hệ thống. Mỗi bản ghi đại diện cho một mối quan hệ giữa một người dùng và một vai trò cụ thể.
    /// </summary>
    public class UserRoleEntity : BaseEntity
    {
        /// <summary>
        /// Khoá ngoại tới bảng người dùng (UserEntity). Đại diện cho người dùng được gán vai trò.
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Khoá ngoại tới bảng vai trò (RoleEntity). Đại diện cho vai trò được gán cho người dùng.
        /// </summary>
        public Guid RoleId { get; set; }
        /// <summary>
        /// Ngày bắt đầu hiệu lực của vai trò đối với người dùng. Nếu có giá trị, vai trò sẽ chỉ có hiệu lực từ ngày này trở đi.
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }
        /// <summary>
        /// Ngày kết thúc hiệu lực của vai trò đối với người dùng. Nếu có giá trị, vai trò sẽ chỉ có hiệu lực đến ngày này.
        /// </summary>
        public DateTime? EffectiveTo { get; set; }
        /// <summary>
        /// Navigation property tới người dùng (UserEntity) liên kết với vai trò này. Cho phép truy cập thông tin chi tiết của người dùng từ bản ghi UserRoleEntity.
        /// </summary>
        public UserEntity User { get; set; } = null!;
        /// <summary>
        /// Navigation property tới vai trò (RoleEntity) liên kết với người dùng này. Cho phép truy cập thông tin chi tiết của vai trò từ bản ghi UserRoleEntity.
        /// </summary>
        public RoleEntity Role { get; set; } = null!;
    }
}