using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class ProductRepository(AppDbContext db) : BaseRepository<ProductModel>(db), IProductRepository
    {
        // adds product parts to the database (bulk, does not save)
        public Task AddProductPartsAsync(IEnumerable<ProductPartModel> lines, CancellationToken ct = default)
        {
            _db.ProductPart.AddRange(lines);
            return Task.CompletedTask;
        }

        // retrieves parts by their IDs
        public async Task<List<PartModel>> GetPartsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            return await _db.Part.Where(p => ids.Contains(p.PartID)).ToListAsync(ct);
        }

        // retrieves a product along with its associated parts
        public async Task<ProductModel?> GetProductWithPartsAsync(int id, CancellationToken ct = default)
        {
            return await _set
                  .AsNoTracking()
                  .Include(p => p.ProductParts)
                    .ThenInclude(pp => pp.Part)
                  .FirstOrDefaultAsync(p => p.ProductID == id, ct);
        }

        // lists all products ordered by name
        public async Task<List<ProductModel>> ListAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\