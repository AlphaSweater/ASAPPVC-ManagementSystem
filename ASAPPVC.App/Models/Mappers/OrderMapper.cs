using ASAPPVC.App.Services;

namespace ASAPPVC.App.Models.Mappers
{
    /// <summary>
    /// Implementation of <see cref="IOrderMapper"/>. Inherits shared helpers from <see cref="MapperBase"/>.
    /// </summary>
    public class OrderMapper(
        IOrderProductMapper orderProductMapper,
        ICodeGenerationService? codeGenerationService = null,
        IImageService? imageService = null) : MapperBase(codeGenerationService, imageService), IOrderMapper
    {
        private readonly IOrderProductMapper _orderProductMapper = orderProductMapper ?? throw new ArgumentNullException(nameof(orderProductMapper));

        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        public OrderListVm ToListVm(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            var lines = order.OrderProducts ?? Enumerable.Empty<OrderProduct>();

            var itemCount = lines.Sum(x => x.Quantity);
            var total = lines.Sum(x => (x.Product?.Price ?? 0m) * x.Quantity);

            return new OrderListVm
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                CustomerName = (order.Customer is null) ? string.Empty : $"{order.Customer.Name} {order.Customer.Surname}".Trim(),
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                ItemCount = itemCount,
                TotalAmount = total
            };
        }

        public OrderDetailVm ToDetailVm(Order order, bool includeProducts = true)
        {
            ArgumentNullException.ThrowIfNull(order);

            var lines = order.OrderProducts ?? Enumerable.Empty<OrderProduct>();
            var products = includeProducts ? _orderProductMapper.ToBridgeVms(lines) : new List<OrderProductVm>();

            var itemCount = lines.Sum(x => x.Quantity);
            var subtotal = lines.Sum(x => (x.Product?.Price ?? 0m) * x.Quantity);
            var tax = 0m; // keep zero for now — compute later if needed
            var grand = subtotal + tax;

            return new OrderDetailVm
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                CustomerName = (order.Customer is null) ? string.Empty : $"{order.Customer.Name} {order.Customer.Surname}".Trim(),
                CustomerEmail = order.Customer?.Email,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                Products = products,
                ItemCount = itemCount,
                Subtotal = subtotal,
                TaxAmount = tax,
                GrandTotal = grand
            };
        }

        // ------------------------------------------------------------
        // Create ViewModel → Domain
        // ------------------------------------------------------------

        public Order FromFormVm(OrderFormVm vm)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var order = new Order
            {
                OrderCode = NormalizeCodeOrGenerate(null, "ORD"),
                CustomerId = vm.CustomerId,
                OrderDate = vm.OrderDate ?? DateTime.UtcNow,
                OrderStatus = vm.OrderStatus
            };

            // Use the bridge create helper and current form VM type
            order.OrderProducts = _orderProductMapper.FromCreateBridgeVms(order.Id, vm.Products ?? Enumerable.Empty<OrderProductFormVm>());

            return order;
        }

        // ------------------------------------------------------------
        // Update existing domain entity from Edit VM
        // ------------------------------------------------------------

        public Order ApplyFormVm(Order existing, OrderFormVm vm)
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(vm);

            if (existing.Id != vm.Id)
                throw new InvalidOperationException("Mismatched order Id.");

            // Scalars
            existing.OrderCode = NormalizeString(vm.OrderCode);
            existing.CustomerId = vm.CustomerId;
            existing.OrderDate = vm.OrderDate ?? existing.OrderDate;
            existing.OrderStatus = vm.OrderStatus;

            // Merge/update bridge lines using the dedicated mapper helper
            var updatedLines = _orderProductMapper.ApplyUpdateBridgeVms(existing.OrderProducts ?? new List<OrderProduct>(), vm.Products ?? Enumerable.Empty<OrderProductFormVm>());

            // Ensure correct OrderId on all lines and replace collection
            foreach (var line in updatedLines)
            {
                line.OrderId = existing.Id;
            }

            existing.OrderProducts = updatedLines;

            return existing;
        }
    }
}