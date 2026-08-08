using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Upload;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IUploadFileService
    {
        Task<UploadFileResultDto> UploadSingleAsync(UploadFileInput file, CancellationToken cancellationToken = default);
        Task<List<UploadFileResultDto>> UploadMultiAsync(IReadOnlyList<UploadFileInput> files, CancellationToken cancellationToken = default);
        Task<UploadFileResultDto> UploadImageAsync(UploadFileInput file, string? folder = null, CancellationToken cancellationToken = default);
        Task<UploadFileResultDto> UploadAudioAsync(UploadFileInput file, string? folder = null, CancellationToken cancellationToken = default);
        Task<UploadFileResultDto> UploadDocumentAsync(UploadFileInput file, string? folder = null, CancellationToken cancellationToken = default);
        Task<UploadFileResultDto> UploadCatboxAsync(UploadFileInput file, CancellationToken cancellationToken = default);
        Task<UploadFileResultDto> UploadToCatboxFromUrlAsync(string url, CancellationToken cancellationToken = default);
        Task<UploadFileResultDto> UploadS3Async(UploadFileInput file, string? folder = null, CancellationToken cancellationToken = default);
        Task<UploadFileResultDto> UploadSingleS3Async(UploadFileInput file, bool isHd = false, CancellationToken cancellationToken = default);
        Task<List<UploadFileResultDto>> UploadMultiS3Async(IReadOnlyList<UploadFileInput> files, CancellationToken cancellationToken = default);
        Task<DownloadImagesAsZipResultDto> DownloadImagesAsZipAsync(DownloadImagesAsZipDto payload, CancellationToken cancellationToken = default);
    }
}
