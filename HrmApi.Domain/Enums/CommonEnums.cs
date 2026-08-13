namespace HrmApi.Domain.Enums
{
    /// <summary>Loại tài khoản / vai trò hệ thống gắn User.Type.</summary>
    public static class UserType
    {
        public const string Admin = "ADMIN";
        public const string Hr = "HR";
        public const string Manager = "MANAGER";
        public const string Employee = "EMPLOYEE";
    }

    /// <summary>Phạm vi dữ liệu gắn RolePermission.</summary>
    public static class DataScope
    {
        public const string All = "ALL";
        public const string Company = "COMPANY";
        public const string Branch = "BRANCH";
        public const string Department = "DEPARTMENT";
        public const string Part = "PART";
        public const string Own = "OWN";
    }

    /// <summary>Nguồn resolve ca làm việc.</summary>
    public static class AttendanceScheduleSource
    {
        public const string DayOverride = "DAY_OVERRIDE";
        public const string WorkPattern = "WORK_PATTERN";
        public const string Position = "POSITION";
    }

    /// <summary>Loại punch chấm công (máy / CSV / mobile).</summary>
    public static class PunchType
    {
        public const string In = "IN";
        public const string Out = "OUT";
        public const string CheckIn = "CHECKIN";
        public const string CheckOut = "CHECKOUT";
    }

    /// <summary>Sự kiện lịch nghỉ (Mobile/Admin calendar).</summary>
    public static class LeaveCalendarEventType
    {
        public const string Leave = "LEAVE";
        public const string Holiday = "HOLIDAY";
    }

    /// <summary>Loại sự kiện timeline hồ sơ nhân viên.</summary>
    public static class EmployeeTimelineKind
    {
        public const string Profile = "PROFILE";
        public const string Salary = "SALARY";
        public const string Transfer = "TRANSFER";
        public const string File = "FILE";
        public const string Audit = "AUDIT";
    }
}
