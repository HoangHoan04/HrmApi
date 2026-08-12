using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Domain.Entities.EmployeeMovement
{
    /// <summary>
    /// Đơn/Quyết định điều chuyển nhân viên (thuyên chuyển, biệt phái, luân chuyển, chuyển công ty/chi nhánh/phòng ban...).
    /// Đây là bảng "header" của một đợt điều chuyển, mỗi đợt có thể gồm nhiều thay đổi cơ cấu tổ chức
    /// (công ty/chi nhánh/phòng ban/bộ phận/chức vụ) được lưu chi tiết tại <see cref="TransferEmployeePositionEntity"/>.
    /// </summary>
    public class TransferEmployeeEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên được điều chuyển (khóa ngoại tới EmployeeEntity) (Require)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Mã đợt điều chuyển (duy nhất, dùng để nhận diện, ví dụ "DC-2026-001")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Loại điều chuyển (Thuyên chuyển nội bộ, Biệt phái, Luân chuyển, Chuyển công ty, Chuyển chi nhánh,...)
        /// </summary>
        public string TransferType { get; set; } = string.Empty;

        /// <summary>
        /// Ngày đề nghị/lập đơn điều chuyển
        /// </summary>
        public DateTime? RequestDate { get; set; }

        /// <summary>
        /// Ngày quyết định điều chuyển bắt đầu có hiệu lực (Require)
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Ngày dự kiến kết thúc điều chuyển (Áp dụng cho trường hợp biệt phái/luân chuyển có thời hạn)
        /// </summary>
        public DateTime? ExpectedEndDate { get; set; }

        /// <summary>
        /// Ngày thực tế kết thúc/hoàn tất điều chuyển (Nếu có)
        /// </summary>
        public DateTime? ActualEndDate { get; set; }

        /// <summary>
        /// Lý do điều chuyển
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Số quyết định/văn bản điều chuyển
        /// </summary>
        public string? DecisionNumber { get; set; }

        /// <summary>
        /// Ngày ký quyết định điều chuyển
        /// </summary>
        public DateTime? DecisionDate { get; set; }

        /// <summary>
        /// URL/đường dẫn file quyết định điều chuyển đính kèm (Nếu có)
        /// </summary>
        public string? DecisionFileUrl { get; set; }

        /// <summary>
        /// Người phê duyệt điều chuyển (Id hoặc tên người duyệt)
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Ngày phê duyệt điều chuyển
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Trạng thái xử lý đơn điều chuyển (Chờ duyệt, Đã duyệt, Từ chối, Đã hủy, Đã áp dụng,...)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        public string? Note { get; set; }


        /// <summary>
        /// Navigation property tới nhân viên được điều chuyển
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }

        /// <summary>
        /// Danh sách chi tiết thay đổi cơ cấu tổ chức/chức vụ phát sinh từ đợt điều chuyển này
        /// (Ví dụ: một đợt điều chuyển có thể vừa đổi phòng ban vừa đổi chức vụ)
        /// </summary>
        public virtual ICollection<TransferEmployeePositionEntity> TransferDetails { get; set; } = [];

    }
}
