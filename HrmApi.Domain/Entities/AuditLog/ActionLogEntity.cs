using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.AuditLog
{
    public class ActionLogEntity : BaseEntity
    {
        /// <summary>
        /// Id người tạo
        /// </summary>
        public Guid CreatedById { get; set; }
        /// <summary>
        /// Code người tạo
        /// </summary>
        public string CreatedByCode { get; set; } = string.Empty;
        /// <summary>
        /// Tên người tạo
        /// </summary>
        public string CreatedByName { get; set; } = string.Empty;
        /// <summary>
        /// Note người tạo
        /// </summary>
        public string? CreatedNote { get; set; }
        /// <summary>
        /// Loại hành động (Create, Update, Delete, Login, Logout, ...)
        /// </summary>
        public string? ActionType { get; set; }
        /// <summary>
        /// Id của entity bị tác động
        /// </summary>
        public Guid? EntityId { get; set; }
        /// <summary>
        /// Tên của entity bị tác động
        /// </summary>
        public string? EntityName { get; set; }
        /// <summary>
        /// Giá trị cũ của entity bị tác động (nếu có)
        /// </summary>
        public string? OldValue { get; set; }
        /// <summary>
        /// Giá trị mới của entity bị tác động (nếu có)
        /// </summary>
        public string? NewValue { get; set; }
        /// <summary>
        /// Địa chỉ IP của người thực hiện hành động
        /// </summary>
        public string? IpAddress { get; set; }
        /// <summary>
        /// User agent của người thực hiện hành động
        /// </summary>
        public string? UserAgent { get; set; }
        /// <summary>
        /// Địa điểm thực hiện hành động (nếu có)
        /// </summary>
        public string? Location { get; set; }


    }
}
