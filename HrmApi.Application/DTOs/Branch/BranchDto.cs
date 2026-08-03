using System;
using System.ComponentModel.DataAnnotations;

namespace HrmApi.Application.DTOs.Branch
{
    /// <summary>
    /// DTO cho chi nhánh
    /// </summary>
    public class BranchDto : BaseDto
    {
        /// <summary>
        /// Mã chi nhánh (duy nhất)
        /// </summary>
        [Required(ErrorMessage = "Mã chi nhánh là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Mã chi nhánh không vượt quá 50 ký tự")]
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        [Required(ErrorMessage = "Tên chi nhánh là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tên chi nhánh không vượt quá 255 ký tự")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Tên viết tắt / tên hiển thị ngắn gọn
        /// </summary>
        [MaxLength(100, ErrorMessage = "Tên viết tắt không vượt quá 100 ký tự")]
        public string ShortName { get; set; } = string.Empty;
        /// <summary>
        /// Mô tả chi nhánh
        /// </summary>
        [MaxLength(500, ErrorMessage = "Mô tả chi nhánh không vượt quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Loại chi nhánh
        /// </summary>
        [MaxLength(50, ErrorMessage = "Loại chi nhánh không vượt quá 50 ký tự")]
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Id công ty sở hữu
        /// </summary>
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// Tên công ty sở hữu
        /// </summary>
        public string? CompanyName { get; set; }
        /// <summary>
        /// Id chi nhánh cha
        /// </summary>
        public Guid? ParentBranchId { get; set; }
        /// <summary>
        /// Tên chi nhánh cha
        /// </summary>
        public string? ParentBranchName { get; set; }
        /// <summary>
        /// Là trụ sở chính
        /// </summary>
        public bool IsHeadQuarter { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Quốc gia
        /// </summary>
        public string? Country { get; set; }
        /// <summary>
        /// Thành phố
        /// </summary>
        public string? City { get; set; }
        /// <summary>
        /// Quận/huyện
        /// </summary>
        public string? District { get; set; }
        /// <summary>
        /// Phường/xã
        /// </summary>
        public string? Ward { get; set; }
        /// <summary>
        /// Kinh độ
        /// </summary>
        public double? Latitude { get; set; }
        /// <summary>
        /// Vĩ độ
        /// </summary>
        public double? Longitude { get; set; }
        /// <summary>
        /// Điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Số fax
        /// </summary>
        public string? Fax { get; set; }
        /// <summary>
        /// IP Address (địa chỉ IP của chi nhánh, dùng cho việc xác thực đăng nhập)
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
        /// <summary>
        /// Id nhân viên là Giám đốc chi nhánh
        /// </summary>
        public Guid? ManagerId { get; set; }
        /// <summary>
        /// Tên nhân viên là Giám đốc chi nhánh
        /// </summary>
        public string? ManagerName { get; set; }
        /// <summary>
        /// Số điện thoại của Giám đốc chi nhánh
        /// </summary>
        public string? ManagerPhone { get; set; }
        /// <summary>
        /// Mã số thuế (Tax Code)
        /// </summary>
        public string? TaxCode { get; set; }
        /// <summary>
        /// Mã đăng ký kinh doanh
        /// </summary>
        public string? BusinessRegistrationCode { get; set; }
        /// <summary>
        /// Ngày thành lập
        /// </summary>
        public DateTime? OpeningDate { get; set; }
        /// <summary>
        /// Ngày đóng cửa (nếu có)
        /// </summary>
        public DateTime? ClosingDate { get; set; }
        /// <summary>
        /// Trạng thái hoạt động (Active, Inactive, Pending, ...)
        /// </summary>
        public string? OperatingStatus { get; set; }
        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Chi nhánh có sử dụng hệ thống HRM hay không
        /// </summary>
        public bool IsUsingHrm { get; set; }
        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// Nhóm lương (Salary Group) của chi nhánh, dùng để xác định bảng lương áp dụng cho chi nhánh
        /// </summary>
        public string GroupSalary { get; set; } = string.Empty;
        /// <summary>
        /// Id chuẩn chấm công (Time Keeping Standard) áp dụng cho chi nhánh
        /// </summary>
        public Guid? TimeKeepingStandardId { get; set; }
        /// <summary>
        /// Sức chứa tối đa của chi nhánh (số lượng nhân viên tối đa)
        /// </summary>
        public int? MaxEmployeeCapacity { get; set; }
        /// <summary>
        /// Múi giờ
        /// </summary>
        public string? TimeZone { get; set; }
    }
}