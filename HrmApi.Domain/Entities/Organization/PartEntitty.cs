using HrmApi.Domain.Common;
using System;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Tổ/Nhóm cụ thể — bản ghi thực tế của một tổ/nhóm được tạo ra từ
    /// <see cref="PartMasterEntity"/> và gắn vào một phòng ban cụ thể.
    /// Ví dụ: mẫu "Tổ cắt" (PartMaster) được tạo thành "Tổ cắt 1" thuộc Phòng Sản xuất (Department).
    /// </summary>
    public class PartEntity : BaseEntity
    {
        /// <summary>Mã riêng của tổ/nhóm (nếu cần đặt mã khác với mẫu, ví dụ "TOCAT-01")</summary>
        public string? Code { get; set; }

        /// <summary>Tên riêng của tổ/nhóm (nếu để trống, hệ thống lấy tên từ PartMaster)</summary>
        public string? Name { get; set; }

        /// <summary>Ghi chú thêm cho tổ/nhóm này (khác biệt so với mô tả chung của mẫu)</summary>
        public string? Description { get; set; }

        /// <summary>Id công ty (denormalize từ Branch/Department để truy vấn nhanh)</summary>
        public Guid? CompanyId { get; set; }

        /// <summary>Id chi nhánh mà tổ/nhóm này trực thuộc</summary>
        public Guid? BranchId { get; set; }

        /// <summary>Id mẫu tổ/nhóm (Part Master) mà bản ghi này được tạo ra từ đó</summary>
        public Guid? PartMasterId { get; set; }

        /// <summary>Id phòng ban mà tổ/nhóm này trực thuộc</summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>Id nhân viên là Tổ trưởng / Trưởng nhóm</summary>
        public Guid? ManagerId { get; set; }

        /// <summary>Số lượng nhân sự định biên của tổ/nhóm này</summary>
        public int? Limit { get; set; }

        /// <summary>Trạng thái kích hoạt (true: đang hoạt động, false: vô hiệu – không xóa dữ liệu)</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Thứ tự hiển thị tổ/nhóm trong danh sách của phòng ban</summary>
        public int DisplayOrder { get; set; }
    }
}