namespace HrmApi.Application.DTOs.WorkSchedule
{
    public class WorkScheduleDto : BaseDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public string? ShiftMasterName { get; set; }
        public string? ShiftMasterCode { get; set; }
        public DateOnly WorkDate { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Note { get; set; }
    }
}
