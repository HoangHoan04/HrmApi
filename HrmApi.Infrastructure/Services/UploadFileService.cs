using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Upload;
using HrmApi.Infrastructure.Services.Upload;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace HrmApi.Infrastructure.Services
{
    public class UploadFileService : IUploadFileService
    {
        private const string CatboxApiUrl = "https://catbox.moe/user/api.php";
        private const string NanoAlphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

        private readonly ILogger<UploadFileService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Cloudinary _cloudinary;
        private readonly string _catboxUserhash;
        private readonly IAmazonS3? _s3;
        private readonly string _s3Bucket;
        private readonly string _s3LinkUpload;

        public UploadFileService(
            IConfiguration configuration,
            ILogger<UploadFileService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            var cloudName = GetConfig(configuration, "UploadSettings:Cloudinary:CloudName", "CLOUDINARY_CLOUD_NAME");
            var apiKey = GetConfig(configuration, "UploadSettings:Cloudinary:ApiKey", "CLOUDINARY_API_KEY");
            var apiSecret = GetConfig(configuration, "UploadSettings:Cloudinary:ApiSecret", "CLOUDINARY_API_SECRET");

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException(
                    "Thiếu cấu hình Cloudinary (CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET)");
            }

            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
            _catboxUserhash = GetConfig(configuration, "UploadSettings:Catbox:UserHash", "CATBOX_USERHASH") ?? string.Empty;

            var s3AccessKeyId = GetConfig(configuration, "UploadSettings:S3:AccessKeyId", "AWS_S3_ACCESS_KEY_ID") ?? string.Empty;
            var s3SecretAccessKey = GetConfig(configuration, "UploadSettings:S3:SecretAccessKey", "AWS_S3_SECRET_ACCESS_KEY") ?? string.Empty;
            _s3Bucket = GetConfig(configuration, "UploadSettings:S3:BucketName", "AWS_S3_BUCKET_NAME") ?? string.Empty;
            _s3LinkUpload = GetConfig(configuration, "UploadSettings:S3:LinkUpload", "LINK_UPLOAD_S3") ?? string.Empty;
            var s3Region = GetConfig(configuration, "UploadSettings:S3:Region", "AWS_S3_REGION") ?? "ap-southeast-1";

            if (!string.IsNullOrWhiteSpace(s3AccessKeyId) && !string.IsNullOrWhiteSpace(s3SecretAccessKey))
            {
                var region = RegionEndpoint.GetBySystemName(s3Region);
                _s3 = new AmazonS3Client(s3AccessKeyId, s3SecretAccessKey, region);
            }
        }

        public Task<UploadFileResultDto> UploadSingleAsync(UploadFileInput file, CancellationToken cancellationToken = default)
        {
            ValidateFile(file);
            return UploadMimeHelper.DetectCategory(file.ContentType) switch
            {
                "image" => UploadImageAsync(file, cancellationToken: cancellationToken),
                "audio" => UploadAudioAsync(file, cancellationToken: cancellationToken),
                "document" => UploadDocumentAsync(file, cancellationToken: cancellationToken),
                _ => UploadDocumentAsync(file, cancellationToken: cancellationToken),
            };
        }

        public async Task<List<UploadFileResultDto>> UploadMultiAsync(IReadOnlyList<UploadFileInput> files, CancellationToken cancellationToken = default)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentException("Danh sách file trống");
            }

            var results = new List<UploadFileResultDto>(files.Count);
            foreach (var file in files)
            {
                results.Add(await UploadSingleAsync(file, cancellationToken));
            }

            return results;
        }

        public async Task<UploadFileResultDto> UploadImageAsync(
            UploadFileInput file,
            string? folder = null,
            CancellationToken cancellationToken = default)
        {
            ValidateFile(file);
            var result = await UploadToCloudinaryAsync(file, folder ?? "wio-images", cancellationToken);
            return new UploadFileResultDto
            {
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                Storage = "cloudinary",
                Checksum = result.Checksum,
            };
        }

        public async Task<UploadFileResultDto> UploadAudioAsync(
            UploadFileInput file,
            string? folder = null,
            CancellationToken cancellationToken = default)
        {
            ValidateFile(file);
            var result = await UploadToCloudinaryAsync(file, folder ?? "wio-audio", cancellationToken);
            return new UploadFileResultDto
            {
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                Storage = "cloudinary",
                Checksum = result.Checksum,
            };
        }

        public async Task<UploadFileResultDto> UploadDocumentAsync(
            UploadFileInput file,
            string? folder = null,
            CancellationToken cancellationToken = default)
        {
            ValidateFile(file);
            var result = await UploadToCloudinaryAsync(file, folder ?? "wio-documents", cancellationToken);
            return new UploadFileResultDto
            {
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                Storage = "cloudinary",
                Checksum = result.Checksum,
            };
        }

        public async Task<UploadFileResultDto> UploadCatboxAsync(UploadFileInput file, CancellationToken cancellationToken = default)
        {
            ValidateFile(file);
            var result = await UploadToCatboxAsync(file, cancellationToken);
            return new UploadFileResultDto
            {
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                Storage = "catbox",
                Checksum = result.Checksum,
            };
        }

        public async Task<UploadFileResultDto> UploadToCatboxFromUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL is required");
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("urlupload"), "reqtype");
            if (!string.IsNullOrWhiteSpace(_catboxUserhash))
            {
                content.Add(new StringContent(_catboxUserhash), "userhash");
            }

            content.Add(new StringContent(url), "url");

            var client = _httpClientFactory.CreateClient(nameof(UploadFileService));
            using var response = await client.PostAsync(CatboxApiUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Catbox API returned status {(int)response.StatusCode}");
            }

            var fileUrl = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            var fileName = url.Split('/').LastOrDefault() ?? $"{GenerateFileId()}.bin";
            return new UploadFileResultDto
            {
                FileName = fileName,
                FileUrl = fileUrl,
                Storage = "catbox",
            };
        }

        public async Task<UploadFileResultDto> UploadS3Async(
            UploadFileInput file,
            string? folder = null,
            CancellationToken cancellationToken = default)
        {
            ValidateFile(file);
            EnsureS3Configured();

            var ext = UploadMimeHelper.GetExtension(file.ContentType);
            var fileId = GenerateFileId();
            var fileName = $"{fileId}.{ext}";
            var key = string.IsNullOrWhiteSpace(folder) ? fileName : $"{folder.Trim('/')}/{fileName}";
            var checksum = ComputeChecksum(file.Buffer);

            var request = new PutObjectRequest
            {
                BucketName = _s3Bucket,
                Key = key,
                InputStream = new MemoryStream(file.Buffer),
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead,
            };

            await _s3!.PutObjectAsync(request, cancellationToken);
            var fileUrl = BuildS3PublicUrl(key);

            return new UploadFileResultDto
            {
                FileName = fileName,
                FileUrl = fileUrl,
                Storage = "s3",
                Checksum = checksum,
            };
        }

        public async Task<UploadFileResultDto> UploadSingleS3Async(
            UploadFileInput file,
            bool isHd = false,
            CancellationToken cancellationToken = default)
        {
            ValidateFile(file);
            EnsureS3Configured();

            var current = DateTime.Now;
            var temp = file.FileName.Split('.');
            var ext = temp.Length > 1 ? $".{temp[^1]}" : string.Empty;
            var fileNameOrigin = temp.Length > 0 ? temp[0][..Math.Min(temp[0].Length, 10)] : string.Empty;
            var fileNanoid = GenerateNanoId();
            var originalBuffer = file.Buffer;
            var checksum = ComputeChecksum(originalBuffer);

            var fileName = $"{current:yyyyMMdd}{fileNanoid}-{fileNameOrigin}{ext}";
            var key = string.IsNullOrWhiteSpace(_s3LinkUpload) ? fileName : $"{_s3LinkUpload.Trim('/')}/{fileName}";

            var uploadBody = originalBuffer;
            var fileExtension = temp.Length > 1 ? temp[^1].ToLowerInvariant() : string.Empty;

            if (UploadMimeHelper.ImageExtensions.Contains(fileExtension))
            {
                if (!isHd && file.Length > 102400)
                {
                    uploadBody = await OptimizeImageAsync(originalBuffer, 1920, 1080, 50, "webp", cancellationToken);
                }
            }

            var putRequest = new PutObjectRequest
            {
                BucketName = _s3Bucket,
                Key = key,
                InputStream = new MemoryStream(uploadBody),
                CannedACL = S3CannedACL.PublicRead,
            };

            if (UploadMimeHelper.ImageExtensions.Contains(fileExtension))
            {
                putRequest.ContentType = $"image/{fileExtension}";
            }
            else if (fileExtension.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                putRequest.ContentType = "application/pdf";
            }

            await _s3!.PutObjectAsync(putRequest, cancellationToken);
            var fileUrl = isHd ? $"{BuildS3PublicUrl(key)}?isHD=1" : BuildS3PublicUrl(key);

            string? mobileFileUrl = null;
            string? mobileFileName = null;

            if (UploadMimeHelper.ImageExtensions.Contains(fileExtension) && file.Length > 0)
            {
                byte[] mobileBuffer;
                if (isHd)
                {
                    mobileBuffer = await ResizeImageAsync(originalBuffer, 420, cancellationToken);
                }
                else
                {
                    var format = fileExtension == "png" ? "png" : fileExtension == "webp" ? "webp" : "jpeg";
                    mobileBuffer = await OptimizeImageAsync(originalBuffer, 420, 99999, 80, format, cancellationToken);
                }

                mobileFileName = $"{current:yyyyMMdd}{fileNanoid}-{fileNameOrigin}-mobile{ext}";
                var mobileKey = string.IsNullOrWhiteSpace(_s3LinkUpload)
                    ? mobileFileName
                    : $"{_s3LinkUpload.Trim('/')}/{mobileFileName}";

                var mobileRequest = new PutObjectRequest
                {
                    BucketName = _s3Bucket,
                    Key = mobileKey,
                    InputStream = new MemoryStream(mobileBuffer),
                    CannedACL = S3CannedACL.PublicRead,
                    ContentType = $"image/{fileExtension}",
                };

                await _s3.PutObjectAsync(mobileRequest, cancellationToken);
                mobileFileUrl = BuildS3PublicUrl(mobileKey);
            }

            return new UploadFileResultDto
            {
                FileName = fileName,
                FileUrl = fileUrl,
                UrlResize = mobileFileUrl,
                MobileFileUrl = mobileFileUrl,
                MobileFileName = mobileFileName,
                IsHD = isHd,
                Checksum = checksum,
                Storage = "s3",
            };
        }

        public async Task<List<UploadFileResultDto>> UploadMultiS3Async(
            IReadOnlyList<UploadFileInput> files,
            CancellationToken cancellationToken = default)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentException("Danh sách file trống");
            }

            var results = new List<UploadFileResultDto>(files.Count);
            foreach (var file in files)
            {
                results.Add(await UploadSingleS3Async(file, cancellationToken: cancellationToken));
            }

            return results;
        }

        public async Task<DownloadImagesAsZipResultDto> DownloadImagesAsZipAsync(
            DownloadImagesAsZipDto payload,
            CancellationToken cancellationToken = default)
        {
            if (payload.ImageUrls == null || payload.ImageUrls.Count == 0)
            {
                throw new ArgumentException("Danh sách URL ảnh trống");
            }

            using var semaphore = new SemaphoreSlim(5);
            var downloadTasks = payload.ImageUrls.Select(async (imageUrl, index) =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await DownloadImageFileAsync(imageUrl, index + 1, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var downloadResults = await Task.WhenAll(downloadTasks);
            var downloadedFiles = downloadResults.Where(file => file != null).Cast<DownloadedImageFile>().ToList();

            if (downloadedFiles.Count == 0)
            {
                throw new InvalidOperationException("No valid images could be downloaded");
            }

            var zipBuffer = await CreateZipBufferAsync(downloadedFiles, cancellationToken);
            var safeTitle = UploadMimeHelper.NormalizeZipTitle(payload.FileTitle);

            return new DownloadImagesAsZipResultDto
            {
                Buffer = zipBuffer,
                Filename = $"{safeTitle}.zip",
                TotalUrls = payload.ImageUrls.Count,
                SuccessCount = downloadedFiles.Count,
                SkippedCount = payload.ImageUrls.Count - downloadedFiles.Count,
            };
        }

        private async Task<(string FileName, string FileUrl, string Checksum)> UploadToCloudinaryAsync(
            UploadFileInput file,
            string folder,
            CancellationToken cancellationToken)
        {
            var fileId = GenerateFileId();
            var ext = UploadMimeHelper.GetExtension(file.ContentType);
            var checksum = ComputeChecksum(file.Buffer);

            await using var stream = new MemoryStream(file.Buffer);
            var fileDescription = new FileDescription($"{fileId}.{ext}", stream);
            var category = UploadMimeHelper.DetectCategory(file.ContentType);

            UploadResult result;
            switch (category)
            {
                case "audio":
                    result = await Task.Run(() => _cloudinary.Upload(new VideoUploadParams
                    {
                        File = fileDescription,
                        Folder = folder,
                        PublicId = fileId,
                    }), cancellationToken);
                    break;
                case "document":
                case "other":
                    result = await Task.Run(() => _cloudinary.Upload(new RawUploadParams
                    {
                        File = fileDescription,
                        Folder = folder,
                        PublicId = fileId,
                    }), cancellationToken);
                    break;
                default:
                    result = await Task.Run(() => _cloudinary.Upload(new ImageUploadParams
                    {
                        File = fileDescription,
                        Folder = folder,
                        PublicId = fileId,
                    }), cancellationToken);
                    break;
            }

            if (result.Error != null)
            {
                throw new InvalidOperationException($"Upload Cloudinary thất bại: {result.Error.Message}");
            }

            return ($"{fileId}.{ext}", result.SecureUrl.ToString(), checksum);
        }

        private async Task<(string FileName, string FileUrl, string Checksum)> UploadToCatboxAsync(
            UploadFileInput file,
            CancellationToken cancellationToken)
        {
            var ext = UploadMimeHelper.GetExtension(file.ContentType);
            var fileName = $"{GenerateFileId()}.{ext}";
            var checksum = ComputeChecksum(file.Buffer);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("fileupload"), "reqtype");
            if (!string.IsNullOrWhiteSpace(_catboxUserhash))
            {
                content.Add(new StringContent(_catboxUserhash), "userhash");
            }

            var fileContent = new ByteArrayContent(file.Buffer);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "fileToUpload", file.FileName);

            var client = _httpClientFactory.CreateClient(nameof(UploadFileService));
            using var response = await client.PostAsync(CatboxApiUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Catbox API returned status {(int)response.StatusCode}");
            }

            var fileUrl = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            return (fileName, fileUrl, checksum);
        }

        private async Task<DownloadedImageFile?> DownloadImageFileAsync(
            string imageUrl,
            int index,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(nameof(UploadFileService));
                client.Timeout = TimeSpan.FromSeconds(15);
                using var response = await client.GetAsync(imageUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(contentType) && !contentType.StartsWith("image/", StringComparison.Ordinal))
                {
                    return null;
                }

                var buffer = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (buffer.Length == 0)
                {
                    return null;
                }

                var extension = UploadMimeHelper.ResolveImageExtension(contentType, imageUrl);
                return new DownloadedImageFile($"image-{index}{extension}", buffer);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<byte[]> CreateZipBufferAsync(
            IReadOnlyList<DownloadedImageFile> files,
            CancellationToken cancellationToken)
        {
            await using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var entry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    await entryStream.WriteAsync(file.Buffer, cancellationToken);
                }
            }

            return memoryStream.ToArray();
        }

        private static async Task<byte[]> OptimizeImageAsync(
            byte[] buffer,
            int maxWidth,
            int maxHeight,
            int quality,
            string format,
            CancellationToken cancellationToken)
        {
            await using var input = new MemoryStream(buffer);
            using var image = await Image.LoadAsync(input, cancellationToken);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(maxWidth, maxHeight),
            }));

            await using var output = new MemoryStream();
            switch (format)
            {
                case "webp":
                    await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = quality }, cancellationToken);
                    break;
                case "png":
                    await image.SaveAsPngAsync(output, new PngEncoder(), cancellationToken);
                    break;
                default:
                    await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = quality }, cancellationToken);
                    break;
            }

            return output.ToArray();
        }

        private static async Task<byte[]> ResizeImageAsync(byte[] buffer, int maxWidth, CancellationToken cancellationToken)
        {
            await using var input = new MemoryStream(buffer);
            using var image = await Image.LoadAsync(input, cancellationToken);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(maxWidth, int.MaxValue),
            }));

            await using var output = new MemoryStream();
            await image.SaveAsync(output, image.Metadata.DecodedImageFormat!, cancellationToken);
            return output.ToArray();
        }

        private string BuildS3PublicUrl(string key)
        {
            if (!string.IsNullOrWhiteSpace(_s3LinkUpload))
            {
                return $"{_s3LinkUpload.TrimEnd('/')}/{key}";
            }

            return $"https://{_s3Bucket}.s3.amazonaws.com/{key}";
        }

        private void EnsureS3Configured()
        {
            if (_s3 == null)
            {
                throw new InvalidOperationException(
                    "AWS S3 chưa được cấu hình (AWS_S3_ACCESS_KEY_ID, AWS_S3_SECRET_ACCESS_KEY)");
            }

            if (string.IsNullOrWhiteSpace(_s3Bucket))
            {
                throw new InvalidOperationException("AWS S3 Bucket name trống");
            }
        }

        private static void ValidateFile(UploadFileInput file)
        {
            if (file == null || file.Buffer.Length == 0)
            {
                throw new ArgumentException("File is required");
            }
        }

        private static string GenerateFileId()
        {
            var now = DateTime.Now;
            var random = RandomNumberGenerator.GetHexString(6, lowercase: true);
            return $"{now.Year}{now.Month}{now.Day}-{random}";
        }

        private static string GenerateNanoId(int length = 10)
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var builder = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                builder.Append(NanoAlphabet[bytes[i] % NanoAlphabet.Length]);
            }

            return builder.ToString();
        }

        private static string ComputeChecksum(byte[] buffer)
        {
            return Convert.ToHexString(SHA256.HashData(buffer)).ToLowerInvariant();
        }

        private static string? GetConfig(IConfiguration configuration, string sectionKey, string envKey)
        {
            return configuration[sectionKey] ?? Environment.GetEnvironmentVariable(envKey);
        }

        private sealed record DownloadedImageFile(string FileName, byte[] Buffer);
    }
}
