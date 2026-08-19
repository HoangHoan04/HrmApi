namespace HrmApi.Application.DTOs.Home
{
    public class HomeDashboardDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public HomeKpiDto Kpis { get; set; } = new();
        public HomePendingDto Pending { get; set; } = new();
        public HomeAttendanceTodayDto AttendanceToday { get; set; } = new();
        public HomeContractSnapshotDto Contracts { get; set; } = new();
        public List<HomeNamedCountDto> GenderBreakdown { get; set; } = [];
        public List<HomeNamedCountDto> DepartmentHeadcount { get; set; } = [];
        public List<HomeNamedCountDto> HeadcountByYear { get; set; } = [];
        public List<HomeNamedCountDto> LeaveStatusThisMonth { get; set; } = [];
        public List<HomeNamedCountDto> NewHiresByMonth { get; set; } = [];
    }

    public class HomeKpiDto
    {
        public int TotalEmployees { get; set; }
        public int FemaleEmployees { get; set; }
        public int MaleEmployees { get; set; }
        public int OtherGenderEmployees { get; set; }
        public int NewHiresThisMonth { get; set; }
        public int NewHiresLastMonth { get; set; }
        public decimal NewHiresChangePercent { get; set; }
        public int ActiveContracts { get; set; }
    }

    public class HomePendingDto
    {
        public int LeaveRequests { get; set; }
        public int AttendanceComplaints { get; set; }
        public int Transfers { get; set; }
        public int ReviewRenewals { get; set; }
        public int Total => LeaveRequests + AttendanceComplaints + Transfers + ReviewRenewals;
    }

    public class HomeAttendanceTodayDto
    {
        public int Present { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }
        public int OnLeave { get; set; }
        public int Incomplete { get; set; }
        public int TotalRecords { get; set; }
        public decimal AttendanceRatePercent { get; set; }
    }

    public class HomeContractSnapshotDto
    {
        public int Active { get; set; }
        public int PendingSign { get; set; }
        public int ExpiringIn30Days { get; set; }
        public int Expired { get; set; }
    }

    public class HomeNamedCountDto
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
