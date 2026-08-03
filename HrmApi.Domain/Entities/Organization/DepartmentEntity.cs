using System;
using System.Collections.Generic;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Phòng ban trong mỗi chi nhánh
    /// </summary>
    public class DepartmentEntity : BaseEntity
    {
        /// <summary>
        /// Mã phòng ban (duy nhất, dùng để nhận diện)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tên viết tắt / tên hiển thị ngắn gọn của phòng ban
        /// </summary>
        public string? ShortName { get; set; }

        /// <summary>
        /// Mô tả chức năng, nhiệm vụ của phòng ban
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Loại phòng ban: Khối văn phòng, Khối sản xuất, Khối kinh doanh, … (có thể dùng enum)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Id công ty sở hữu phòng ban (denormalize từ Branch để truy vấn nhanh, không cần join qua Branch)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Id chi nhánh chứa phòng ban này
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Id phòng ban cha (nếu có cấu trúc phòng/ban trực thuộc, ví dụ Phòng Kỹ thuật → Tổ Bảo trì)
        /// </summary>
        public Guid? ParentDepartmentId { get; set; }

        /// <summary>
        /// Cấp bậc của phòng ban trong cây tổ chức (1: cấp cao nhất, tăng dần theo độ sâu)
        /// </summary>
        public int Level { get; set; } = 1;

        // --- Định biên nhân sự ---

        /// <summary>
        /// Số lượng nhân sự tối đa được phép của phòng ban (định biên)
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Số lượng nhân sự hiện tại của phòng ban (denormalize để hiển thị nhanh, cần đồng bộ khi thêm/xóa nhân viên)
        /// </summary>
        public int? CurrentHeadCount { get; set; }

        // --- Người quản lý ---

        /// <summary>
        /// Id nhân viên là Trưởng phòng / Quản lý phòng ban
        /// </summary>
        public Guid? ManagerId { get; set; }

        /// <summary>
        /// Id nhân viên là Phó phòng / Trợ lý quản lý (nếu có)
        /// </summary>
        public Guid? DeputyManagerId { get; set; }

        // --- Liên hệ ---

        /// <summary>
        /// Email liên hệ chung của phòng ban
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Số điện thoại nội bộ (extension) của phòng ban
        /// </summary>
        public string? PhoneExtension { get; set; }

        // --- Kế toán / chi phí ---

        /// <summary>
        /// Mã trung tâm chi phí (Cost Center) dùng cho phân bổ ngân sách, kế toán quản trị
        /// </summary>
        public string? CostCenterCode { get; set; }

        // --- Vận hành / trạng thái ---

        /// <summary>
        /// Trạng thái kích hoạt (true: đang hoạt động, false: vô hiệu – không xóa dữ liệu)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị phòng ban trên danh sách / sơ đồ tổ chức
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Ngày thành lập / bắt đầu hoạt động của phòng ban
        /// </summary>
        public DateTime? EstablishedDate { get; set; }

        /// <summary>
        /// Ngày giải thể / ngừng hoạt động của phòng ban (nếu đã sáp nhập hoặc đóng)
        /// </summary>
        public DateTime? DissolvedDate { get; set; }

        /// <summary>
        /// Có nhận thông báo marketing/nội bộ hay không (ví dụ: email tuyển dụng, sự kiện công ty)
        /// </summary>
        public bool IsNotifyMarketing { get; set; }

        // --- Quan hệ ---

        /// <summary>
        /// Danh sách phòng ban con (nếu có cấu trúc phân cấp phòng/tổ trực thuộc)
        /// </summary>
        public List<DepartmentEntity> ChildDepartments { get; set; } = new List<DepartmentEntity>();
    }
}