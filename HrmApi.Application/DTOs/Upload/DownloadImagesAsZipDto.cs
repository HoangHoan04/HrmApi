namespace HrmApi.Application.DTOs.Upload
{
    public class DownloadImagesAsZipDto
    {
        public List<string> ImageUrls { get; set; } = [];
        public string FileTitle { get; set; } = "images";
    }
}
