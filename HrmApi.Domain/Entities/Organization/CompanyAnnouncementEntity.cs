using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Thông báo nội bộ theo công ty (mobile feed).
    /// </summary>
    public class CompanyAnnouncementEntity : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
