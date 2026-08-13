using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Bản ghi chấm công theo ngày / nhân viên
    /// </summary>
    public class TimekeepingEntity : BaseEntity
    {
        /// <summary>
        /// Id nhân viên (khóa ngoại tới EmployeeEntity)
        /// </summary>
        public Guid EmployeeId { get; set; }
        /// <summary>
        /// Id công ty (khóa ngoại tới CompanyEntity)
        /// </summary>
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// Id chi nhánh (khóa ngoại tới BranchEntity)
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Ngày công
        /// </summary>
        public DateOnly WorkDate { get; set; }
        /// <summary>
        /// Id ca làm việc (khóa ngoại tới ShiftEntity)
        /// </summary>
        public Guid? ShiftId { get; set; }
        /// <summary>
        /// Id ca làm việc master (khóa ngoại tới ShiftMasterEntity)
        /// </summary>
        public Guid? ShiftMasterId { get; set; }
        /// <summary>
        /// Thời gian check-in thực tế (nếu có)
        /// </summary>
        public DateTime? CheckInAt { get; set; }
        /// <summary>
        /// Thời gian check-out thực tế (nếu có)
        /// </summary>
        public DateTime? CheckOutAt { get; set; }
        /// <summary>
        /// Vĩ độ vị trí check-in (nếu có)
        /// </summary>
        public double? CheckInLatitude { get; set; }
        /// <summary>
        /// Kinh độ vị trí check-in (nếu có)
        /// </summary>
        public double? CheckInLongitude { get; set; }
        /// <summary>
        /// Vĩ độ vị trí check-out (nếu có)
        /// </summary>
        public double? CheckOutLatitude { get; set; }
        /// <summary>
        /// Kinh độ vị trí check-out (nếu có)
        /// </summary>
        public double? CheckOutLongitude { get; set; }
        /// <summary>
        /// Khoảng cách từ vị trí check-in tới vị trí định vị ca làm việc (nếu có)
        /// </summary>
        public double? CheckInDistanceM { get; set; }
        /// <summary>
        /// Khoảng cách từ vị trí check-out tới vị trí định vị ca làm việc (nếu có)
        /// </summary>
        public double? CheckOutDistanceM { get; set; }

        /// <summary>
        /// Trạng thái chấm công (INCOMPLETE, COMPLETE, LATE, EARLY, ABSENT, LEAVE,...)
        /// </summary>
        public HrmApi.Domain.Enums.AttendanceStatus Status { get; set; } = HrmApi.Domain.Enums.AttendanceStatus.INCOMPLETE;
        /// <summary>
        /// Số phút đi muộn (nếu có)
        /// </summary>
        public int LateMinutes { get; set; }

        /// <summary>
        /// Số phút về sớm (nếu có)
        /// </summary>
        public int EarlyMinutes { get; set; }
        /// <summary>
        /// Số phút làm việc thực tế (nếu có)
        /// </summary>
        public int WorkedMinutes { get; set; }

        /// <summary>
        /// Số phút OT trong ngày (theo đơn duyệt ∩ giờ làm thực tế)
        /// </summary>
        public int OtMinutes { get; set; }

        /// <summary>
        /// Số phút làm đêm (giao với khung NightStart–NightEnd của chuẩn CC)
        /// </summary>
        public int NightMinutes { get; set; }

        /// <summary>
        /// Ghi chú thêm về bản ghi chấm công
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Đã được Admin chỉnh tay
        /// </summary>
        public bool IsManualAdjusted { get; set; }
        /// <summary>
        /// Navigation property tới nhân viên sở hữu bản ghi chấm công này (EmployeeEntity)
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }
        /// <summary>
        /// Navigation property tới công ty sở hữu bản ghi chấm công này (CompanyEntity)
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh sở hữu bản ghi chấm công này (BranchEntity)
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }
        /// <summary>
        /// Navigation property tới ca làm việc sở hữu bản ghi chấm công này (ShiftEntity)
        /// </summary>
        public virtual ShiftEntity? Shift { get; set; }
        /// <summary>
        /// Navigation property tới ca làm việc master sở hữu bản ghi chấm công này (ShiftMasterEntity)
        /// </summary>
        public virtual ShiftMasterEntity? ShiftMaster { get; set; }


    }
}
