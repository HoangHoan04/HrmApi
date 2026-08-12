using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Employee
{
    /// <summary>
    /// Lịch sử học vấn, trình độ chuyên môn của nhân sự
    /// </summary>
    public class EmployeeEducationEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Tên trường Đại học/Cao đẳng (Require)
        /// </summary>
        public string SchoolName { get; set; } = string.Empty;

        /// <summary>
        /// Bằng cấp (Cử nhân, Kỹ sư, Thạc sĩ)
        /// </summary>
        public string? Degree { get; set; }

        /// <summary>
        /// Chuyên ngành học
        /// </summary>
        public string? Major { get; set; }

        /// <summary>
        /// Ngày bắt đầu học
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày tốt nghiệp
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Điểm số tích lũy (GPA)
        /// </summary>
        public string? Gpa { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên sở hữu bản ghi học vấn này
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }


    }
}
