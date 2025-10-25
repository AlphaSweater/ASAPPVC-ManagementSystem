using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface IOrderRepository : IBaseRepository<OrderModel>
    {
        Task<OrderModel> AddOrderAsync(OrderModel order, CancellationToken ct = default);

        Task AddOrderProductsAsync(IEnumerable<OrderProductModel> lines, CancellationToken ct = default);

        Task<List<OrderModel>> ListAsync(CancellationToken ct = default);

        Task<OrderModel?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    }
}