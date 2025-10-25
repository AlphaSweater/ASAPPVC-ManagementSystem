using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class PartRepository(AppDbContext db) : BaseRepository<PartModel>(db), IPartRepository
    {
        // ---------- Domain-flavoured CRUD (compose + Save) ----------
        public async Task<PartModel> AddPartAsync(PartModel part, CancellationToken ct = default)
        {
            // Compose base Add + Save
            var added = await AddAsync(part, ct);
            await SaveAsync(ct);
            return added;
        }

        public async Task<bool> UpdatePartAsync(PartModel part, CancellationToken ct = default)
        {
            Update(part);
            return await SaveAsync(ct) > 0;
        }

        public async Task<bool> DeletePartAsync(int id, CancellationToken ct = default)
        {
            if (!await RemoveByIdAsync(id, ct))
                return false;
            return await SaveAsync(ct) > 0;
        }

        // ---------- Reads with slight interpretation ----------
        public Task<List<PartModel>> ListOrderedByNameAsync(CancellationToken ct = default)
        {
            return _set.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
        }

        public Task<PartModel?> GetByPartCodeAsync(string partCode, CancellationToken ct = default)
        {
            return FirstOrDefaultAsync(p => p.PartCode == partCode, asNoTracking: true, ct);
        }

        public Task<List<PartModel>> SearchAsync(string term, CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
                return ListOrderedByNameAsync(ct);

            // Simple contains search on Name/PartCode; push to DB with AsNoTracking
            return _set.AsNoTracking()
                       .Where(p => EF.Functions.Like(p.Name, $"%{term}%")
                                || EF.Functions.Like(p.PartCode, $"%{term}%"))
                       .OrderBy(p => p.Name)
                       .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\