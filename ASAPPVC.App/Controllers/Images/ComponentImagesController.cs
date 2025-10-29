using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using ASAPPVC.App.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.App.Controllers.Images
{
    /// <summary>
    /// Serves component images with clean routing and ETag caching.
    /// </summary>
    [Route("components/{id:guid}/image")]
    public sealed class ComponentImagesController(AppDbContext db) : Controller
    {
        private readonly AppDbContext _db = db;

        /// <summary>
        /// GET /components/{id}/image - Returns the full component image
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var img = await _db.Set<Component>()
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => c.Image)
                .FirstOrDefaultAsync(ct);

            return ImageResult.Stream(this, img, thumb: false);
        }

        /// <summary>
        /// GET /components/{id}/image/thumb - Returns the thumbnail
        /// </summary>
        [HttpGet("thumb")]
        public async Task<IActionResult> GetThumb(Guid id, CancellationToken ct)
        {
            var img = await _db.Set<Component>()
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => c.Image)
                .FirstOrDefaultAsync(ct);

            return ImageResult.Stream(this, img, thumb: true);
        }
    }
}