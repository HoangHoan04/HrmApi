using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Asset
{
    public class AssetTypeEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public CompanyEntity? Company { get; set; }
        public List<AssetEntity> Assets { get; set; } = [];
    }
}
