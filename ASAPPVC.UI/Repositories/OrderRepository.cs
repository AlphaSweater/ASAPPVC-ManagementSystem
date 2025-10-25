using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class OrderRepository(AppDbContext db) : BaseRepository<OrderModel>(db), IOrderRepository
    {
        public Task AddOrderProductsAsync(IEnumerable<OrderProductModel> lines, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(lines);
            _db.OrderProduct.AddRange(lines);
            return Task.CompletedTask;
        }

        public async Task<List<OrderModel>> ListAsync(CancellationToken ct = default)
        {
            return await _set
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(ct);
        }

        public async Task<OrderModel?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        {
            return await _set
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\