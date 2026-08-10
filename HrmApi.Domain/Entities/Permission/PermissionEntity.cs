using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Danh mục quyền hạn - định nghĩa từng hành động cụ thể trong hệ thống
    /// </summary>
    public class PermissionEntity : BaseEntity
    {
        /// <summary>
        /// Mã quyền hạn (ví dụ: "USER_CREATE", "USER_DELETE", "LEAVE_APPROVE") - duy nhất trong hệ thống
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Tên quyền hạn (ví dụ: "Tạo người dùng", "Xóa người dùng", "Phê duyệt nghỉ phép") - mô tả ngắn gọn quyền hạn
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Tên module hoặc nhóm chức năng mà quyền hạn này thuộc về (ví dụ: "User Management", "Leave Management") - giúp phân loại quyền hạn
        /// </summary>
        public string Module { get; set; } = string.Empty;
        /// <summary>
        /// Hành động cụ thể mà quyền hạn này cho phép (ví dụ: "Create", "Read", "Update", "Delete") - xác định loại thao tác được phép thực hiện
        /// </summary>
        public string Action { get; set; } = string.Empty;
        /// <summary>
        /// Mô tả chi tiết về quyền hạn, có thể bao gồm các điều kiện hoặc lưu ý khi sử dụng quyền hạn này
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Xác định quyền hạn này có thể áp dụng cho các phạm vi khác nhau (ví dụ: theo phòng ban, theo dự án) hay không. Nếu true, quyền hạn có thể được gán cho các phạm vi cụ thể; nếu false, quyền hạn chỉ áp dụng chung cho tất cả người dùng.
        /// </summary>
        public bool IsScopable { get; set; } = true;
        /// <summary>
        /// Xác định quyền hạn này có phải là quyền hệ thống (không thể xóa hoặc chỉnh sửa) hay không. Nếu true, quyền hạn được coi là quyền hệ thống và không thể bị xóa hoặc chỉnh sửa; nếu false, quyền hạn có thể được quản lý bình thường.
        /// </summary>
        public bool IsSystem { get; set; } = false;
        /// <summary>
        /// Navigation property tới danh sách các RolePermissionEntity liên quan, thể hiện mối quan hệ giữa quyền hạn và các vai trò (roles) trong hệ thống. Mỗi RolePermissionEntity đại diện cho việc gán quyền hạn này cho một vai trò cụ thể.
        /// </summary>
        public List<RolePermissionEntity> RolePermissions { get; set; } = [];

    }
}