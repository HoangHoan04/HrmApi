using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Chức danh cụ thể — bản ghi thực tế của một chức danh được tạo ra từ
    /// <see cref="PositionMasterEntity"/> và gắn vào một phòng ban / tổ cụ thể.
    /// Ví dụ: mẫu "Nhân viên vận hành máy" (PositionMaster) được gán cho Tổ cắt 1 (Part) thuộc Phòng Sản xuất (Department).
    /// </summary>
    public class PositionEntity : BaseEntity
    {

        /// <summary>
        /// Id công ty (denormalize từ Branch/Department để truy vấn nhanh)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Id chi nhánh mà chức danh này trực thuộc
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Id mẫu chức danh (Position Master) mà bản ghi này được tạo ra từ đó
        /// </summary>
        public Guid? PositionMasterId { get; set; }

        /// <summary>
        /// Id phòng ban mà chức danh này trực thuộc
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Id tổ/nhóm mà chức danh này trực thuộc (nếu chức danh gắn ở cấp tổ thay vì cả phòng ban)
        /// </summary>
        public Guid? PartId { get; set; }

        /// <summary>
        /// Số lượng định biên thực tế của chức danh này tại phòng ban/tổ (có thể khác QuantityStandard của mẫu)
        /// </summary>
        public int? QuantityStandard { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt (true: đang sử dụng, false: vô hiệu – không xóa dữ liệu)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị chức danh trong danh sách của phòng ban/tổ
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// Navigation property tới công ty mà chức danh này trực thuộc
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }

        /// <summary>
        /// Navigation property tới chi nhánh mà chức danh này trực thuộc
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }

        /// <summary>
        /// Navigation property tới mẫu chức danh mà bản ghi này được tạo ra từ đó
        /// </summary>
        public virtual PositionMasterEntity? PositionMaster { get; set; }
        /// <summary>
        /// Navigation property tới phòng ban mà chức danh này trực thuộc
        /// </summary>
        public virtual DepartmentEntity? Department { get; set; }
        /// <summary>
        /// Navigation property tới tổ/nhóm mà chức danh này trực thuộc
        /// </summary>
        public virtual PartEntity? Part { get; set; }
        /// <summary>
        /// Navigation property tới danh sách nhân viên đang đảm nhiệm chức danh này
        /// </summary>
        public virtual ICollection<EmployeeEntity> EmployeeEntities { get; set; } = [];


    }
}