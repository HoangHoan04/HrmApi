using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.EmployeeMovement;

namespace HrmApi.Domain.Entities.Employee
{
    /// <summary>
    /// Bảng dữ liệu nhân viên
    /// </summary>
    public class EmployeeEntity : BaseEntity
    {
        /// <summary>
        /// Mã nhân viên (Require)
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Họ nhân viên (Require)
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>
        /// Tên nhân viên 
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        /// <summary>
        /// Họ tên đầy đủ nhân viên ( LastName + FirstName)
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// Giới tính nhân viên
        /// </summary>
        public string? Gender { get; set; }
        /// <summary>
        /// URL ảnh đại diện nhân viên
        /// </summary>
        public string? AvatarUrl { get; set; }
        /// <summary>
        /// Số điện thoại nhân viên
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        /// <summary>
        /// Số điện thoại phụ (Nếu có)
        /// </summary>
        public string? SecondaryPhone { get; set; }
        /// <summary>
        /// Email nhân viên
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Email nội bộ công ty cung cấp (Nếu có)
        /// </summary>
        public string? CompanyEmail { get; set; }
        /// <summary>
        /// Ngày sinh nhân viên
        /// </summary>
        public DateTime DayOfBirth { get; set; }
        /// <summary>
        /// Quốc tịch
        /// </summary>
        public string? Nationality { get; set; }
        /// <summary>
        /// Dân tộc
        /// </summary>
        public string? Ethnicity { get; set; }
        /// <summary>
        /// Tôn giáo
        /// </summary>
        public string? Religion { get; set; }
        /// <summary>
        /// Số CCCD (Căn cước công dân)
        /// </summary>
        public string IdentityCard { get; set; } = string.Empty;
        /// <summary>
        /// Nơi cấp CCCD
        /// </summary>
        public string PlaceOfIsssuance { get; set; } = string.Empty;
        /// <summary>
        /// Ngày cấp CCCD
        /// </summary>
        public DateTime IssuanceDate { get; set; }
        /// <summary>
        /// Địa chỉ thường trú
        /// </summary>
        public string? PermanentAddress { get; set; }
        /// <summary>
        /// Địa chỉ hiện tại (nơi ở hiện tại)
        /// </summary>
        public string? NowAddress { get; set; }
        /// <summary>
        /// Tỉnh/Thành phố hiện tại
        /// </summary>
        public string? CurrentCity { get; set; }
        /// <summary>
        /// Phường/Xã hiện tại
        /// </summary>
        public string? CurrentWard { get; set; }
        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        public string? BankAccountNumber { get; set; }
        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string? BankName { get; set; }
        /// <summary>
        /// Tên chi nhánh ngân hàng
        /// </summary>
        public string? BankBranchName { get; set; }
        /// <summary>
        /// Tên chủ tài khoản ngân hàng
        /// </summary>
        public string? BankAccountHolder { get; set; }
        /// <summary>
        /// Mã số thuế cá nhân
        /// </summary>
        public string? TaxCode { get; set; }
        /// <summary>
        /// Số sổ bảo hiểm xã hội (BHXH)
        /// </summary>
        public string? SocialInsuranceNumber { get; set; }
        /// <summary>
        /// Số thẻ bảo hiểm y tế (BHYT)
        /// </summary>
        public string? HealthInsuranceNumber { get; set; }
        /// <summary>
        /// Cấp bậc / trình độ nhân viên
        /// </summary>
        public string? Level { get; set; }
        /// <summary>
        /// Hình thức làm việc (Full-time, Part-time, Remote,...)
        /// </summary>
        public string? WorkingMode { get; set; }
        /// <summary>
        /// Loại hợp đồng lao động (Thử việc, Chính thức, Thời vụ,...)
        /// </summary>
        public string? ContractType { get; set; }
        /// <summary>
        /// Trạng thái nhân viên (Đang làm việc, Nghỉ việc, Tạm nghỉ,...)
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Ngày vào làm
        /// </summary>
        public DateTime JoinDate { get; set; }
        /// <summary>
        /// Ngày nghỉ việc (Nếu có)
        /// </summary>
        public DateTime? ResignationDate { get; set; }
        /// <summary>
        /// Lý do nghỉ việc (Nếu có)
        /// </summary>
        public string? ResignationReason { get; set; }

        /// <summary>
        /// Công ty mà nhân viên đang làm việc (Khóa ngoại tới bảng CompanyEntity)
        /// </summary>
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// Chi nhánh mà nhân viên đang làm việc (Khóa ngoại tới bảng BranchEntity)
        /// </summary>
        public Guid? BranchId { get; set; }
        /// <summary>
        /// Phòng ban mà nhân viên đang làm việc (Khóa ngoại tới bảng DepartmentEntity)
        /// </summary>
        public Guid? DepartmentId { get; set; }
        /// <summary>
        /// Bộ phận mà nhân viên đang làm việc (Khóa ngoại tới bảng PartEntity)
        /// </summary>
        public Guid? PartId { get; set; }
        /// <summary>
        /// Chức vụ mà nhân viên đang đảm nhiệm (Khóa ngoại tới bảng PositionEntity)
        /// </summary>
        public Guid? PositionId { get; set; }

        /// <summary>
        /// Quản lý trực tiếp (Employee khác cùng hệ thống)
        /// </summary>
        public Guid? DirectManagerId { get; set; }

        /// <summary>
        /// Navigation property tới công ty mà nhân viên đang làm việc
        /// </summary>
        public virtual Organization.CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh mà nhân viên đang làm việc
        /// </summary>
        public virtual Organization.BranchEntity? Branch { get; set; }
        /// <summary>
        /// Navigation property tới phòng ban mà nhân viên đang làm việc
        /// </summary>
        public virtual Organization.DepartmentEntity? Department { get; set; }
        /// <summary>
        /// Navigation property tới bộ phận mà nhân viên đang làm việc
        /// </summary>
        public virtual Organization.PartEntity? Part { get; set; }
        /// <summary>
        /// Navigation property tới chức vụ mà nhân viên đang đảm nhiệm
        /// </summary>
        public virtual Organization.PositionEntity? Position { get; set; }

        /// <summary>
        /// Quản lý trực tiếp
        /// </summary>
        public virtual EmployeeEntity? DirectManager { get; set; }

        /// <summary>
        /// Nhân viên đang báo cáo trực tiếp cho người này
        /// </summary>
        public virtual ICollection<EmployeeEntity> DirectReports { get; set; } = [];

        /// <summary>
        /// ID tài khoản người dùng từ Auth Service (nếu có)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Danh sách người phụ thuộc của nhân viên
        /// </summary>
        public virtual ICollection<EmployeeDependentEntity> Dependents { get; set; } = [];

        /// <summary>
        /// Danh sách quá trình học vấn
        /// </summary>
        public virtual ICollection<EmployeeEducationEntity> Educations { get; set; } = [];

        /// <summary>
        /// Danh sách chứng chỉ kỹ năng/nghề nghiệp
        /// </summary>
        public virtual ICollection<EmployeeCertificateEntity> Certificates { get; set; } = [];

        /// <summary>
        /// Danh sách file/tài liệu đính kèm
        /// </summary>
        public virtual ICollection<EmployeeFileEntity> Files { get; set; } = [];

        /// <summary>
        /// Lịch sử thay đổi lương
        /// </summary>
        public virtual ICollection<EmployeeSalaryHistoryEntity> SalaryHistories { get; set; } = [];

        /// <summary>
        /// Danh sách hợp đồng lao động của nhân viên (thử việc, chính thức, gia hạn,...)
        /// </summary>
        public virtual ICollection<Contract.ContractEntity> Contracts { get; set; } = [];

        /// <summary>
        /// Danh sách các đợt điều chuyển (thuyên chuyển, biệt phái, luân chuyển,...) của nhân viên
        /// </summary>
        public virtual ICollection<TransferEmployeeEntity> TransferEmployees { get; set; } = [];

        /// <summary>
        /// Lịch sử chi tiết thay đổi cơ cấu tổ chức/chức vụ của nhân viên (theo từng đợt điều chuyển)
        /// </summary>
        public virtual ICollection<TransferEmployeePositionEntity> TransferEmployeePositions { get; set; } = [];

    }
}
