using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<OrderModel> AddOrderAsync(OrderModel order, CancellationToken ct = default)
        {
            var entry = await _db.Order.AddAsync(order, ct);
            return entry.Entity;
        }

        public Task AddOrderProductsAsync(IEnumerable<OrderProductModel> lines, CancellationToken ct = default)
        {
            _db.OrderProduct.AddRange(lines);
            return Task.CompletedTask;
        }

        public async Task<List<OrderModel>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Order
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(ct);
        }

        public async Task<OrderModel?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _db.Order
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id, ct);
        }

        public async Task SaveAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
