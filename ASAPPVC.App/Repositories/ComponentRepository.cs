using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.App.Repositories
{
    public class ComponentRepository(AppDbContext db) : BaseRepository<Component>(db), IComponentRepository
    {
        protected override string? CodePropertyName => "ComponentCode";

        // ---------- Reads ----------

        public async Task<Component?> GetByIdOrCodeAsync(
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

        public async Task<List<Component>> GetListByIdOrCodeAsync(
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
            return new List<Component>(0);
        }

        public async Task<List<Component>> GetListOrderedByCodeAsync(
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            var list = await ListAsync(asNoTracking, ct);
            return list.OrderBy(c => c.ComponentCode).ToList();
        }

        // ---------- Search ----------

        public Task<List<Component>> SearchAsync(
            string term,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
            {
                // When no term, return full list ordered by ComponentCode (align with new default list behavior)
                return GetListOrderedByCodeAsync(asNoTracking, ct);
            }

            // Simple contains search on Name/ComponentCode; push to DB with tracking control
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
            return query.Where(p => EF.Functions.Like(p.Name, $"%{term}%")
                                || EF.Functions.Like(p.ComponentCode, $"%{term}%"))
                       .OrderBy(p => p.Name)
                       .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\