using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Mẫu ca làm việc
    /// </summary>
    public class ShiftMasterEntity : BaseEntity
    {

        /// <summary>
        /// Mã ca
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên ca
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Giờ bắt đầu ca
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc ca
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Số phút nghỉ giữa ca
        /// </summary>
        public int BreakMinutes { get; set; }

        /// <summary>
        /// Số phút làm việc chuẩn
        /// </summary>
        public int WorkingMinutes { get; set; }

        /// <summary>
        /// Ca qua đêm (end &lt; start)
        /// </summary>
        public bool IsOvernight { get; set; }

        /// <summary>
        /// Đang kích hoạt
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property tới công ty sở hữu ca làm việc (CompanyEntity)
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới danh sách các ca làm việc thuộc mẫu ca này (ShiftEntity)
        /// </summary>
        public virtual ICollection<ShiftEntity> ShiftEntities { get; set; } = [];
        /// <summary>
        /// Navigation property tới danh sách các bản ghi chấm công thuộc mẫu ca này (TimekeepingEntity)
        /// </summary>
        public virtual ICollection<TimekeepingEntity> TimekeepingEntities { get; set; } = [];
        /// <summary>
        /// Navigation property tới danh sách các nhân viên được phân ca thuộc mẫu ca này (WorkScheduledEmployeeEntity)
        /// </summary>
        public virtual ICollection<WorkScheduledEmployeeEntity> WorkScheduledEmployeeEntities { get; set; } = [];


    }
}
