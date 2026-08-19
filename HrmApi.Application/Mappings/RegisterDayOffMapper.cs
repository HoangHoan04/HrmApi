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
            string? approverName = null,
            string? requestedApproverName = null)
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
                FromDate = entity.FromDate,
                ToDate = entity.ToDate,
                Session = entity.Session,
                TotalDays = entity.TotalDays,
                Reason = entity.Reason,
                AttachmentUrl = entity.AttachmentUrl,
                Status = entity.Status,
                RequestedApproverId = entity.RequestedApproverId,
                RequestedApproverName = requestedApproverName,
                ApproverId = entity.ApproverId,
                ApproverName = approverName,
                ApprovedAt = entity.ApprovedAt,
                ApproverNote = entity.ApproverNote,
                CancelReason = entity.CancelReason,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static object ToLogObject(RegisterDayOffEntity entity)
        {
            return new
            {
                entity.Id,
                entity.EmployeeId,
                entity.DayOffConfigId,
                entity.FromDate,
                entity.ToDate,
                entity.Session,
                entity.TotalDays,
                entity.Status,
                entity.RequestedApproverId,
                entity.ApproverId,
                entity.ApproverNote,
                entity.CancelReason
            };
        }
    }
}
