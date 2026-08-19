using HrmApi.Application.DTOs.AttendanceComplaint;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;

namespace HrmApi.Application.Mappings
{
    internal static class AttendanceComplaintMapper
    {
        public static AttendanceComplaintDto ToDto(
            AttendanceComplaintEntity entity,
            string? employeeName = null,
            string? employeeCode = null,
            string? branchName = null,
            string? approverName = null,
            DateTime? currentCheckInAt = null,
            DateTime? currentCheckOutAt = null,
            AttendanceStatus? currentStatus = null)
        {
            return new AttendanceComplaintDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = employeeCode,
                EmployeeName = employeeName,
                CompanyId = entity.CompanyId,
                BranchId = entity.BranchId,
                BranchName = branchName,
                WorkDate = entity.WorkDate,
                TimekeepingId = entity.TimekeepingId,
                ComplaintType = entity.ComplaintType,
                ComplaintTypeLabel = entity.ComplaintType.ToString(),
                RequestedCheckInTime = entity.RequestedCheckInTime,
                RequestedCheckOutTime = entity.RequestedCheckOutTime,
                Reason = entity.Reason,
                AttachmentUrl = entity.AttachmentUrl,
                Status = entity.Status,
                ApproverId = entity.ApproverId,
                ApproverName = approverName,
                ReviewedAt = entity.ReviewedAt,
                ApproverNote = entity.ApproverNote,
                CurrentCheckInAt = currentCheckInAt,
                CurrentCheckOutAt = currentCheckOutAt,
                CurrentStatus = currentStatus,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
                IsDeleted = entity.IsDeleted,
            };
        }

        public static object ToLogObject(AttendanceComplaintEntity entity)
        {
            return new
            {
                entity.Id,
                entity.EmployeeId,
                entity.WorkDate,
                entity.ComplaintType,
                entity.RequestedCheckInTime,
                entity.RequestedCheckOutTime,
                entity.Reason,
                entity.Status,
                entity.ApproverNote,
                entity.TimekeepingId
            };
        }
    }
}
