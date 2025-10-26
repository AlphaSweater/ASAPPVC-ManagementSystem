using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class ProductRepository(AppDbContext db) : BaseRepository<ProductModel>(db), IProductRepository
    {
        // ---------- Reads ----------

        public async Task<ProductModel?> GetByIdOrCodeAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            // Prefer Id lookups if any valid Guid provided
            if (id.HasValue && id.Value != Guid.Empty)
                return await GetByIdAsync(id.Value, asNoTracking: true, ct);

            // Fallback to Code lookups if any non-blank string provided
            if (!string.IsNullOrWhiteSpace(code))
                return await GetByCodeAsync(code!, asNoTracking: true, ct);

            // Neither provided => nothing to fetch
            return null;
        }

        public async Task<ProductModel?> GetByIdOrCodeWithComponentsAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            // Prefer Id lookups if any valid Guid provided
            if (id.HasValue && id.Value != Guid.Empty)
                return await _set.AsNoTracking()
                                 .Include(p => p.ProductComponents)
                                   .ThenInclude(pc => pc.Component)
                                 .FirstOrDefaultAsync(p => p.Id == id.Value, ct);

            // Fallback to Code lookups if any non-blank string provided
            if (!string.IsNullOrWhiteSpace(code))
                return await _set.AsNoTracking()
                                 .Include(p => p.ProductComponents)
                                   .ThenInclude(pc => pc.Component)
                                 .FirstOrDefaultAsync(p => p.ProductCode == code, ct);

            // Neither provided => nothing to fetch
            return null;
        }

        public async Task<List<ProductModel>> GetListByIdsOrCodesAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default)
        {
            // Prefer IDs when any valid Guid is present
            var hasValidIds = ids?.Any(g => g != Guid.Empty) == true;
            if (hasValidIds)
                return await GetListByIdsAsync(ids!, asNoTracking: true, ct);

            // Fallback to codes when any non-blank code is present
            var hasValidCodes = codes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true;
            if (hasValidCodes)
                return await GetListByCodesAsync(codes!, asNoTracking: true, ct);

            // Neither provided => empty
            return new List<ProductModel>(0);
        }

        public async Task<List<ProductModel>> GetListAsync(CancellationToken ct = default)
        {
            var list = await ListAsync(asNoTracking: true, ct);
            return list.OrderBy(p => p.ProductCode).ToList();
        }

        // ---------- Search ----------

        public Task<List<ProductModel>> SearchAsync(string term, CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
            {
                // When no term, return full list ordered by ComponentCode (align with new default list behavior)
                return GetListAsync(ct);
            }

            // Simple contains search on Name/ComponentCode; push to DB with AsNoTracking
            return _set.AsNoTracking()
                       .Where(p => EF.Functions.Like(p.Name, $"%{term}%")
                                || EF.Functions.Like(p.ProductCode, $"%{term}%"))
                       .OrderBy(p => p.Name)
                       .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\