using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class ComponentRepository(AppDbContext db) : BaseRepository<ComponentModel>(db), IComponentRepository
    {
        protected override string? CodePropertyName => "ComponentCode";

        // ---------- Reads ----------

        public async Task<ComponentModel?> GetByIdOrCodeAsync(
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

        public async Task<List<ComponentModel>> GetListByIdOrCodeAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default)
        {
            // Prefer IDs when any valid Guid is present
            var hasValidIds = ids?.Any(g => g != Guid.Empty) == true;
            if (hasValidIds)
                return await GetByIdsAsync(ids!, asNoTracking: true, ct);

            // Fallback to codes when any non-blank code is present
            var hasValidCodes = codes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true;
            if (hasValidCodes)
                return await GetByCodesAsync(codes!, asNoTracking: true, ct);

            // Neither provided => empty
            return new List<ComponentModel>(0);
        }

        public Task<List<ComponentModel>> GetListAsync(CancellationToken ct = default)
        {
            return _set.AsNoTracking()
                       .OrderBy(c => c.ComponentCode)
                       .ToListAsync(ct);
        }

        // ---------- Search ----------

        public Task<List<ComponentModel>> SearchAsync(string term, CancellationToken ct = default)
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
                                || EF.Functions.Like(p.ComponentCode, $"%{term}%"))
                       .OrderBy(p => p.Name)
                       .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\