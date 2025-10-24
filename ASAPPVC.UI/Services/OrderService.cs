using ASAPPVC.UI.Models.ViewModels.Order;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using System.Linq;

namespace ASAPPVC.UI.Services
{
    public class OrderService : IOrderService
    {
        //─────────── Dependencies ───────────\\
        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //creates a new order with associated products
        public async Task<(bool Ok, string? Error, OrderModel? Order)> CreateAsync(CreateOrderViewModel vm, CancellationToken ct = default)
        {
            //validates input and returns an error message if invalid
            if (vm == null)
                return (false, "Request body is required.", null);

            if (vm.CustomerId <= 0)
                return (false, "Customer selection is required.", null);

            if (vm.ProductQuantities == null || vm.ProductQuantities.Count == 0)
                return (false, "Please select at least one product.", null);

            if (string.IsNullOrWhiteSpace(vm.OrderStatus))
                vm.OrderStatus = "Pending";

            //Create main order
            var order = new OrderModel
            {
                CustomerID = vm.CustomerId,
                OrderStatus = vm.OrderStatus.Trim(),
                OrderDate = vm.OrderDate ?? DateTime.UtcNow
            };

            // Add order to repository and ensure we have the persisted OrderID
            var addedOrder = await _repo.AddOrderAsync(order, ct);
            await _repo.SaveAsync(ct);

            // Build order lines: group by product id and sum quantities in case duplicates were submitted
            var orderLines = vm.ProductQuantities
                .Where(pq => pq != null && pq.ProductId > 0 && pq.Quantity > 0)
                .GroupBy(pq => pq.ProductId)
                .Select(g => new OrderProductModel
                {
                    OrderID = addedOrder.OrderID,
                    ProductID = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            if (orderLines.Count == 0)
            {
                return (false, "No valid products provided.", addedOrder);
            }

            await _repo.AddOrderProductsAsync(orderLines, ct);
            await _repo.SaveAsync(ct);

            return (true, null, addedOrder);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves a single order with details by ID
        public async Task<OrderModel?> GetAsync(int id, CancellationToken ct = default)
        {
            return await _repo.GetWithDetailsAsync(id, ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves a list of orders with customer and products
        public async Task<List<OrderModel>> ListAsync(CancellationToken ct = default)
        {
            return await _repo.ListAsync(ct);
        }
    }
}
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\