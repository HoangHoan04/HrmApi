using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Cấu hình bảng lương / nhóm lương (được Branch.GroupSalary tham chiếu theo Code).
    /// </summary>
    public class SalaryConfigEntity : BaseEntity
    {
        /// <summary>
        /// Mã cấu hình lương (duy nhất theo công ty, ví dụ "SAL-DEFAULT")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên cấu hình lương
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty sở hữu cấu hình (null = dùng chung hệ thống)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Số ngày công chuẩn trong tháng để tính lương
        /// </summary>
        public int StandardWorkingDays { get; set; } = 26;

        /// <summary>
        /// Tỷ lệ BHXH người lao động đóng (%)
        /// </summary>
        public decimal BhxhEmployeeRate { get; set; } = 8m;

        /// <summary>
        /// Tỷ lệ BHYT người lao động đóng (%)
        /// </summary>
        public decimal BhytEmployeeRate { get; set; } = 1.5m;

        /// <summary>
        /// Tỷ lệ BHTN người lao động đóng (%)
        /// </summary>
        public decimal BhtnEmployeeRate { get; set; } = 1m;

        /// <summary>
        /// Ngày chi trả lương mặc định trong tháng (1-31)
        /// </summary>
        public int? DefaultPayDay { get; set; } = 5;

        /// <summary>
        /// Có tính lương tháng trước hay không
        /// </summary>
        public bool IsComputePrevMonth { get; set; }

        /// <summary>
        /// Đơn vị tiền tệ mặc định
        /// </summary>
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }

        public virtual CompanyEntity? Company { get; set; }
        public virtual ICollection<SalaryEntity> Salaries { get; set; } = [];
    }
}
