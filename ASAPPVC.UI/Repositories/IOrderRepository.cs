using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface IOrderRepository : IBaseRepository<OrderModel>
    {
        Task AddOrderProductsAsync(IEnumerable<OrderProductModel> lines, CancellationToken ct = default);

        Task<List<OrderModel>> ListAsync(CancellationToken ct = default);

        Task<OrderModel?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    }
}