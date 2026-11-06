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
        /// <summary>
        /// Mã tài sản
        /// </summary>
        public Guid AssetId { get; set; }
        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public Guid EmployeeId { get; set; }
        /// <summary>
        /// Mã công ty
        /// </summary>
        public Guid CompanyId { get; set; }
        /// <summary>
        /// Mã chi nhánh
        /// </summary>
        public Guid? BranchId { get; set; }
        /// <summary>
        /// Ngày nhận
        /// </summary>
        public DateTime IssuedAt { get; set; }
        /// <summary>
        /// Ngày trả
        /// </summary>
        public DateTime? ReturnedAt { get; set; }
        /// <summary>
        /// Phiếu nhận
        /// </summary>
        public Guid? IssuedTicketId { get; set; }
        /// <summary>
        /// Phiếu trả
        /// </summary>
        public Guid? ReturnedTicketId { get; set; }
        /// <summary>
        /// Điều kiện nhận
        /// </summary>
        public string? ConditionOnIssue { get; set; }
        /// <summary>
        /// Điều kiện trả
        /// </summary>
        public string? ConditionOnReturn { get; set; }
        public string? Note { get; set; }
        /// <summary>
        /// Liên kết
        /// </summary>
        public AssetEntity? Asset { get; set; }
        public EmployeeEntity? Employee { get; set; }
        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public AssetTicketEntity? IssuedTicket { get; set; }
        public AssetTicketEntity? ReturnedTicket { get; set; }
    }
}
