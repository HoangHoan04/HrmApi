using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Asset
{
    public class AssetEntity : BaseEntity
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Tên tài sản
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Mã loại tài sản
        /// </summary>
        public Guid AssetTypeId { get; set; }
        /// <summary>
        /// Mã công ty
        /// </summary>
        public Guid CompanyId { get; set; }
        /// <summary>
        /// Mã chi nhánh
        /// </summary>
        public Guid? BranchId { get; set; }
        /// <summary>
        /// Số Seri tài sản
        /// </summary>
        public string? SerialNumber { get; set; }
        /// <summary>
        /// Ngày mua
        /// </summary>
        public DateOnly? PurchaseDate { get; set; }
        /// <summary>
        /// Giá mua
        /// </summary>
        public decimal? PurchaseCost { get; set; }
        /// <summary>
        /// Ngày hết hạn bảo hành
        /// </summary>
        public DateOnly? WarrantyExpiryDate { get; set; }
        /// <summary>
        /// Nhà phân phối
        /// </summary>
        public string? Vendor { get; set; }
        /// <summary>
        /// Kiểu mẫu
        /// </summary>
        public string? Model { get; set; }
        /// <summary>
        /// Địa điểm
        /// </summary>
        public string? Location { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public string Status { get; set; } = AssetStatus.Available;
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }
        /// <summary>
        /// Liên kết
        /// </summary>
        public AssetTypeEntity? AssetType { get; set; }
        public CompanyEntity? Company { get; set; }
        public BranchEntity? Branch { get; set; }
        public List<AssetTicketEntity> Tickets { get; set; } = [];
        public List<AssetAssignmentEntity> Assignments { get; set; } = [];
    }
}
