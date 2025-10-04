using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class PartRepository : IPartRepository
    {
        private readonly AppDbContext _db;
        public PartRepository(AppDbContext db) => _db = db;

        public async Task<PartModel> AddAsync(PartModel part, CancellationToken ct = default)
        {
            var entry = await _db.Part.AddAsync(part, ct);
            return entry.Entity;
        }

        public Task<PartModel?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Part.FirstOrDefaultAsync(p => p.PartID == id, ct);

        public Task<List<PartModel>> ListAsync(CancellationToken ct = default)
            => _db.Part.OrderBy(p => p.Name).ToListAsync(ct);

        public Task SaveAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
