using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Order;
using ASAPPVC.UI.Repositories;

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
        public async Task<(bool Ok, string? Error, Order? Order)> CreateAsync(CreateOrderViewModel vm, CancellationToken ct = default)
        {
            //validates input and returns an error message if invalid
            if (vm == null)
                return (false, "Request body is required.", null);

            if (vm.ProductQuantities == null || vm.ProductQuantities.Count == 0)
                return (false, "Please select at least one product.", null);

            //Create main order
            var order = new Order
            {
                CustomerId = vm.CustomerId,
                OrderDate = vm.OrderDate ?? DateTime.UtcNow
            };

            // Add order to repository and ensure we have the persisted Id (GUID)
            var addedOrder = await _repo.AddAsync(order, ct);
            await _repo.SaveAsync(ct);

            // Build order lines: group by product id and sum quantities in case duplicates were submitted
            var orderLines = vm.ProductQuantities
                .Where(pq => pq != null && pq.ProductId != Guid.Empty && pq.Quantity > 0)
                .GroupBy(pq => pq.ProductId)
                .Select(g => new OrderProductModel
                {
                    OrderId = addedOrder.Id,
                    ProductId = g.Key,
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
        public async Task<Order?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _repo.GetWithDetailsAsync(id, ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves a list of orders with customer and products
        public async Task<List<Order>> ListAsync(CancellationToken ct = default)
        {
            return await _repo.ListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\