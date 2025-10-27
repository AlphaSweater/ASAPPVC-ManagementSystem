using ASAPPVC.UI.Utils;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;

namespace ASAPPVC.UI.Services
{
    public sealed class ImageService(IOptions<ImageServiceOptions> options) : IImageService
    {
        private readonly ImageServiceOptions _opt = options.Value;

        public async Task<Result<ImagePayload>> ProcessUploadAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                return Result<ImagePayload>.Fail("No file provided.");

            if (file.Length > _opt.MaxBytes)
                return Result<ImagePayload>.Fail($"File too large. Max {_opt.MaxBytes / 1024 / 1024} MB.");

            if (!_opt.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return Result<ImagePayload>.Fail("Unsupported image type.");

            // Read into memory once
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();

            // Validate magic bytes (basic)
            if (!LooksLikeImage(bytes))
                return Result<ImagePayload>.Fail("File content is not a valid image.");

            return await ProcessBytesInternalAsync(bytes, file.ContentType, ct);
        }

        public Task<Result<ImagePayload>> ProcessBytesAsync(byte[] data, string contentType, CancellationToken ct = default)
        {
            if (data is null || data.Length == 0)
                return Task.FromResult(Result<ImagePayload>.Fail("Empty data."));
            if (!_opt.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
                return Task.FromResult(Result<ImagePayload>.Fail("Unsupported image type."));

            if (!LooksLikeImage(data))
                return Task.FromResult(Result<ImagePayload>.Fail("Data is not a valid image."));

            return ProcessBytesInternalAsync(data, contentType, ct);
        }

        // ---- Internals ----

        private async Task<Result<ImagePayload>> ProcessBytesInternalAsync(byte[] input, string inputContentType, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            IImageFormat? detected = null;
            try
            {
                using var msImg = new MemoryStream(input, writable: false);
                detected = Image.DetectFormat(msImg);
                if (detected is null)
                    return Result<ImagePayload>.Fail("Unsupported or corrupted image.");

                // Enforce allowlist against what was actually detected
                var detectedMime = detected.DefaultMimeType ?? inputContentType;
                if (!_opt.AllowedContentTypes.Contains(detectedMime, StringComparer.OrdinalIgnoreCase))
                    return Result<ImagePayload>.Fail("Unsupported image type.");

                msImg.Position = 0;
                using var img = Image.Load(msImg); // uses same config as DetectFormat

                // Optional: pixel-bomb guard (e.g., > ~100 MP)
                const long MaxPixels = 100L * 1024 * 1024;
                if ((long)img.Width * img.Height > MaxPixels)
                    return Result<ImagePayload>.Fail("Image dimensions are too large.");

                img.Metadata.ExifProfile = null;
                img.Metadata.IccProfile = null;
                img.Metadata.XmpProfile = null;

                Constrain(img, _opt.MaxWidth, _opt.MaxHeight);

                // Prefer ForceEncodeAs; otherwise keep detected
                var targetMime = _opt.ForceEncodeAs ?? detectedMime;
                var (encoder, finalContentType) = ChooseEncoder(targetMime);

                var mainBytes = await EncodeAsync(img, encoder, ct);

                using var thumbClone = img.Clone(x => { });
                Constrain(thumbClone, _opt.ThumbWidth, _opt.ThumbHeight, upscale: false);
                var thumbBytes = await EncodeAsync(thumbClone, encoder, ct);

                var sha256 = ComputeSha256(mainBytes);

                var payload = new ImagePayload
                {
                    Data = mainBytes,
                    Thumb = thumbBytes,
                    ContentType = finalContentType,
                    Width = img.Width,
                    Height = img.Height,
                    Length = mainBytes.LongLength,
                    Sha256 = sha256
                };
                return Result<ImagePayload>.Success(payload);
            }
            catch (UnknownImageFormatException)
            {
                return Result<ImagePayload>.Fail("Unsupported or corrupted image.");
            }
            catch (SixLabors.ImageSharp.InvalidImageContentException)
            {
                return Result<ImagePayload>.Fail("Invalid image content.");
            }
        }

        private static void Constrain(Image img, int maxW, int maxH, bool upscale = false)
        {
            var w = img.Width;
            var h = img.Height;
            if (!upscale && w <= maxW && h <= maxH)
                return;

            var ratio = Math.Min((double)maxW / w, (double)maxH / h);
            if (!upscale && ratio >= 1)
                return;

            var nw = Math.Max(1, (int)Math.Round(w * ratio));
            var nh = Math.Max(1, (int)Math.Round(h * ratio));
            img.Mutate(x => x.Resize(nw, nh));
        }

        private static (IImageEncoder encoder, string mime) ChooseEncoder(string desiredMime)
        {
            return desiredMime.ToLowerInvariant() switch
            {
                "image/png" => (new PngEncoder { CompressionLevel = PngCompressionLevel.Level6 }, "image/png"),
                "image/webp" => (new WebpEncoder { Quality = 85 }, "image/webp"),
                _ => (new JpegEncoder { Quality = 85 }, "image/jpeg")
            };
        }

        private static async Task<byte[]> EncodeAsync(Image img, IImageEncoder encoder, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await img.SaveAsync(ms, encoder, ct);
            return ms.ToArray();
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }

        private static bool LooksLikeImage(byte[] data)
        {
            // very small signature checks for JPEG/PNG/WebP
            if (data.Length < 12)
                return false;

            // JPEG: FF D8
            if (data[0] == 0xFF && data[1] == 0xD8)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (data.Length >= 8 &&
                data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
                data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
                return true;

            // WebP: "RIFF....WEBP"
            if (data.Length >= 12 &&
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
                return true;

            return false;
        }
    }

    #region Options and Payload

    public sealed class ImageServiceOptions
    {
        public long MaxBytes { get; set; } = 5 * 1024 * 1024; // 5 MB
        public int MaxWidth { get; set; } = 2048;
        public int MaxHeight { get; set; } = 2048;

        public int ThumbWidth { get; set; } = 400;
        public int ThumbHeight { get; set; } = 400;

        public string[] AllowedContentTypes { get; set; } = new[]
        { "image/jpeg", "image/png", "image/webp" };

        // null = keep original, otherwise force encode to one of "image/jpeg","image/png","image/webp"
        public string? ForceEncodeAs { get; set; } = null;
    }

    public sealed class ImagePayload
    {
        public byte[] Data { get; init; }
        public byte[]? Thumb { get; init; }
        public string ContentType { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
        public long Length { get; init; }
        public string Sha256 { get; init; }
        public DateTime UploadedUtc { get; init; } = DateTime.UtcNow;
    }

    #endregion Options and Payload
}