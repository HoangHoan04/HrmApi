using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Employee
{
    /// <summary>
    /// Danh sách các chứng chỉ kỹ năng/nghề nghiệp của nhân sự
    /// </summary>
    public class EmployeeCertificateEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Tên chứng chỉ (Ví dụ: AWS Cloud Architect) (Require)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Đơn vị/Tổ chức cấp phát
        /// </summary>
        public string? IssuingOrganization { get; set; }

        /// <summary>
        /// Ngày cấp chứng chỉ
        /// </summary>
        public DateTime? IssueDate { get; set; }

        /// <summary>
        /// Ngày chứng chỉ hết hạn
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Mã số chứng chỉ xác thực trực tuyến
        /// </summary>
        public string? CredentialId { get; set; }

        /// <summary>
        /// URL ảnh chứng chỉ
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên sở hữu chứng chỉ này
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }


    }
}
