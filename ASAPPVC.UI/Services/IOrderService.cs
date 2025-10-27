namespace ASAPPVC.UI.Services;

using ASAPPVC.UI.Models;
using ASAPPVC.UI.ViewModels.Order;

public interface IOrderService
{
    Task<(bool Ok, string? Error, Order? Order)> CreateAsync(CreateOrderViewModel vm, CancellationToken ct = default);
    Task<Order?> GetAsync(Guid id, CancellationToken ct = default);
    Task<List<Order>> ListAsync(CancellationToken ct = default);
}