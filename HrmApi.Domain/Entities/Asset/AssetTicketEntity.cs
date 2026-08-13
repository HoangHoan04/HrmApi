using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Asset
{
    public class AssetTicketEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public Guid AssetId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid CompanyId { get; set; }
        public string TicketType { get; set; } = AssetTicketType.Issue;
        public string Status { get; set; } = AssetTicketStatus.Draft;
        public DateTime TicketAt { get; set; }
        public string? Note { get; set; }

        public AssetEntity? Asset { get; set; }
        public EmployeeEntity? Employee { get; set; }
        public CompanyEntity? Company { get; set; }
    }
}
