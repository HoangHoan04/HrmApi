using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Settings
{
    /// <summary>Cấu hình Zalo Official Account (adapter tích hợp).</summary>
    public class ZaloOaConfigEntity : BaseEntity
    {
        public string OaId { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }
    }
}
