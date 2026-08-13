using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Payroll
{
    /// <summary>
    /// Phiếu lương / bảng lương tháng của một nhân viên.
    /// Mỗi nhân viên - tháng/năm chỉ có một phiếu lương hiệu lực.
    /// </summary>
    public class SalaryEntity : BaseEntity
    {
        /// <summary>
        /// Id nhân viên (Require)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Id cấu hình lương áp dụng (nếu có)
        /// </summary>
        public Guid? SalaryConfigId { get; set; }

        /// <summary>
        /// Năm kỳ lương (Require)
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tháng kỳ lương 1-12 (Require)
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Mã kỳ lương (ví dụ "2026-07"), duy nhất theo nhân viên
        /// </summary>
        public string PeriodCode { get; set; } = string.Empty;

        /// <summary>
        /// Ngày chi trả lương
        /// </summary>
        public DateTime? PayDate { get; set; }

        /// <summary>
        /// Trạng thái phiếu lương (DRAFT, PROCESSING, APPROVED, PAID, CANCELLED)
        /// </summary>
        public string Status { get; set; } = SalaryStatus.Draft;

        /// <summary>
        /// Snapshot công ty tại thời điểm tính lương
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Snapshot chi nhánh
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Snapshot phòng ban
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Snapshot chức vụ
        /// </summary>
        public Guid? PositionId { get; set; }

        /// <summary>
        /// Số ngày công chuẩn
        /// </summary>
        public decimal? StandardWorkingDays { get; set; }

        /// <summary>
        /// Số ngày công thực tế
        /// </summary>
        public decimal? ActualWorkingDays { get; set; }

        /// <summary>
        /// Lương cơ bản kỳ này
        /// </summary>
        public decimal BasicSalary { get; set; }

        /// <summary>
        /// Tổng thu nhập (gross) = lương + phụ cấp + thưởng + OT...
        /// </summary>
        public decimal GrossSalary { get; set; }

        /// <summary>
        /// Tổng khấu trừ
        /// </summary>
        public decimal TotalDeduction { get; set; }

        /// <summary>
        /// Lương thực nhận (net)
        /// </summary>
        public decimal NetSalary { get; set; }

        /// <summary>
        /// Lương đóng BHXH
        /// </summary>
        public decimal? InsuranceSalary { get; set; }

        /// <summary>
        /// Đơn vị tiền tệ
        /// </summary>
        public string Currency { get; set; } = CurrencyCode.Vnd;

        /// <summary>
        /// URL file phiếu lương PDF
        /// </summary>
        public string? PayslipFileUrl { get; set; }

        /// <summary>
        /// Ngày duyệt phiếu lương
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Người duyệt
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Ngày đánh dấu đã chi trả
        /// </summary>
        public DateTime? PaidDate { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }

        public virtual EmployeeEntity? Employee { get; set; }
        public virtual SalaryConfigEntity? SalaryConfig { get; set; }
        public virtual CompanyEntity? Company { get; set; }
        public virtual BranchEntity? Branch { get; set; }
        public virtual DepartmentEntity? Department { get; set; }
        public virtual PositionEntity? Position { get; set; }
        public virtual ICollection<SalaryLineItemEntity> LineItems { get; set; } = [];
    }
}
