using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.Common.Interfaces
{
    public class WorkWindowResult
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public int BreakMinutes { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsOvernight { get; set; }
        public string Source { get; set; } = AttendanceScheduleSource.WorkPattern;
        public bool IsScheduledWorkDay { get; set; } = true;
    }

    public class AttendanceStandardResult
    {
        public Guid? StandardId { get; set; }
        public int AllowedRadiusMeters { get; set; } = 200;
        public int LateGraceMinutes { get; set; }
        public int EarlyLeaveGraceMinutes { get; set; }
        public TimeSpan NightStartTime { get; set; } = new(22, 0, 0);
        public TimeSpan NightEndTime { get; set; } = new(6, 0, 0);
    }

    public interface IAttendanceRuleService
    {
        Task<EmployeeEntity> ResolveEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

        Task<WorkWindowResult> ResolveWorkWindowAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default);

        Task<AttendanceStandardResult> ResolveStandardAsync(Guid? branchId, Guid? companyId, CancellationToken cancellationToken = default);

        double ValidateGeofence(double? branchLat, double? branchLng, double punchLat, double punchLng, int allowedRadiusMeters);

        Task<bool> HasApprovedLeaveAsync(Guid employeeId, DateOnly workDate, CancellationToken cancellationToken = default);

        void ComputeStatus(TimekeepingEntity record, WorkWindowResult window, AttendanceStandardResult standard);

        Task FinalizeOtAndNightAsync(
            TimekeepingEntity record,
            WorkWindowResult window,
            AttendanceStandardResult standard,
            CancellationToken cancellationToken = default);

        Task<TimekeepingEntity> GetOrCreateTodayRecordAsync(EmployeeEntity employee, DateOnly workDate, CancellationToken cancellationToken = default);
    }
}
