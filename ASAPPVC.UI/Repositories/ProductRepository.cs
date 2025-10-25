using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class ProductRepository(AppDbContext db) : BaseRepository<ProductModel>(db), IProductRepository
    {
        // ---------- Domain-flavoured CRUD ----------

        /// <summary>Add product only and save.</summary>
        public async Task<ProductModel> AddProductAsync(ProductModel product, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(product);
            var added = await AddAsync(product, ct);
            await SaveAsync(ct);
            return added;
        }

        /// <summary>
        /// Add product + its component bridge rows in one save.
        /// </summary>
        public async Task<ProductModel> AddProductWithComponentsAsync(
            ProductModel product,
            IEnumerable<ProductComponentModel> components,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentNullException.ThrowIfNull(components);

            if (product.Id == Guid.Empty)
                product.Id = Guid.NewGuid();

            await AddAsync(product, ct); // staged

            var rows = components as IList<ProductComponentModel> ?? components.ToList();
            if (rows.Count > 0)
            {
                foreach (var r in rows)
                    r.ProductId = product.Id; // normalize

                await AddRangeToBridgeAsync(_db.ProductComponents, rows, ct);
            }

            await SaveAsync(ct);
            return product;
        }

        // Update + Save (fields only; does not touch components)
        public async Task<bool> UpdateProductAsync(ProductModel product, CancellationToken ct = default)
        {
            Update(product);
            return await SaveAsync(ct) > 0;
        }

        // Delete + Save (also relies on cascade rules for bridge rows)
        public async Task<bool> DeleteProductAsync(Guid id, CancellationToken ct = default)
        {
            if (!await RemoveByIdAsync(id, ct))
                return false;
            return await SaveAsync(ct) > 0;
        }

        // Replace the entire component set for a product (delete old, add new) + Save
        public async Task<bool> ReplaceComponentsAsync(Guid productId, IEnumerable<ProductComponentModel> components, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(components);

            // Delete existing
            var existing = await _db.ProductComponents
                .Where(pp => pp.ProductId == productId)
                .ToListAsync(ct);

            if (existing.Count > 0)
                _db.ProductComponents.RemoveRange(existing);

            // Add new (normalize productId just in case)
            var rows = components.ToList();
            foreach (var r in rows)
                r.ProductId = productId;
            if (rows.Count > 0)
                await _db.ProductComponents.AddRangeAsync(rows, ct);

            return await SaveAsync(ct) > 0;
        }

        // ---------- Reads with small interpretations ----------

        public Task<ProductModel?> GetWithComponentsAsync(Guid id, CancellationToken ct = default)
        {
            return _set.AsNoTracking()
                           .Include(p => p.ProductComponents)
                             .ThenInclude(pp => pp.Component)
                           .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public Task<List<ProductModel>> ListOrderedByNameAsync(CancellationToken ct = default)
        {
            return _set.AsNoTracking()
                           .OrderBy(p => p.Name)
                           .ToListAsync(ct);
        }

        public Task<ProductModel?> GetByProductCodeAsync(string productCode, CancellationToken ct = default)
        {
            return FirstOrDefaultAsync(p => p.ProductCode == productCode, asNoTracking: true, ct);
        }

        public Task<List<ProductModel>> SearchAsync(string term, CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
                return ListOrderedByNameAsync(ct);

            return _set.AsNoTracking()
                       .Where(p =>
                           EF.Functions.Like(p.Name, $"%{term}%") ||
                           EF.Functions.Like(p.ProductCode, $"%{term}%"))
                       .OrderBy(p => p.Name)
                       .ToListAsync(ct);
        }

        // ---------- Utility ----------

        public Task<List<ComponentModel>> GetComponentsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(ids);
            var list = ids as ICollection<Guid> ?? ids.ToArray();
            if (list.Count == 0)
                return Task.FromResult(new List<ComponentModel>());
            return _db.Components.Where(p => list.Contains(p.Id)).AsNoTracking().ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\