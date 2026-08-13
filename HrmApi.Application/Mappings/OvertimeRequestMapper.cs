using HrmApi.Application.DTOs.OvertimeRequest;
using HrmApi.Domain.Entities.Timekeeping;

namespace HrmApi.Application.Mappings
{
    internal static class OvertimeRequestMapper
    {
        public static OvertimeRequestDto ToDto(
            OvertimeRequestEntity entity,
            string? employeeName = null,
            string? employeeCode = null,
            string? branchName = null,
            string? approverName = null)
        {
            return new OvertimeRequestDto
            {
                Id = entity.Id,
                Code = entity.Code,
                EmployeeId = entity.EmployeeId,
                EmployeeName = employeeName,
                EmployeeCode = employeeCode,
                CompanyId = entity.CompanyId,
                BranchId = entity.BranchId,
                BranchName = branchName,
                WorkDate = entity.WorkDate,
                FromTime = entity.FromTime,
                ToTime = entity.ToTime,
                RequestedMinutes = entity.RequestedMinutes,
                ApprovedMinutes = entity.ApprovedMinutes,
                OtType = entity.OtType,
                Reason = entity.Reason,
                AttachmentUrl = entity.AttachmentUrl,
                Status = entity.Status,
                ApproverId = entity.ApproverId,
                ApproverName = approverName,
                ReviewedAt = entity.ReviewedAt,
                ApproverNote = entity.ApproverNote,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Version = entity.Version,
            };
        }

        public static object ToLogObject(OvertimeRequestEntity entity) => new
        {
            entity.Id,
            entity.Code,
            entity.EmployeeId,
            entity.WorkDate,
            entity.FromTime,
            entity.ToTime,
            entity.RequestedMinutes,
            entity.ApprovedMinutes,
            entity.OtType,
            entity.Status,
            entity.ApproverId,
            entity.ApproverNote,
        };
    }
}
