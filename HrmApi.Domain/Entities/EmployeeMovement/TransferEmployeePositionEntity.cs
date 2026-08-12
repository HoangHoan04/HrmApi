using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.EmployeeMovement
{
    /// <summary>
    /// Chi tiết thay đổi cơ cấu tổ chức/chức vụ của nhân viên trong một đợt điều chuyển.
    /// Lưu lại giá trị Trước (Old) và Sau (New) của Công ty/Chi nhánh/Phòng ban/Bộ phận/Chức vụ
    /// để phục vụ truy vết lịch sử điều chuyển và đồng bộ dữ liệu hiện tại của <see cref="EmployeeEntity"/>.
    /// </summary>
    public class TransferEmployeePositionEntity : BaseEntity
    {
        /// <summary>
        /// Id đợt điều chuyển (khóa ngoại tới TransferEmployeeEntity) (Require)
        /// </summary>
        public Guid TransferEmployeeId { get; set; }
        /// <summary>
        /// Id nhân viên (denormalize từ TransferEmployeeEntity để truy vấn nhanh, không cần join) (Require)
        /// </summary>
        public Guid EmployeeId { get; set; }
        /// <summary>
        /// Ngày thay đổi bắt đầu có hiệu lực (Thường trùng EffectiveDate của đợt điều chuyển)
        /// </summary>
        public DateTime EffectiveDate { get; set; }
        /// <summary>
        /// Công ty trước khi điều chuyển
        /// </summary>
        public Guid? OldCompanyId { get; set; }
        /// <summary>
        /// Công ty sau khi điều chuyển
        /// </summary>
        public Guid? NewCompanyId { get; set; }
        /// <summary>
        /// Chi nhánh trước khi điều chuyển
        /// </summary>
        public Guid? OldBranchId { get; set; }
        /// <summary>
        /// Chi nhánh sau khi điều chuyển
        /// </summary>
        public Guid? NewBranchId { get; set; }
        /// <summary>
        /// Phòng ban trước khi điều chuyển
        /// </summary>
        public Guid? OldDepartmentId { get; set; }
        /// <summary>
        /// Phòng ban sau khi điều chuyển
        /// </summary>
        public Guid? NewDepartmentId { get; set; }
        /// <summary>
        /// Bộ phận/Tổ trước khi điều chuyển
        /// </summary>
        public Guid? OldPartId { get; set; }
        /// <summary>
        /// Bộ phận/Tổ sau khi điều chuyển
        /// </summary>
        public Guid? NewPartId { get; set; }
        /// <summary>
        /// Chức vụ trước khi điều chuyển
        /// </summary>
        public Guid? OldPositionId { get; set; }
        /// <summary>
        /// Chức vụ sau khi điều chuyển
        /// </summary>
        public Guid? NewPositionId { get; set; }
        /// <summary>
        /// Loại thay đổi (Chuyển phòng ban, Chuyển bộ phận, Bổ nhiệm chức vụ, Thăng chức, Giáng chức, Miễn nhiệm,...)
        /// </summary>
        public string? ChangeType { get; set; }
        /// <summary>
        /// Ghi chú thêm cho thay đổi này (khác biệt so với lý do chung của cả đợt điều chuyển)
        /// </summary>
        public string? Note { get; set; }
        /// <summary>
        /// Navigation property tới đợt điều chuyển chứa thay đổi này
        /// </summary>
        public virtual TransferEmployeeEntity? TransferEmployee { get; set; }
        /// <summary>
        /// Navigation property tới nhân viên được điều chuyển
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }
        /// <summary>
        /// Navigation property tới công ty trước khi điều chuyển
        /// </summary>
        public virtual CompanyEntity? OldCompany { get; set; }
        /// <summary>
        /// Navigation property tới công ty sau khi điều chuyển
        /// </summary>
        public virtual CompanyEntity? NewCompany { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh trước khi điều chuyển
        /// </summary>
        public virtual BranchEntity? OldBranch { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh sau khi điều chuyển
        /// </summary>
        public virtual BranchEntity? NewBranch { get; set; }
        /// <summary>
        /// Navigation property tới phòng ban trước khi điều chuyển
        /// </summary>
        public virtual DepartmentEntity? OldDepartment { get; set; }
        /// <summary>
        /// Navigation property tới phòng ban sau khi điều chuyển
        /// </summary>
        public virtual DepartmentEntity? NewDepartment { get; set; }
        /// <summary>
        /// Navigation property tới bộ phận/tổ trước khi điều chuyển
        /// </summary>
        public virtual PartEntity? OldPart { get; set; }
        /// <summary>
        /// Navigation property tới bộ phận/tổ sau khi điều chuyển
        /// </summary>
        public virtual PartEntity? NewPart { get; set; }
        /// <summary>
        /// Navigation property tới chức vụ trước khi điều chuyển
        /// </summary>
        public virtual PositionEntity? OldPosition { get; set; }
        /// <summary>
        /// Navigation property tới chức vụ sau khi điều chuyển
        /// </summary>
        public virtual PositionEntity? NewPosition { get; set; }

    }
}
