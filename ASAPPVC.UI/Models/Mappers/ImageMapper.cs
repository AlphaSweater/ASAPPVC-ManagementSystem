using ASAPPVC.UI.Models.General;
using ASAPPVC.UI.Services;

namespace ASAPPVC.UI.Models.Mappers
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