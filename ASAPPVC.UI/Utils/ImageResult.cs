using ASAPPVC.UI.Models.General;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Utils
{
    /// <summary>
    /// Helper for returning image file results with proper caching headers.
    /// </summary>
    public static class ImageResult
    {
        public static IActionResult Stream(ControllerBase ctrl, AppImage? img, bool thumb, string? etagSuffix = null)
        {
            if (img is null)
                return ctrl.NotFound();
            var bytes = thumb ? img.Thumb : img.Data;
            if (bytes is null || bytes.Length == 0)
                return ctrl.NotFound();

            var etag = $"\"{img.Sha256}{(thumb ? "-thumb" : "")}{etagSuffix}\"";
            if (ctrl.Request.Headers.TryGetValue("If-None-Match", out var inm) && inm == etag)
                return ctrl.StatusCode(StatusCodes.Status304NotModified);

            ctrl.Response.Headers["ETag"] = etag;
            ctrl.Response.Headers["Cache-Control"] = "public,max-age=86400";
            return ctrl.File(bytes, img.ContentType);
        }
    }
}