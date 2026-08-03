using System;
using System.ComponentModel.DataAnnotations;

namespace HrmApi.Application.DTOs
{
    /// <summary>
    /// DTO cho phòng ban
    /// </summary>
    public class DepartmentDto : BaseDto
    {
        /// <summary>
        /// Mã phòng ban (duy nhất)
        /// </summary>
        [Required(ErrorMessage = "Mã phòng ban là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Mã phòng ban không vượt quá 50 ký tự")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tên phòng ban không vượt quá 255 ký tự")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tên viết tắt / tên hiển thị ngắn gọn
        /// </summary>
        [MaxLength(100)]
        public string? ShortName { get; set; }

        /// <summary>
        /// Mô tả chức năng, nhiệm vụ
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại phòng ban (Khối văn phòng, Sản xuất, Kinh doanh, ...)
        /// </summary>
        [MaxLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Id công ty sở hữu (denormalize từ Branch)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Id chi nhánh chứa phòng ban
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Id phòng ban cha (nếu có)
        /// </summary>
        public Guid? ParentDepartmentId { get; set; }

        /// <summary>
        /// Cấp bậc trong cây tổ chức
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Cấp bậc phải lớn hơn hoặc bằng 1")]
        public int Level { get; set; } = 1;

        /// <summary>
        /// Số lượng nhân sự tối đa (định biên)
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tối đa phải >= 0")]
        public int Limit { get; set; }

        /// <summary>
        /// Số lượng nhân sự hiện tại
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? CurrentHeadCount { get; set; }

        /// <summary>
        /// Id nhân viên là Trưởng phòng / Quản lý
        /// </summary>
        public Guid? ManagerId { get; set; }

        /// <summary>
        /// Id nhân viên là Phó phòng / Trợ lý
        /// </summary>
        public Guid? DeputyManagerId { get; set; }

        /// <summary>
        /// Email liên hệ chung
        /// </summary>
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(255)]
        public string? Email { get; set; }

        /// <summary>
        /// Số điện thoại nội bộ (extension)
        /// </summary>
        [MaxLength(20)]
        public string? PhoneExtension { get; set; }

        /// <summary>
        /// Mã trung tâm chi phí (Cost Center)
        /// </summary>
        [MaxLength(50)]
        public string? CostCenterCode { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Ngày thành lập
        /// </summary>
        public DateTime? EstablishedDate { get; set; }

        /// <summary>
        /// Ngày giải thể / ngừng hoạt động
        /// </summary>
        public DateTime? DissolvedDate { get; set; }

        /// <summary>
        /// Có nhận thông báo marketing/nội bộ không
        /// </summary>
        public bool IsNotifyMarketing { get; set; }
    }
}