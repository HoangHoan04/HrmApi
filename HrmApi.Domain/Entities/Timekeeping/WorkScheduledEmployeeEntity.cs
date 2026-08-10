using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Xếp lịch làm việc cho nhân viên theo ngày
    /// </summary>
    public class WorkScheduledEmployeeEntity : BaseEntity
    {

        /// <summary>
        /// Id nhân viên
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Id ca cụ thể (nếu có)
        /// </summary>
        public Guid? ShiftId { get; set; }

        /// <summary>
        /// Id mẫu ca (dùng khi không tạo Shift instance)
        /// </summary>
        public Guid? ShiftMasterId { get; set; }

        /// <summary>
        /// Ngày làm việc
        /// </summary>
        public DateOnly WorkDate { get; set; }

        /// <summary>
        /// Id chi nhánh làm việc
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên được xếp lịch
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }
        /// <summary>
        /// Navigation property tới ca làm việc cụ thể (ShiftEntity)
        /// </summary>
        public virtual ShiftEntity? Shift { get; set; }
        /// <summary>
        /// Navigation property tới mẫu ca (ShiftMasterEntity)
        /// </summary>
        public virtual ShiftMasterEntity? ShiftMaster { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh làm việc (BranchEntity)
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }


    }
}
