using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Asset
{
    public class AssetEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid AssetTypeId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? SerialNumber { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string Status { get; set; } = AssetStatus.Available;
        public string? Note { get; set; }

        public AssetTypeEntity? AssetType { get; set; }
        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public List<AssetTicketEntity> Tickets { get; set; } = [];
    }
}
