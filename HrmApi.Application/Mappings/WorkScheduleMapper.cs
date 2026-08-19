using HrmApi.Application.DTOs.WorkSchedule;
using HrmApi.Domain.Entities.Timekeeping;

namespace HrmApi.Application.Mappings
{
    internal class WorkScheduleMapper
    {
        public static WorkScheduleDto ToDto(
            WorkScheduledEmployeeEntity entity,
            string? employeeName = null,
            string? employeeCode = null,
            string? shiftMasterName = null,
            string? shiftMasterCode = null,
            string? branchName = null)
        {
            return new WorkScheduleDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName,
                EmployeeCode = employeeCode,
                ShiftId = entity.ShiftId,
                ShiftMasterId = entity.ShiftMasterId,
                ShiftMasterName = shiftMasterName,
                ShiftMasterCode = shiftMasterCode,
                WorkDate = entity.WorkDate,
                BranchId = entity.BranchId,
                BranchName = branchName,
                Note = entity.Note,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static void ApplyCommandFields(WorkScheduledEmployeeEntity entity, WorkScheduleCommandFields fields)
        {
            if (fields.EmployeeId != Guid.Empty)
            {
                entity.EmployeeId = fields.EmployeeId;
            }

            if (fields.ShiftId.HasValue)
            {
                entity.ShiftId = fields.ShiftId;
            }

            if (fields.ShiftMasterId.HasValue)
            {
                entity.ShiftMasterId = fields.ShiftMasterId;
            }

            if (fields.WorkDate != default)
            {
                entity.WorkDate = fields.WorkDate;
            }

            if (fields.BranchId.HasValue)
            {
                entity.BranchId = fields.BranchId;
            }

            if (fields.Note != null)
            {
                entity.Note = string.IsNullOrWhiteSpace(fields.Note) ? null : fields.Note.Trim();
            }
        }

        public static object ToLogObject(WorkScheduledEmployeeEntity entity)
        {
            return new
            {
                entity.Id,
                entity.EmployeeId,
                entity.ShiftId,
                entity.ShiftMasterId,
                entity.WorkDate,
                entity.BranchId,
                entity.Note
            };
        }
    }

    public class WorkScheduleCommandFields
    {
        public Guid EmployeeId { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public Guid? ShiftId { get; set; }
        public DateOnly WorkDate { get; set; }
        public Guid? BranchId { get; set; }
        public string? Note { get; set; }
    }
}
