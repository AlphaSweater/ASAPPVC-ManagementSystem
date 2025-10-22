using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        //─────────── Dependencies ───────────\\
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //adds products to the database
        public async Task<ProductModel> AddProductAsync(ProductModel product, CancellationToken ct = default)
        {
            var entry = await _db.Product.AddAsync(product, ct);
            return entry.Entity;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //adds product parts to the database
        public Task AddProductPartsAsync(IEnumerable<ProductPartModel> lines, CancellationToken ct = default)
        {
            _db.ProductPart.AddRange(lines);
            return Task.CompletedTask;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves parts by their IDs
        public async Task<List<PartModel>> GetPartsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            return await _db.Part.Where(p => ids.Contains(p.PartID)).ToListAsync(ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves a product along with its associated parts
        public async Task<ProductModel?> GetProductWithPartsAsync(int id, CancellationToken ct = default)
        {
            return await _db.Product
                  .AsNoTracking()
                  .Include(p => p.ProductParts)
                    .ThenInclude(pp => pp.Part)
                  .FirstOrDefaultAsync(p => p.ProductID == id, ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //lists all products ordered by name
        public async Task<List<ProductModel>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Product.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //saves changes to the database
        public async Task SaveAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\