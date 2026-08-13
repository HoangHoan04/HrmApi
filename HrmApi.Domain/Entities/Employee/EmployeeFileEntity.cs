using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Employee
{
    /// <summary>
    /// Bảng lưu trữ file/tài liệu đính kèm của nhân viên (CCCD, bằng cấp, hợp đồng, CV, ảnh chân dung...)
    /// </summary>
    public class EmployeeFileEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên sở hữu file (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Loại tài liệu (CCCD, Bằng cấp, Hợp đồng, CV, Ảnh chân dung, Khác...)
        /// </summary>
        public string FileCategory { get; set; } = string.Empty;

        /// <summary>
        /// Tên file gốc khi upload
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn/URL lưu trữ file (S3, storage nội bộ,...)
        /// </summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Loại nội dung file (Content-Type, ví dụ: application/pdf, image/png)
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Dung lượng file (byte)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Ghi chú/mô tả thêm về file
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Ngày hết hạn của tài liệu (Nếu có, ví dụ: CCCD, giấy khám sức khỏe)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Số phiên bản trong cùng FileCategory (1, 2, 3…).
        /// </summary>
        public int VersionNo { get; set; } = 1;

        /// <summary>
        /// File phiên bản trước (nếu đây là bản thay thế).
        /// </summary>
        public Guid? ReplacesFileId { get; set; }

        /// <summary>
        /// Bản hiện hành của loại tài liệu (chỉ một IsCurrent=true / category / employee).
        /// </summary>
        public bool IsCurrent { get; set; } = true;

        /// <summary>
        /// Navigation property tới nhân viên sở hữu file này
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }

        public virtual EmployeeFileEntity? ReplacesFile { get; set; }
    }
}
