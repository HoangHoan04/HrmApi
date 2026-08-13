using HrmApi.Domain.Common;
using HrmApi.Domain.Enums;

namespace HrmApi.Domain.Entities.Recruitment
{
    /// <summary>
    /// Master nguồn nhận ứng viên (giới thiệu nội bộ, mail HR, job board...).
    /// DisplayOrder càng nhỏ càng ưu tiên khi chọn / báo cáo.
    /// </summary>
    public class HiringSourceEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>REFERRAL | EMAIL | CAREERS_SITE | JOBBOARD | SOCIAL | AGENCY | WALK_IN | OTHER</summary>
        public string ChannelType { get; set; } = HiringSourceChannel.Other;

        /// <summary>Email nhận CV (áp dụng kênh Mail HR).</summary>
        public string? ContactEmail { get; set; }

        public int DisplayOrder { get; set; }

        /// <summary>Nguồn hệ thống — không xóa; chỉ tắt / chỉnh tên, email, thứ tự.</summary>
        public bool IsSystem { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CandidateEntity> Candidates { get; set; } = [];
    }
}
