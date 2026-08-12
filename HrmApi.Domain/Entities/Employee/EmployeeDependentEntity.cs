using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Employee
{
    /// <summary>
    /// Bảng thông tin người phụ thuộc của nhân viên (dùng để giảm trừ gia cảnh, khai báo BHXH...)
    /// </summary>
    public class EmployeeDependentEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên sở hữu người phụ thuộc này (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Họ tên đầy đủ người phụ thuộc
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Mối quan hệ với nhân viên (Con, Vợ/Chồng, Cha, Mẹ,...)
        /// </summary>
        public string Relationship { get; set; } = string.Empty;

        /// <summary>
        /// Ngày sinh người phụ thuộc
        /// </summary>
        public DateTime? DayOfBirth { get; set; }

        /// <summary>
        /// Giới tính người phụ thuộc
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Số CCCD/Giấy khai sinh của người phụ thuộc (Nếu có)
        /// </summary>
        public string? IdentityNumber { get; set; }

        /// <summary>
        /// Mã số thuế người phụ thuộc (Dùng để đăng ký giảm trừ gia cảnh)
        /// </summary>
        public string? TaxCode { get; set; }

        /// <summary>
        /// Ngày bắt đầu được tính giảm trừ gia cảnh
        /// </summary>
        public DateTime? DependentFromDate { get; set; }

        /// <summary>
        /// Ngày kết thúc được tính giảm trừ gia cảnh (Nếu có)
        /// </summary>
        public DateTime? DependentToDate { get; set; }

        /// <summary>
        /// Trạng thái đăng ký giảm trừ (Đang hiệu lực, Đã hủy,...)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        public string? Note { get; set; }

        public virtual EmployeeEntity? Employee { get; set; }
    }
}
