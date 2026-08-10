using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Contract
{
    /// <summary>
    /// Mẫu Loại hợp đồng lao động (Ví dụ: Thử việc, Xác định thời hạn 12 tháng, Không xác định thời hạn, Thời vụ,...).
    /// Dùng làm danh mục dùng chung, mỗi <see cref="ContractEntity"/> cụ thể sẽ tham chiếu tới một mẫu tại đây.
    /// </summary>
    public class ContractTypeEntity : BaseEntity
    {

        /// <summary>
        /// Mã loại hợp đồng (duy nhất, dùng để nhận diện, ví dụ "HD-THUVIEC")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên loại hợp đồng (Require)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả loại hợp đồng
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty sở hữu mẫu loại hợp đồng này (null nếu dùng chung cho toàn hệ thống)
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Có phải loại hợp đồng thử việc hay không (Ảnh hưởng tới quy tắc lương thử việc, thời gian thử việc tối đa)
        /// </summary>
        public bool IsProbation { get; set; }

        /// <summary>
        /// Có phải hợp đồng không xác định thời hạn hay không (true: EndDate của hợp đồng luôn để trống)
        /// </summary>
        public bool IsUnlimited { get; set; }

        /// <summary>
        /// Thời hạn mặc định của loại hợp đồng này (đơn vị: tháng), dùng để gợi ý tính EndDate khi tạo hợp đồng mới
        /// </summary>
        public int? DefaultDurationMonths { get; set; }

        /// <summary>
        /// Số lần được phép ký loại hợp đồng này tối đa theo quy định (Ví dụ: hợp đồng xác định thời hạn tối đa ký 2 lần)
        /// </summary>
        public int? MaxRenewalTimes { get; set; }

        /// <summary>
        /// Số ngày cần thông báo/đánh giá trước khi hợp đồng loại này hết hạn (dùng để cảnh báo gia hạn)
        /// </summary>
        public int? NotifyBeforeExpiryDays { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt (true: đang sử dụng, false: vô hiệu – không xóa dữ liệu)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị trên danh sách chọn loại hợp đồng
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Danh sách hợp đồng cụ thể được tạo từ loại hợp đồng này
        /// </summary>
        public List<ContractEntity> Contracts { get; set; } = [];

        /// <summary>
        /// Navigation property tới công ty sở hữu mẫu loại hợp đồng này
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }

    }
}