using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class PartRepository : IPartRepository
    {
        private readonly AppDbContext _db;

        public PartRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PartModel> AddAsync(PartModel part, CancellationToken ct = default)
        {
            var entry = await _db.Part.AddAsync(part, ct);
            return entry.Entity;
        }

        public async Task<PartModel?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Part.FirstOrDefaultAsync(p => p.PartID == id, ct);
        }

        public async Task<List<PartModel>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Part.OrderBy(p => p.Name).ToListAsync(ct);
        }

        public async Task SaveAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}