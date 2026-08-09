using System;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Timekeeping;

namespace HrmApi.Application.Common.Interfaces
{
    public class WorkWindowResult
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsOvernight { get; set; }
    }

    public class AttendanceStandardResult
    {
        public Guid? StandardId { get; set; }
        public int AllowedRadiusMeters { get; set; } = 200;
        public int LateGraceMinutes { get; set; }
        public int EarlyLeaveGraceMinutes { get; set; }
    }

    public interface IAttendanceRuleService
    {
        Task<EmployeeEntity> ResolveEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

        Task<WorkWindowResult> ResolveWorkWindowAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default);

        Task<AttendanceStandardResult> ResolveStandardAsync(Guid? branchId, Guid? companyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate geofence; trả về khoảng cách (mét). Ném InvalidOperationException nếu không hợp lệ.
        /// </summary>
        double ValidateGeofence(double? branchLat, double? branchLng, double punchLat, double punchLng, int allowedRadiusMeters);

        Task<bool> HasApprovedLeaveAsync(Guid employeeId, DateOnly workDate, CancellationToken cancellationToken = default);

        void ComputeStatus(TimekeepingEntity record, WorkWindowResult window, AttendanceStandardResult standard);

        Task<TimekeepingEntity> GetOrCreateTodayRecordAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default);
    }
}
