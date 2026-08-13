using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>
    /// Cấu hình lưu trữ soft-delete (thường chỉ có một bản ghi active).
    /// </summary>
    public class SystemRetentionConfigEntity : BaseEntity
    {
        public int SoftDeleteRetentionDays { get; set; } = 365;
        public bool IsPurgeEnabled { get; set; }
        public string? Note { get; set; }
    }
}
