using System;
using System.Collections.Generic;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Bảng thông tin công ty / tổ chức
    /// </summary>
    public class CompanyEntity : BaseEntity
    {
        /// <summary>
        /// Mã công ty (duy nhất, dùng để nhận diện)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên công ty (tiếng Việt hoặc tên đầy đủ)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả công ty (lĩnh vực, quy mô, …)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Địa chỉ trụ sở chính (có thể là đầy đủ số nhà, đường, phường/xã)
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Mã số thuế của công ty
        /// </summary>
        public string? TaxCode { get; set; }

        /// <summary>
        /// Số điện thoại hotline (liên hệ chung)
        /// </summary>
        public string? Hotline { get; set; }

        /// <summary>
        /// Tiền tố tạo mã nhân viên nam (ví dụ: "NV-M-")
        /// </summary>
        public string? PrefixMaleCode { get; set; }

        /// <summary>
        /// Tiền tố tạo mã nhân viên nữ (ví dụ: "NV-F-")
        /// </summary>
        public string? PrefixFemaleCode { get; set; }

        /// <summary>
        /// Tiền tố tạo mã nhân viên chính thức (full-time)
        /// </summary>
        public string? PrefixFullTimeCode { get; set; }

        /// <summary>
        /// Tiền tố tạo mã nhân viên bán thời gian (part-time)
        /// </summary>
        public string? PrefixPartTimeCode { get; set; }

        /// <summary>
        /// Id của công ty mẹ (nếu có) – dùng để xây dựng cây tổ chức công ty
        /// </summary>
        public Guid? ParentId { get; set; } = null;

        /// <summary>
        /// Ngày bắt đầu tính lương trong tháng (ví dụ: ngày 20) để xác định chu kỳ lương
        /// </summary>
        public DateTime? DayComputeSalary { get; set; }

        /// <summary>
        /// Có tính lương vào tháng trước không? (true: tính vào kỳ lương tháng trước)
        /// </summary>
        public bool? IsComputePrevMonth { get; set; }

        /// <summary>
        /// Email liên hệ chính của công ty
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Địa chỉ website công ty
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Số fax
        /// </summary>
        public string? Fax { get; set; }

        /// <summary>
        /// Quốc gia (mã hoặc tên)
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Thành phố / Tỉnh
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Quận / Huyện
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// Phường / Xã
        /// </summary>
        public string? Ward { get; set; }

        /// <summary>
        /// Mã số đăng ký kinh doanh (nếu khác mã số thuế)
        /// </summary>
        public string? BusinessRegistrationCode { get; set; }

        /// <summary>
        /// Ngày thành lập công ty
        /// </summary>
        public DateTime? FoundedDate { get; set; }

        /// <summary>
        /// Trạng thái hoạt động: Hoạt động, Tạm ngừng, Giải thể, … (có thể dùng enum)
        /// </summary>
        public string? OperatingStatus { get; set; }

        /// <summary>
        /// Họ tên người đại diện pháp luật
        /// </summary>
        public string? LegalRepresentative { get; set; }

        /// <summary>
        /// Chức danh của người đại diện (Giám đốc, Chủ tịch, …)
        /// </summary>
        public string? LegalRepresentativePosition { get; set; }

        /// <summary>
        /// Loại hình doanh nghiệp: TNHH, Cổ phần, Doanh nghiệp tư nhân, …
        /// </summary>
        public string? CompanyType { get; set; }

        /// <summary>
        /// Ngành nghề kinh doanh chính
        /// </summary>
        public string? Industry { get; set; }

        /// <summary>
        /// Số tài khoản ngân hàng của công ty
        /// </summary>
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string? BankName { get; set; }

        /// <summary>
        /// Chi nhánh ngân hàng
        /// </summary>
        public string? BankBranch { get; set; }

        /// <summary>
        /// Múi giờ làm việc (ví dụ: "SE Asia Standard Time")
        /// </summary>
        public string? TimeZone { get; set; }

        /// <summary>
        /// Ngôn ngữ mặc định của hệ thống cho công ty này (ví dụ: "vi-VN", "en-US")
        /// </summary>
        public string? DefaultLanguage { get; set; }

        /// <summary>
        /// Đường dẫn URL đến logo của công ty
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt (true: đang hoạt động, false: vô hiệu – không xóa dữ liệu)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Mã số tham gia bảo hiểm xã hội (có thể trùng hoặc khác mã số thuế)
        /// </summary>
        public string? SocialInsuranceCode { get; set; }

        /// <summary>
        /// Id của chuẩn chấm công áp dụng cho công ty này (nếu có)
        /// </summary>
        public Guid? TimeKeepingStandardId { get; set; }

        // --- Quan hệ ---

        /// <summary>
        /// Danh sách công ty con (cấu trúc cây)
        /// </summary>
        public List<CompanyEntity> ChildCompanies { get; set; } = new List<CompanyEntity>();

        /// <summary>
        /// Danh sách chi nhánh thuộc công ty
        /// </summary>
        public List<BranchEntity> Branches { get; set; } = new List<BranchEntity>();
    }
}