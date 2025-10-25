using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class PartRepository(AppDbContext db) : BaseRepository<PartModel>(db), IPartRepository
    {
        // convenience: strongly-typed lookup by int id
        public async Task<PartModel?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _set.FirstOrDefaultAsync(p => p.PartID == id, ct);
        }

        // retrieves a list of parts ordered by name
        public async Task<List<PartModel>> ListAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\