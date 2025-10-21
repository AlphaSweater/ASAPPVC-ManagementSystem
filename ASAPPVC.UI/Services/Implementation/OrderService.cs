using ASAPPVC.UI.Models.ViewModels.Order;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using ASAPPVC.UI.Services.Interfaces;

namespace ASAPPVC.UI.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<(bool Ok, string? Error, OrderModel? Order)> CreateAsync(CreateOrderViewModel vm, CancellationToken ct = default)
        {
            if (vm.CustomerID <= 0)
                return (false, "Customer selection is required.", null);

            if (vm.ProductIDs == null || vm.ProductIDs.Count == 0)
                return (false, "Please select at least one product.", null);

            if (string.IsNullOrWhiteSpace(vm.OrderStatus))
                vm.OrderStatus = "Pending";

            //Create main order
            var order = new OrderModel
            {
                CustomerID = vm.CustomerID,
                OrderStatus = vm.OrderStatus.Trim(),
                OrderDate = vm.OrderDate ?? DateTime.UtcNow
            };

            await _repo.AddOrderAsync(order, ct);
            await _repo.SaveAsync(ct);

            //Add order products
            var orderLines = vm.ProductIDs
                .Distinct()
                .Select(id => new OrderProductModel
                {
                    OrderID = order.OrderID,
                    ProductID = id
                })
                .ToList();

            await _repo.AddOrderProductsAsync(orderLines, ct);
            await _repo.SaveAsync(ct);

            return (true, null, order);
        }

        public async Task<OrderModel?> GetAsync(int id, CancellationToken ct = default)
        {
            return await _repo.GetWithDetailsAsync(id, ct);
        }

        public async Task<List<OrderModel>> ListAsync(CancellationToken ct = default)
        {
            return await _repo.ListAsync(ct);
        }
    }
}
