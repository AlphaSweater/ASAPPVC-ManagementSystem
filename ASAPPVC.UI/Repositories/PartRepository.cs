using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories.Interfaces
{
    public class PartRepository : IPartRepository
    {
        //─────────── Dependencies ───────────\\
        private readonly AppDbContext _db;

        public PartRepository(AppDbContext db)
        {
            _db = db;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // adds a new part to the database
        public async Task<PartModel> AddAsync(PartModel part, CancellationToken ct = default)
        {
            var entry = await _db.Part.AddAsync(part, ct);
            return entry.Entity;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // retrieves a part by its ID
        public async Task<PartModel?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Part.FirstOrDefaultAsync(p => p.PartID == id, ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // retrieves a list of parts ordered by name
        public async Task<List<PartModel>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Part.OrderBy(p => p.Name).ToListAsync(ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // saves changes to the database
        public async Task SaveAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\