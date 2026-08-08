namespace HrmApi.Application.Common.Models
{
    public class UploadFileInput
    {
        public byte[] Buffer { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Length { get; set; }
    }
}
