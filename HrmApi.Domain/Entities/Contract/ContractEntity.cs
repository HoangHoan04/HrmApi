using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Contract
{
    /// <summary>
    /// Hợp đồng lao động của nhân viên (thử việc, chính thức, thời vụ,...).
    /// Mỗi lần ký mới/tái ký/gia hạn hợp đồng sẽ tạo một bản ghi riêng, liên kết tới hợp đồng trước đó
    /// qua <see cref="PreviousContractId"/> để truy vết toàn bộ lịch sử hợp đồng của nhân viên.
    /// </summary>
    public class ContractEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên sở hữu hợp đồng (khóa ngoại tới EmployeeEntity) (Require)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Id loại hợp đồng (khóa ngoại tới ContractTypeEntity)
        /// </summary>
        public Guid? ContractTypeId { get; set; }

        /// <summary>
        /// Số/Mã hợp đồng (duy nhất, dùng để nhận diện, ví dụ "HĐLĐ-2026-0012")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Ngày ký hợp đồng
        /// </summary>
        public DateTime? SignDate { get; set; }

        /// <summary>
        /// Ngày hợp đồng bắt đầu có hiệu lực (Require)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày hợp đồng hết hạn (Để trống nếu là hợp đồng không xác định thời hạn)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Chức danh công việc theo hợp đồng (Snapshot tại thời điểm ký, có thể khác chức vụ hiện tại của nhân viên)
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Địa điểm làm việc theo hợp đồng
        /// </summary>
        public string? WorkingLocation { get; set; }

        /// <summary>
        /// Mức lương cơ bản theo hợp đồng
        /// </summary>
        public decimal? BasicSalary { get; set; }

        /// <summary>
        /// Tổng phụ cấp theo hợp đồng (ăn trưa, xăng xe, điện thoại,...)
        /// </summary>
        public decimal? Allowance { get; set; }

        /// <summary>
        /// Mức lương đóng bảo hiểm xã hội theo hợp đồng
        /// </summary>
        public decimal? InsuranceSalary { get; set; }

        /// <summary>
        /// Hình thức trả lương (Tiền mặt, Chuyển khoản,...)
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Id công ty ký hợp đồng (Snapshot tại thời điểm ký)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Id chi nhánh làm việc theo hợp đồng (Snapshot tại thời điểm ký)
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Id phòng ban làm việc theo hợp đồng (Snapshot tại thời điểm ký)
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Id chức vụ theo hợp đồng (Snapshot tại thời điểm ký)
        /// </summary>
        public Guid? PositionId { get; set; }

        /// <summary>
        /// Người đại diện công ty ký hợp đồng
        /// </summary>
        public string? SignedByCompanyRepresentative { get; set; }

        /// <summary>
        /// Họ tên nhân viên ký hợp đồng (Snapshot tại thời điểm ký, phòng khi hồ sơ nhân viên thay đổi sau này)
        /// </summary>
        public string? SignedByEmployeeName { get; set; }

        /// <summary>
        /// Hợp đồng có tự động gia hạn khi hết hạn hay không (Chỉ áp dụng cho loại hợp đồng có thời hạn)
        /// </summary>
        public bool IsAutoRenew { get; set; }

        /// <summary>
        /// Id hợp đồng trước đó mà hợp đồng này được tái ký/gia hạn từ đó (Nếu có)
        /// </summary>
        public Guid? PreviousContractId { get; set; }

        /// <summary>
        /// Lần ký thứ mấy của loại hợp đồng này đối với nhân viên (dùng để kiểm tra MaxRenewalTimes)
        /// </summary>
        public int RenewalTimes { get; set; }

        /// <summary>
        /// Ngày chấm dứt hợp đồng trước thời hạn (Nếu có)
        /// </summary>
        public DateTime? TerminationDate { get; set; }

        /// <summary>
        /// Lý do chấm dứt hợp đồng trước thời hạn
        /// </summary>
        public string? TerminationReason { get; set; }

        /// <summary>
        /// Trạng thái hợp đồng (Nháp, Chờ ký, Hiệu lực, Sắp hết hạn, Hết hạn, Đã chấm dứt, Đã thanh lý,...)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// URL/đường dẫn file hợp đồng đã ký (bản scan/PDF)
        /// </summary>
        public string? FileUrl { get; set; }

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        public string? Note { get; set; }

        /* Navigation Relations */

        /// <summary>
        /// Navigation property tới nhân viên sở hữu hợp đồng này
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }

        /// <summary>
        /// Navigation property tới loại hợp đồng
        /// </summary>
        public virtual ContractTypeEntity? ContractType { get; set; }

        /// <summary>
        /// Navigation property tới công ty ký hợp đồng
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }

        /// <summary>
        /// Navigation property tới chi nhánh làm việc theo hợp đồng
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }

        /// <summary>
        /// Navigation property tới phòng ban làm việc theo hợp đồng
        /// </summary>
        public virtual DepartmentEntity? Department { get; set; }

        /// <summary>
        /// Navigation property tới chức vụ theo hợp đồng
        /// </summary>
        public virtual PositionEntity? Position { get; set; }

        /// <summary>
        /// Navigation property tới hợp đồng trước đó (nếu hợp đồng này là bản tái ký/gia hạn)
        /// </summary>
        public virtual ContractEntity? PreviousContract { get; set; }

        /// <summary>
        /// Danh sách các hợp đồng được tái ký/gia hạn từ hợp đồng này
        /// </summary>
        public virtual ICollection<ContractEntity> RenewedContracts { get; set; } = [];

        /// <summary>
        /// Danh sách các đợt đánh giá/đề xuất gia hạn gắn với hợp đồng này
        /// </summary>
        public virtual ICollection<ReviewRenewalEntity> ReviewRenewals { get; set; } = [];

    }
}