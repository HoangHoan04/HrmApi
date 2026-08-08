using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Upload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    /// <summary>
    /// API upload file (Cloudinary, Catbox, S3)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/upload-file")]
    public class UploadFileController : ControllerBase
    {
        private readonly IUploadFileService _uploadFileService;

        public UploadFileController(IUploadFileService uploadFileService)
        {
            _uploadFileService = uploadFileService;
        }

        /// <summary>
        /// Upload single file - tự động phân loại
        /// </summary>
        [HttpPost("upload-single")]
        public async Task<ActionResult<UploadFileResultDto>> UploadSingle(IFormFile file, CancellationToken cancellationToken)
        {
            return Ok(await _uploadFileService.UploadSingleAsync(await ToUploadInputAsync(file, cancellationToken), cancellationToken));
        }

        /// <summary>
        /// Upload multiple files - tự động phân loại
        /// </summary>
        [HttpPost("upload-multi")]
        public async Task<ActionResult<List<UploadFileResultDto>>> UploadMulti(List<IFormFile> files, CancellationToken cancellationToken)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("Danh sách file trống");
            }

            var inputs = new List<UploadFileInput>(files.Count);
            foreach (var file in files)
            {
                inputs.Add(await ToUploadInputAsync(file, cancellationToken));
            }

            return Ok(await _uploadFileService.UploadMultiAsync(inputs, cancellationToken));
        }

        /// <summary>
        /// Upload ảnh
        /// </summary>
        [HttpPost("upload-image")]
        public async Task<ActionResult<UploadFileResultDto>> UploadImage(IFormFile file, CancellationToken cancellationToken)
        {
            return Ok(await _uploadFileService.UploadImageAsync(await ToUploadInputAsync(file, cancellationToken), cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Upload audio
        /// </summary>
        [HttpPost("upload-audio")]
        public async Task<ActionResult<UploadFileResultDto>> UploadAudio(IFormFile file, CancellationToken cancellationToken)
        {
            return Ok(await _uploadFileService.UploadAudioAsync(await ToUploadInputAsync(file, cancellationToken), cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Upload document
        /// </summary>
        [HttpPost("upload-document")]
        public async Task<ActionResult<UploadFileResultDto>> UploadDocument(IFormFile file, CancellationToken cancellationToken)
        {
            return Ok(await _uploadFileService.UploadDocumentAsync(await ToUploadInputAsync(file, cancellationToken), cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Upload file lên Catbox.moe
        /// </summary>
        [HttpPost("upload-catbox")]
        public async Task<ActionResult<UploadFileResultDto>> UploadCatbox(IFormFile file, CancellationToken cancellationToken)
        {
            return Ok(await _uploadFileService.UploadCatboxAsync(await ToUploadInputAsync(file, cancellationToken), cancellationToken));
        }

        /// <summary>
        /// Upload từ URL lên Catbox.moe
        /// </summary>
        [HttpPost("upload-catbox-url")]
        public async Task<ActionResult<UploadFileResultDto>> UploadCatboxUrl([FromBody] UploadCatboxUrlRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _uploadFileService.UploadToCatboxFromUrlAsync(request.Url, cancellationToken));
        }

        /// <summary>
        /// Upload file lên AWS S3
        /// </summary>
        [HttpPost("upload-s3")]
        public async Task<ActionResult<UploadFileResultDto>> UploadS3(
            IFormFile file,
            [FromForm] string? folder,
            CancellationToken cancellationToken)
        {
            return Ok(await _uploadFileService.UploadS3Async(await ToUploadInputAsync(file, cancellationToken), folder, cancellationToken));
        }

        /// <summary>
        /// Upload single S3 optimized
        /// </summary>
        [HttpPost("upload-single-s3")]
        public async Task<ActionResult<UploadFileResultDto>> UploadSingleS3(
            IFormFile file,
            [FromForm] string? isHD,
            CancellationToken cancellationToken)
        {
            var isHd = string.Equals(isHD, "true", StringComparison.OrdinalIgnoreCase);
            return Ok(await _uploadFileService.UploadSingleS3Async(await ToUploadInputAsync(file, cancellationToken), isHd, cancellationToken));
        }

        /// <summary>
        /// Upload multi S3 optimized
        /// </summary>
        [HttpPost("upload-multi-s3")]
        public async Task<ActionResult<List<UploadFileResultDto>>> UploadMultiS3(List<IFormFile> files, CancellationToken cancellationToken)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("Danh sách file trống");
            }

            var inputs = new List<UploadFileInput>(files.Count);
            foreach (var file in files)
            {
                inputs.Add(await ToUploadInputAsync(file, cancellationToken));
            }

            return Ok(await _uploadFileService.UploadMultiS3Async(inputs, cancellationToken));
        }

        /// <summary>
        /// Tải ảnh về dạng zip
        /// </summary>
        [HttpPost("download-images-zip")]
        public async Task<IActionResult> DownloadImagesZip(
            [FromBody] DownloadImagesAsZipDto payload,
            CancellationToken cancellationToken)
        {
            var result = await _uploadFileService.DownloadImagesAsZipAsync(payload, cancellationToken);
            return File(result.Buffer, "application/zip", result.Filename);
        }

        private static async Task<UploadFileInput> ToUploadInputAsync(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadHttpRequestException("File is required");
            }

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);

            return new UploadFileInput
            {
                Buffer = memoryStream.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
            };
        }
    }

    public class UploadCatboxUrlRequest
    {
        public string Url { get; set; } = string.Empty;
    }
}
