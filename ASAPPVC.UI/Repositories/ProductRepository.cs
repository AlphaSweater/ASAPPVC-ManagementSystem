using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class ProductRepository(AppDbContext db) : BaseRepository<Product>(db), IProductRepository
    {
        // ---------- Reads ----------

        public async Task<Product?> GetByIdOrCodeAsync(
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

        public async Task<Product?> GetByIdOrCodeWithComponentsAsync(
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

        public async Task<List<Product>> GetListByIdsOrCodesAsync(
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
            return new List<Product>(0);
        }

        public async Task<List<Product>> GetListByIdsOrCodesWithComponentsAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default)
        {
            // Prefer IDs when any valid Guid is present
            var hasValidIds = ids?.Any(g => g != Guid.Empty) == true;
            if (hasValidIds)
            {
                var idSet = ids!.Where(g => g != Guid.Empty).ToHashSet();
                return await _set.AsNoTracking()
                                 .Where(p => idSet.Contains(p.Id))
                                 .Include(p => p.ProductComponents)
                                   .ThenInclude(pc => pc.Component)
                                 .OrderBy(p => p.ProductCode)
                                 .ToListAsync(ct);
            }

            // Fallback to codes when any non-blank code is present
            var hasValidCodes = codes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true;
            if (hasValidCodes)
            {
                var codeSet = codes!.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                return await _set.AsNoTracking()
                                 .Where(p => codeSet.Contains(p.ProductCode))
                                 .Include(p => p.ProductComponents)
                                   .ThenInclude(pc => pc.Component)
                                 .OrderBy(p => p.ProductCode)
                                 .ToListAsync(ct);
            }

            // Neither provided => empty
            return new List<Product>(0);
        }

        public async Task<List<Product>> GetListAsync(CancellationToken ct = default)
        {
            var list = await ListAsync(asNoTracking: true, ct);
            return list.OrderBy(p => p.ProductCode).ToList();
        }

        public async Task<List<Product>> GetListWithComponentsAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                             .Include(p => p.ProductComponents)
                               .ThenInclude(pc => pc.Component)
                             .OrderBy(p => p.ProductCode)
                             .ToListAsync(ct);
        }

        // ---------- Search ----------

        public Task<List<Product>> SearchAsync(string term, CancellationToken ct = default)
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