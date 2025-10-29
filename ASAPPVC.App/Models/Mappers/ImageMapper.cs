using ASAPPVC.App.Models.General;
using ASAPPVC.App.Services;

namespace ASAPPVC.App.Models.Mappers
{
    internal static class ImageMapper
    {
        public static AppImage ToAppImage(this ImagePayload p)
        {
            return new()
            {
                Data = p.Data,
                Thumb = p.Thumb,
                ContentType = p.ContentType,
                Width = p.Width,
                Height = p.Height,
                Length = p.Length,
                Sha256 = p.Sha256,
                UploadedUtc = p.UploadedUtc
            };
        }
    }
}