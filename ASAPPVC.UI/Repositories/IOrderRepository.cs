using ASAPPVC.UI.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASAPPVC.UI.Repositories.Interfaces
{
    public interface IOrderRepository : IBaseRepository<OrderModel>
    {
        Task<OrderModel> AddOrderAsync(OrderModel order, CancellationToken ct = default);
        Task AddOrderProductsAsync(IEnumerable<OrderProductModel> lines, CancellationToken ct = default);
        Task<List<OrderModel>> ListAsync(CancellationToken ct = default);
        Task<OrderModel?> GetWithDetailsAsync(int id, CancellationToken ct = default);

        // keep signature consistent with base
        new Task<int> SaveAsync(CancellationToken ct = default);
    }
}
