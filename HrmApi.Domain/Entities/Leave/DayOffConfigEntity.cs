using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Cấu hình loại nghỉ phép / số ngày phép theo năm
    /// </summary>
    public class DayOffConfigEntity : BaseEntity
    {
        /// <summary>
        /// Mã loại nghỉ phép (ví dụ: ANNUAL, SICK, UNPAID, OTHER)
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Tên loại nghỉ phép (ví dụ: Nghỉ phép năm, Nghỉ ốm, Nghỉ không lương, Khác)
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Mô tả chi tiết về loại nghỉ phép
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Id công ty (khóa ngoại tới CompanyEntity)
        /// </summary>
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// Loại nghỉ phép
        /// </summary>
        public HrmApi.Domain.Enums.DayOffType DayOffType { get; set; } = HrmApi.Domain.Enums.DayOffType.ANNUAL;
        /// <summary>
        /// Số ngày phép mặc định trong năm
        /// </summary>
        public decimal DefaultDaysPerYear { get; set; }

        /// <summary>
        /// Có tính lương hay không
        /// </summary>
        public bool IsPaid { get; set; } = true;
        /// <summary>
        /// Có áp dụng cho tất cả nhân viên hay không
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property tới công ty sở hữu cấu hình ngày phép này (CompanyEntity)
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới danh sách nhân viên áp dụng cấu hình ngày phép này (DayOffConfigEmployeeEntity)
        /// </summary>
        public virtual ICollection<DayOffConfigEmployeeEntity> DayOffConfigEmployeeEntities { get; set; } = [];
        /// <summary>
        /// Navigation property tới danh sách đăng ký nghỉ phép áp dụng cấu hình ngày phép này (RegisterDayOffEntity)
        /// </summary>
        public virtual ICollection<RegisterDayOffEntity> RegisterDayOffEntities { get; set; } = [];


    }
}
