using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db) => _db = db;

        public async Task<ProductModel> AddProductAsync(ProductModel product, CancellationToken ct = default)
        {
            var entry = await _db.Product.AddAsync(product, ct);
            return entry.Entity;
        }

        public Task AddProductPartsAsync(IEnumerable<ProductPartModel> lines, CancellationToken ct = default)
        {
            _db.ProductPart.AddRange(lines);
            return Task.CompletedTask;
        }

        public Task<List<PartModel>> GetPartsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
            => _db.Part.Where(p => ids.Contains(p.PartID)).ToListAsync(ct);

        public Task<ProductModel?> GetProductWithPartsAsync(int id, CancellationToken ct = default)
            => _db.Product
                  .AsNoTracking()
                  .Include(p => p.ProductParts)
                    .ThenInclude(pp => pp.Part)
                  .FirstOrDefaultAsync(p => p.ProductID == id, ct);

        public Task SaveAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
