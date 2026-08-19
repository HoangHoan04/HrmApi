using System;
using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Asset
{
    /// <summary>
    /// Sổ theo dõi lịch sử bàn giao tài sản cho nhân viên
    /// Mỗi khi ISSUE Complete -> Mở 1 lượt bàn giao (ReturnedAt = null)
    /// Khi RETURN / TRANSFER Complete -> Đóng lượt bàn giao (ReturnedAt != null)
    /// </summary>
    public class AssetAssignmentEntity : BaseEntity
    {
        public Guid AssetId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }

        public DateTime IssuedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }

        public Guid? IssuedTicketId { get; set; }
        public Guid? ReturnedTicketId { get; set; }

        public string? ConditionOnIssue { get; set; }
        public string? ConditionOnReturn { get; set; }
        public string? Note { get; set; }

        public AssetEntity? Asset { get; set; }
        public EmployeeEntity? Employee { get; set; }
        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public AssetTicketEntity? IssuedTicket { get; set; }
        public AssetTicketEntity? ReturnedTicket { get; set; }
    }
}
