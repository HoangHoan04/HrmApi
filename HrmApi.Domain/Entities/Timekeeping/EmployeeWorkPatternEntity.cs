using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Ca / lịch làm việc mặc định của nhân viên (pattern tuần).
    /// Full-time VP: gắn 1 lần, không cần tạo lịch từng ngày.
    /// Lịch ngày (WorkScheduledEmployee) chỉ dùng để ghi đè / roster / OT cuối tuần.
    /// </summary>
    public class EmployeeWorkPatternEntity : BaseEntity
    {
        public Guid EmployeeId { get; set; }

        public Guid ShiftMasterId { get; set; }

        public WorkPatternType PatternType { get; set; } = WorkPatternType.FIXED_WEEKLY;

        public bool WorkOnMonday { get; set; } = true;
        public bool WorkOnTuesday { get; set; } = true;
        public bool WorkOnWednesday { get; set; } = true;
        public bool WorkOnThursday { get; set; } = true;
        public bool WorkOnFriday { get; set; } = true;
        public bool WorkOnSaturday { get; set; }
        public bool WorkOnSunday { get; set; }

        /// <summary>Ngày bắt đầu hiệu lực (inclusive).</summary>
        public DateOnly EffectiveFrom { get; set; }

        /// <summary>Ngày kết thúc hiệu lực (inclusive). Null = vô thời hạn.</summary>
        public DateOnly? EffectiveTo { get; set; }

        public Guid? BranchId { get; set; }

        public string? Note { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual EmployeeEntity? Employee { get; set; }
        public virtual ShiftMasterEntity? ShiftMaster { get; set; }
        public virtual BranchEntity? Branch { get; set; }
    }
}
