using HrmApi.Domain.Enums;

namespace HrmApi.Application.Features.Recruitment
{
    internal static class HiringSourceCatalog
    {
        public sealed record SeedItem(
            string Code,
            string Name,
            string ChannelType,
            int DisplayOrder,
            string? Description = null);

        public static readonly SeedItem[] SystemDefaults =
        [
            new("REFERRAL", "Giới thiệu nội bộ", HiringSourceChannel.Referral, 10,
                "Nhân viên giới thiệu ứng viên (ưu tiên cao nhất)."),
            new("HR_EMAIL", "Mail HR", HiringSourceChannel.Email, 20,
                "Ứng viên gửi CV / liên hệ qua hộp thư HR. Cấu hình ContactEmail trên nguồn này."),
            new("CAREERS_SITE", "Website tuyển dụng", HiringSourceChannel.CareersSite, 30,
                "Form / trang careers trên website công ty."),
            new("FACEBOOK", "Facebook", HiringSourceChannel.Social, 40,
                "Fanpage / nhóm tuyển dụng Facebook."),
            new("TOPCV", "TopCV", HiringSourceChannel.JobBoard, 50,
                "Ứng viên từ TopCV (nhập tay / import; tích hợp API giai đoạn sau)."),
            new("ITVIEC", "ITviec", HiringSourceChannel.JobBoard, 60,
                "Ứng viên từ ITviec (nhập tay / import; tích hợp API giai đoạn sau)."),
            new("LINKEDIN", "LinkedIn", HiringSourceChannel.JobBoard, 70,
                "Ứng viên từ LinkedIn."),
            new("JOBBOARD", "Trang tuyển dụng khác", HiringSourceChannel.JobBoard, 80,
                "Job board / trang đăng tin khác (VietnamWorks, CareerBuilder...)."),
            new("HEADHUNTER", "Headhunter / Agency", HiringSourceChannel.Agency, 90,
                "Đối tác headhunter hoặc agency."),
            new("WALK_IN", "Ứng tuyển trực tiếp", HiringSourceChannel.WalkIn, 100,
                "Ứng viên đến nộp hồ sơ / walk-in."),
            new("OTHER", "Khác", HiringSourceChannel.Other, 999,
                "Nguồn không thuộc các kênh trên."),
        ];
    }
}
