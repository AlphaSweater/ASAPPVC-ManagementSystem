using ASAPPVC.UI.Utils;

namespace ASAPPVC.UI.Services
{
    public interface IImageService
    {
        Task<Result<ImagePayload>> ProcessUploadAsync(IFormFile file, CancellationToken ct = default);

        Task<Result<ImagePayload>> ProcessBytesAsync(byte[] data, string contentType, CancellationToken ct = default);
    }
}