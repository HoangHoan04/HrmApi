using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Timekeeping
{
    /// <summary>
    /// Tổng hợp công theo tháng / nhân viên
    /// </summary>
    public class TimekeepingSummaryEntity : BaseEntity
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
        /// Năm tổng hợp công
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Tháng tổng hợp công
        /// </summary>
        public int Month { get; set; }
        /// <summary>
        /// Số ngày đi làm đúng giờ (không đi muộn, không về sớm)
        /// </summary>
        public int OnTimeDays { get; set; }
        /// <summary>
        /// Số ngày đi muộn (Late)
        /// </summary>
        public int LateDays { get; set; }
        /// <summary>
        /// Số ngày về sớm (Early)
        /// </summary>
        public int EarlyDays { get; set; }
        /// <summary>
        /// Số ngày nghỉ phép (Leave)
        /// </summary>
        public int LeaveDays { get; set; }
        /// <summary>
        /// Số ngày nghỉ không phép (Absent)
        /// </summary>
        public int AbsentDays { get; set; }
        /// <summary>
        /// Số ngày công chưa hoàn thành (Incomplete)
        /// </summary>
        public int IncompleteDays { get; set; }
        /// <summary>
        /// Số ngày công thực tế (Working Days) = OnTimeDays + LateDays + EarlyDays + LeaveDays + AbsentDays + IncompleteDays
        /// </summary>
        public int WorkingDays { get; set; }
        /// <summary>
        /// Tổng số phút làm việc thực tế trong tháng (Total Worked Minutes)
        /// </summary>
        public int TotalWorkedMinutes { get; set; }
        /// <summary>
        /// Tổng số phút đi muộn trong tháng (Total Late Minutes)
        /// </summary>
        public int TotalLateMinutes { get; set; }
        /// <summary>
        /// Tổng số phút về sớm trong tháng (Total Early Minutes)
        /// </summary>
        public int TotalEarlyMinutes { get; set; }

        /// <summary>
        /// Tổng phút OT trong tháng
        /// </summary>
        public int TotalOtMinutes { get; set; }

        /// <summary>
        /// Tổng phút làm đêm trong tháng
        /// </summary>
        public int TotalNightMinutes { get; set; }

        /// <summary>
        /// Navigation property tới nhân viên sở hữu tổng hợp công này (EmployeeEntity)
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }
        /// <summary>
        /// Navigation property tới công ty sở hữu tổng hợp công này (CompanyEntity)
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh sở hữu tổng hợp công này (BranchEntity)
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }


    }
}
