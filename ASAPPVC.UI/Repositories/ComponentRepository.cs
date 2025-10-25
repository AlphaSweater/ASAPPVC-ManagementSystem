using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class ComponentRepository(AppDbContext db) : BaseRepository<ComponentModel>(db), IComponentRepository
    {
        // ---------- Domain-flavoured CRUD (compose + Save) ----------
        public async Task<ComponentModel> AddComponentAsync(ComponentModel part, CancellationToken ct = default)
        {
            // Compose base Add + Save
            var added = await AddAsync(part, ct);
            await SaveAsync(ct);
            return added;
        }

        public async Task<bool> UpdateComponentAsync(ComponentModel part, CancellationToken ct = default)
        {
            Update(part);
            return await SaveAsync(ct) > 0;
        }

        public async Task<bool> DeleteComponentAsync(Guid id, CancellationToken ct = default)
        {
            if (!await RemoveByIdAsync(id, ct))
                return false;
            return await SaveAsync(ct) > 0;
        }

        // ---------- Reads with slight interpretation ----------
        public Task<List<ComponentModel>> ListOrderedByNameAsync(CancellationToken ct = default)
        {
            return _set.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
        }

        public Task<ComponentModel?> GetByComponentCodeAsync(string partCode, CancellationToken ct = default)
        {
            return FirstOrDefaultAsync(p => p.ComponentCode == partCode, asNoTracking: true, ct);
        }

        public Task<List<ComponentModel>> SearchAsync(string term, CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
                return ListOrderedByNameAsync(ct);

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