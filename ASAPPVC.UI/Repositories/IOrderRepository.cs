using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task AddOrderProductsAsync(IEnumerable<OrderProductModel> lines, CancellationToken ct = default);

        Task<List<Order>> ListAsync(CancellationToken ct = default);

        Task<Order?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    }
}