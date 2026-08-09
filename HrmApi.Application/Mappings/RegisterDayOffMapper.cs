using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Domain.Entities.Leave;

namespace HrmApi.Application.Mappings
{
    internal class RegisterDayOffMapper
    {
        public static RegisterDayOffDto ToDto(
            RegisterDayOffEntity entity,
            string? employeeName = null,
            string? employeeCode = null,
            string? branchName = null,
            string? dayOffConfigName = null,
            string? approverName = null)
        {
            return new RegisterDayOffDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName,
                EmployeeCode = employeeCode,
                CompanyId = entity.CompanyId,
                BranchId = entity.BranchId,
                BranchName = branchName,
                DayOffConfigId = entity.DayOffConfigId,
                DayOffConfigName = dayOffConfigName,
                DayOffType = entity.DayOffType,
                FromDate = entity.FromDate,
                ToDate = entity.ToDate,
                TotalDays = entity.TotalDays,
                Reason = entity.Reason,
                Status = entity.Status,
                ApproverId = entity.ApproverId,
                ApproverName = approverName,
                ApprovedAt = entity.ApprovedAt,
                ApproverNote = entity.ApproverNote,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static object ToLogObject(RegisterDayOffEntity entity) => new
        {
            entity.Id,
            entity.EmployeeId,
            entity.DayOffConfigId,
            entity.DayOffType,
            entity.FromDate,
            entity.ToDate,
            entity.TotalDays,
            entity.Status,
            entity.ApproverId,
            entity.ApproverNote
        };
    }
}
