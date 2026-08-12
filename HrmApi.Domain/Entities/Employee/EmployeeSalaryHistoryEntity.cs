using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Employee
{
    /// <summary>
    /// Lịch sử thay đổi lương của nhân viên (tăng lương, điều chỉnh lương, phụ cấp...)
    /// </summary>
    public class EmployeeSalaryHistoryEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Ngày quyết định có hiệu lực (Require)
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Lương cơ bản trước khi thay đổi
        /// </summary>
        public decimal? OldBasicSalary { get; set; }

        /// <summary>
        /// Lương cơ bản sau khi thay đổi (Require)
        /// </summary>
        public decimal NewBasicSalary { get; set; }

        /// <summary>
        /// Tổng phụ cấp (ăn trưa, xăng xe, điện thoại,...)
        /// </summary>
        public decimal? Allowance { get; set; }

        /// <summary>
        /// Loại thay đổi (Tăng lương định kỳ, Xét duyệt đột xuất, Điều chỉnh vị trí,...)
        /// </summary>
        public string? ChangeType { get; set; }

        /// <summary>
        /// Lý do thay đổi lương
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Số quyết định/văn bản liên quan
        /// </summary>
        public string? DecisionNumber { get; set; }

        /// <summary>
        /// Người phê duyệt thay đổi lương (Id hoặc tên người duyệt)
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên sở hữu lịch sử lương này
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }


    }
}