namespace ASAPPVC.UI.Services;

using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Order;

public interface IOrderService
{
    Task<(bool Ok, string? Error, OrderModel? Order)> CreateAsync(CreateOrderViewModel vm, CancellationToken ct = default);
    Task<OrderModel?> GetAsync(Guid id, CancellationToken ct = default);
    Task<List<OrderModel>> ListAsync(CancellationToken ct = default);
}