using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Ca làm việc theo ngày / chi nhánh (instance của ShiftMaster)
    /// </summary>
    public class ShiftEntity : BaseEntity
    {

        /// <summary>
        /// Id mẫu ca
        /// </summary>
        public Guid ShiftMasterId { get; set; }

        /// <summary>
        /// Id chi nhánh áp dụng
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Ngày làm việc (null nếu ca recurring theo master)
        /// </summary>
        public DateOnly? WorkDate { get; set; }

        /// <summary>
        /// Giờ bắt đầu (có thể override master)
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Navigation property tới mẫu ca (ShiftMasterEntity)
        /// </summary>
        public virtual ShiftMasterEntity? ShiftMaster { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh áp dụng (BranchEntity)
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }
        /// <summary>
        /// Navigation property tới các bản ghi chấm công liên quan (TimekeepingEntity)
        /// </summary>
        public virtual ICollection<TimekeepingEntity> TimekeepingEntities { get; set; } = [];
        /// <summary>
        /// Navigation property tới các bản ghi lịch làm việc liên quan (WorkScheduledEmployeeEntity)
        /// </summary>
        public virtual ICollection<WorkScheduledEmployeeEntity> WorkScheduledEmployeeEntities { get; set; } = [];


    }
}
