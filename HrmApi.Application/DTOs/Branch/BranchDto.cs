using System.ComponentModel.DataAnnotations;

namespace HrmApi.Application.DTOs.Branch
{

    public class BranchDto : BaseDto
    {
        [Required(ErrorMessage = "Mã chi nhánh là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Mã chi nhánh không vượt quá 50 ký tự")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tên chi nhánh là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tên chi nhánh không vượt quá 255 ký tự")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "Tên viết tắt không vượt quá 100 ký tự")]
        public string ShortName { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Mô tả chi nhánh không vượt quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;
        [MaxLength(50, ErrorMessage = "Loại chi nhánh không vượt quá 50 ký tự")]
        public string Type { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? ParentBranchId { get; set; }
        public string? ParentBranchName { get; set; }
        public bool IsHeadQuarter { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Fax { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerPhone { get; set; }
        public string? TaxCode { get; set; }
        public string? BusinessRegistrationCode { get; set; }
        public DateTime? OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string? OperatingStatus { get; set; }
        public bool IsActive { get; set; }
        public bool IsUsingHrm { get; set; }
        public int DisplayOrder { get; set; }
        public string GroupSalary { get; set; } = string.Empty;
        public Guid? TimeKeepingStandardId { get; set; }
        public int? MaxEmployeeCapacity { get; set; }
        public string? TimeZone { get; set; }
    }
    public class BranchSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
    }

}
