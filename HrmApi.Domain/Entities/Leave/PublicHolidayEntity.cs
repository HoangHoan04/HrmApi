using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Leave
{
    /// <summary>
    /// Ngày nghỉ lễ
    /// </summary>
    public class PublicHolidayEntity : BaseEntity
    {
        /// <summary>
        /// Mã ngày nghỉ lễ
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Tên ngày nghỉ lễ
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Id công ty (khóa ngoại tới CompanyEntity)
        /// </summary>
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// Ngày nghỉ lễ
        /// </summary>
        public DateOnly HolidayDate { get; set; }
        /// <summary>
        /// Ngày nghỉ lễ có lặp lại hàng năm hay không
        /// </summary>
        public bool IsRecurringYearly { get; set; }
        /// <summary>
        /// Ghi chú thêm về ngày nghỉ lễ
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Trạng thái hoạt động của ngày nghỉ lễ (true: hoạt động, false: không hoạt động)
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Navigation property tới công ty sở hữu ngày nghỉ lễ này (CompanyEntity)
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }


    }
}
