using ASAPPVC.App.Services;

namespace ASAPPVC.App.Models.General
{
    internal static class AppImageMapper
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