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
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer Id lookups if any valid Guid provided
            if (id.HasValue && id.Value != Guid.Empty)
                return await GetByIdAsync(id.Value, asNoTracking, ct);

            // Fallback to Code lookups if any non-blank string provided
            if (!string.IsNullOrWhiteSpace(code))
                return await GetByCodeAsync(code!, asNoTracking, ct);

            // Neither provided => nothing to fetch
            return null;
        }

        public async Task<Product?> GetByIdOrCodeWithComponentsAsync(
            Guid? id = null,
            string? code = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer Id lookups if any valid Guid provided
            if (id.HasValue && id.Value != Guid.Empty)
            {
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
                    .Include(p => p.ProductComponents)
                    .ThenInclude(pc => pc.Component)
                    .FirstOrDefaultAsync(p => p.Id == id.Value, ct);
            }

            // Fallback to Code lookups if any non-blank string provided
            if (!string.IsNullOrWhiteSpace(code))
            {
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
                    .Include(p => p.ProductComponents)
                    .ThenInclude(pc => pc.Component)
                    .FirstOrDefaultAsync(p => p.ProductCode == code, ct);
            }

            // Neither provided => nothing to fetch
            return null;
        }

        public async Task<List<Product>> GetListByIdsOrCodesAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer IDs when any valid Guid is present
            var hasValidIds = ids?.Any(g => g != Guid.Empty) == true;
            if (hasValidIds)
                return await GetListByIdsAsync(ids!, asNoTracking, ct);

            // Fallback to codes when any non-blank code is present
            var hasValidCodes = codes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true;
            if (hasValidCodes)
                return await GetListByCodesAsync(codes!, asNoTracking, ct);

            // Neither provided => empty
            return new List<Product>(0);
        }

        public async Task<List<Product>> GetListByIdsOrCodesWithComponentsAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer IDs when any valid Guid is present
            var hasValidIds = ids?.Any(g => g != Guid.Empty) == true;
            if (hasValidIds)
            {
                var idSet = ids!.Where(g => g != Guid.Empty).ToHashSet();
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
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
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
                    .Where(p => codeSet.Contains(p.ProductCode))
                    .Include(p => p.ProductComponents)
                    .ThenInclude(pc => pc.Component)
                    .OrderBy(p => p.ProductCode)
                    .ToListAsync(ct);
            }

            // Neither provided => empty
            return new List<Product>(0);
        }

        public async Task<List<Product>> GetListAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            var list = await ListAsync(asNoTracking, ct);
            return list.OrderBy(p => p.ProductCode).ToList();
        }

        public async Task<List<Product>> GetListWithComponentsAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
            return await query
                .Include(p => p.ProductComponents)
                .ThenInclude(pc => pc.Component)
                .OrderBy(p => p.ProductCode)
                .ToListAsync(ct);
        }

        // ---------- Search ----------

        public Task<List<Product>> SearchAsync(string term, bool asNoTracking = true, CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
            {
                // When no term, return full list ordered by ComponentCode (align with new default list behavior)
                return GetListAsync(asNoTracking, ct);
            }

            // Simple contains search on Name/ComponentCode; push to DB with tracking control
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
            return query.Where(p => EF.Functions.Like(p.Name, $"%{term}%")
                                || EF.Functions.Like(p.ProductCode, $"%{term}%"))
                       .OrderBy(p => p.Name)
                       .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\