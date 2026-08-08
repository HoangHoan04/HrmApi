namespace HrmApi.Infrastructure.Services.Upload
{
    internal static class UploadMimeHelper
    {
        internal static readonly HashSet<string> ImageMimes =
        [
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/svg+xml",
            "image/avif",
        ];

        internal static readonly HashSet<string> AudioMimes =
        [
            "audio/mpeg",
            "audio/mp3",
            "audio/wav",
            "audio/ogg",
            "audio/webm",
            "audio/x-m4a",
            "audio/aac",
        ];

        internal static readonly HashSet<string> DocumentMimes =
        [
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "text/plain",
            "text/csv",
        ];

        internal static readonly HashSet<string> ImageExtensions =
        [
            "jpg", "jpeg", "jpe", "png", "webp", "svg", "bmp", "tiff", "tif",
        ];

        internal static string DetectCategory(string mimetype)
        {
            if (ImageMimes.Contains(mimetype)) return "image";
            if (AudioMimes.Contains(mimetype)) return "audio";
            if (DocumentMimes.Contains(mimetype)) return "document";
            return "other";
        }

        internal static string GetExtension(string mimetype)
        {
            return mimetype switch
            {
                "image/jpeg" => "jpg",
                "image/png" => "png",
                "image/gif" => "gif",
                "image/webp" => "webp",
                "image/svg+xml" => "svg",
                "image/avif" => "avif",
                "audio/mpeg" => "mp3",
                "audio/mp3" => "mp3",
                "audio/wav" => "wav",
                "audio/ogg" => "ogg",
                "audio/webm" => "webm",
                "audio/x-m4a" => "m4a",
                "audio/aac" => "aac",
                "application/pdf" => "pdf",
                "application/msword" => "doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
                "application/vnd.ms-powerpoint" => "ppt",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" => "pptx",
                "text/plain" => "txt",
                "text/csv" => "csv",
                _ => "bin",
            };
        }

        internal static string? ExtensionFromContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return null;
            }

            var normalizedType = contentType.Split(';')[0].Trim();
            return normalizedType switch
            {
                "image/avif" => ".avif",
                "image/bmp" => ".bmp",
                "image/gif" => ".gif",
                "image/heic" => ".heic",
                "image/heif" => ".heif",
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/svg+xml" => ".svg",
                "image/tiff" => ".tiff",
                "image/webp" => ".webp",
                _ => null,
            };
        }

        internal static string? ExtensionFromUrl(string imageUrl)
        {
            try
            {
                var pathname = new Uri(imageUrl).AbsolutePath.ToLowerInvariant();
                var match = System.Text.RegularExpressions.Regex.Match(pathname, @"\.[a-z0-9]+$");
                if (!match.Success)
                {
                    return null;
                }

                var extension = match.Value;
                HashSet<string> valid = [".avif", ".bmp", ".gif", ".heic", ".heif", ".jpeg", ".jpg", ".png", ".svg", ".tif", ".tiff", ".webp"];
                return valid.Contains(extension) ? extension : null;
            }
            catch
            {
                return null;
            }
        }

        internal static string ResolveImageExtension(string contentType, string imageUrl)
        {
            return ExtensionFromContentType(contentType)
                ?? ExtensionFromUrl(imageUrl)
                ?? ".jpg";
        }

        internal static string NormalizeZipTitle(string fileTitle)
        {
            var sanitized = System.Text.RegularExpressions.Regex.Replace(fileTitle.Trim(), @"[^a-zA-Z0-9\-_]+", "-");
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"-+", "-").Trim('-');
            return string.IsNullOrWhiteSpace(sanitized) ? "images" : sanitized;
        }
    }
}
