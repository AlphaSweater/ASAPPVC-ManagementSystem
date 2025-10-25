using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class OrderRepository(AppDbContext db) : BaseRepository<OrderModel>(db), IOrderRepository
    {
        public async Task<OrderModel> AddOrderAsync(OrderModel order, CancellationToken ct = default)
        {
            // delegate to base AddAsync (keeps behavior consistent)
            var entry = await base.AddAsync(order, ct);
            return entry;
        }

        public Task AddOrderProductsAsync(IEnumerable<OrderProductModel> lines, CancellationToken ct = default)
        {
            // AppDbContext.OrderProduct is accessible via base protected _db
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

        public async Task<OrderModel?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _set
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id, ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\