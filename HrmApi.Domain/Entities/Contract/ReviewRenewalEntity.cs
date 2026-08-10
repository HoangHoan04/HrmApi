using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.Contract
{
    /// <summary>
    /// Đợt đánh giá nhân sự trước khi hợp đồng hết hạn và đề xuất phương án xử lý
    /// (Gia hạn, Chuyển loại hợp đồng, Tăng lương, Chấm dứt hợp đồng,...).
    /// Thường được tạo tự động trước ngày hết hạn hợp đồng N ngày (xem <see cref="ContractTypeEntity.NotifyBeforeExpiryDays"/>).
    /// </summary>
    public class ReviewRenewalEntity : BaseEntity
    {

        /// <summary>
        /// Id hợp đồng cần đánh giá/gia hạn (khóa ngoại tới ContractEntity) (Require)
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Id nhân viên (denormalize từ ContractEntity để truy vấn nhanh, không cần join) (Require)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Ngày thực hiện đánh giá
        /// </summary>
        public DateTime? ReviewDate { get; set; }

        /// <summary>
        /// Người thực hiện đánh giá (Id hoặc tên người đánh giá, thường là quản lý trực tiếp)
        /// </summary>
        public string? ReviewedBy { get; set; }

        /// <summary>
        /// Điểm đánh giá hiệu suất làm việc (Thang điểm tùy cấu hình, ví dụ 0-100)
        /// </summary>
        public decimal? PerformanceScore { get; set; }

        /// <summary>
        /// Kết quả đánh giá (Đạt yêu cầu, Không đạt, Cần theo dõi thêm,...)
        /// </summary>
        public string? ReviewResult { get; set; }

        /// <summary>
        /// Nhận xét/đánh giá chi tiết của người đánh giá
        /// </summary>
        public string? ReviewComment { get; set; }

        /// <summary>
        /// Phương án đề xuất (Gia hạn hợp đồng, Chuyển loại hợp đồng, Tăng lương, Chấm dứt hợp đồng, Không thay đổi,...)
        /// </summary>
        public string? Recommendation { get; set; }

        /// <summary>
        /// Id loại hợp đồng mới được đề xuất (Nếu đề xuất chuyển loại hợp đồng, ví dụ từ Thử việc sang Xác định thời hạn)
        /// </summary>
        public Guid? ProposedContractTypeId { get; set; }

        /// <summary>
        /// Ngày bắt đầu hiệu lực đề xuất cho hợp đồng mới
        /// </summary>
        public DateTime? ProposedStartDate { get; set; }

        /// <summary>
        /// Ngày hết hạn đề xuất cho hợp đồng mới (Để trống nếu đề xuất không xác định thời hạn)
        /// </summary>
        public DateTime? ProposedEndDate { get; set; }

        /// <summary>
        /// Mức lương cơ bản đề xuất cho hợp đồng mới
        /// </summary>
        public decimal? ProposedBasicSalary { get; set; }

        /// <summary>
        /// Người phê duyệt phương án đề xuất
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Ngày phê duyệt phương án đề xuất
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Trạng thái xử lý (Chờ đánh giá, Chờ duyệt, Đã duyệt, Từ chối, Đã tạo hợp đồng mới,...)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Id hợp đồng mới được tạo ra sau khi phương án đề xuất được duyệt và áp dụng (Nếu có)
        /// </summary>
        public Guid? NewContractId { get; set; }

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        public string? Note { get; set; }

        /* Navigation Relations */

        /// <summary>
        /// Navigation property tới hợp đồng đang được đánh giá/gia hạn
        /// </summary>
        public virtual ContractEntity? Contract { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên được đánh giá
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }

        /// <summary>
        /// Navigation property tới loại hợp đồng được đề xuất
        /// </summary>
        public virtual ContractTypeEntity? ProposedContractType { get; set; }

        /// <summary>
        /// Navigation property tới hợp đồng mới được tạo ra sau khi phương án được áp dụng
        /// </summary>
        public virtual ContractEntity? NewContract { get; set; }

    }
}