using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Controllers.Images
{
    /// <summary>
    /// Serves product images with clean routing and ETag caching.
    /// </summary>
    [Route("products/{id:guid}/image")]
    public sealed class ProductImagesController(AppDbContext db) : Controller
    {
        private readonly AppDbContext _db = db;

        /// <summary>
        /// GET /products/{id}/image - Returns the full product image
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var img = await _db.Set<Product>()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => p.Image)
                .FirstOrDefaultAsync(ct);

            return ImageResult.Stream(this, img, thumb: false);
        }

        /// <summary>
        /// GET /products/{id}/image/thumb - Returns the thumbnail
        /// </summary>
        [HttpGet("thumb")]
        public async Task<IActionResult> GetThumb(Guid id, CancellationToken ct)
        {
            var img = await _db.Set<Product>()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => p.Image)
                .FirstOrDefaultAsync(ct);

            return ImageResult.Stream(this, img, thumb: true);
        }
    }
}