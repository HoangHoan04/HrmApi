namespace HrmApi.Application.DTOs.Upload
{
    public class UploadFileResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string? Checksum { get; set; }
        public string? UrlResize { get; set; }
        public string? MobileFileUrl { get; set; }
        public string? MobileFileName { get; set; }
        public bool? IsHD { get; set; }
        public long? Size { get; set; }
        public bool? IsSkipped { get; set; }
    }
}
