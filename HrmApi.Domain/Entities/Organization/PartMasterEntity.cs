using System;
using System.Collections.Generic;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// 
    /// Mẫu Tổ/Nhóm (Part Master) — định nghĩa danh mục các loại tổ/nhóm dùng chung
    /// cho công ty hoặc chi nhánh (ví dụ: "Tổ cắt", "Tổ may", "Tổ đóng gói").
    /// 
    /// </summary>
    public class PartMasterEntity : BaseEntity
    {
        /// <summary>
        /// Mã mẫu tổ/nhóm (duy nhất, dùng để nhận diện)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên mẫu tổ/nhóm
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chức năng, phạm vi công việc của tổ/nhóm
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty sở hữu mẫu tổ/nhóm này (mẫu áp dụng ở phạm vi công ty)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Id chi nhánh sở hữu mẫu tổ/nhóm này (null nếu mẫu áp dụng chung cho mọi chi nhánh của công ty)
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Loại tổ/nhóm: Sản xuất, Hỗ trợ, Kỹ thuật, … (có thể dùng enum)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt (true: đang sử dụng, false: vô hiệu – không xóa dữ liệu)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị trên danh sách chọn mẫu tổ/nhóm
        /// </summary>
        public int DisplayOrder { get; set; }

        // --- Quan hệ ---

        /// <summary>
        /// Danh sách mẫu chức danh (Position Master) có thể gán cho tổ/nhóm thuộc mẫu này
        /// </summary>
        public List<PositionMasterEntity> PositionMasters { get; set; } = new List<PositionMasterEntity>();

        /// <summary>
        /// Danh sách các tổ/nhóm cụ thể (tại từng phòng ban) được tạo từ mẫu này
        /// </summary>
        public List<PartEntity> Parts { get; set; } = new List<PartEntity>();
    }
}