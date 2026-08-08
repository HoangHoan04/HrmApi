namespace HrmApi.Application.DTOs.Upload
{
    public class DownloadImagesAsZipResultDto
    {
        public byte[] Buffer { get; set; } = Array.Empty<byte>();
        public string Filename { get; set; } = "images.zip";
        public int TotalUrls { get; set; }
        public int SuccessCount { get; set; }
        public int SkippedCount { get; set; }
    }
}
