using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Mẫu Tổ/Nhóm (Part Master) — định nghĩa danh mục các loại bộ phận dùng chung
    /// cho công ty hoặc chi nhánh (ví dụ: "Tổ cắt", "Tổ may", "Tổ đóng gói").
    /// </summary>
    public class PartMasterEntity : BaseEntity
    {

        /// <summary>
        /// Mã mẫu bộ phận (duy nhất, dùng để nhận diện)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên mẫu bộ phận
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chức năng, phạm vi công việc của bộ phận
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty sở hữu mẫu bộ phận này (mẫu áp dụng ở phạm vi công ty)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Id chi nhánh sở hữu mẫu bộ phận này (null nếu mẫu áp dụng chung cho mọi chi nhánh của công ty)
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Loại bộ phận: Sản xuất, Hỗ trợ, Kỹ thuật, … (có thể dùng enum)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt (true: đang sử dụng, false: vô hiệu – không xóa dữ liệu)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị trên danh sách chọn mẫu bộ phận
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Danh sách các bộ phận cụ thể (tại từng phòng ban) được tạo từ mẫu này
        /// </summary>
        public List<PartEntity> Parts { get; set; } = [];

        /// <summary>
        /// Navigation property tới công ty sở hữu mẫu bộ phận này
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh sở hữu mẫu bộ phận này
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }

    }
}
