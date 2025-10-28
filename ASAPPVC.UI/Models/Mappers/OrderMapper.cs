using ASAPPVC.UI.Services;

namespace ASAPPVC.UI.Models.Mappers
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
            var products = includeProducts ? _orderProductMapper.ToVms(lines) : new List<OrderProductVm>();

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

        public Order FromCreateVm(CreateOrderVm vm)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var order = new Order
            {
                OrderCode = NormalizeCodeOrGenerate(null, "ORD"),
                CustomerId = vm.CustomerId,
                OrderDate = vm.OrderDate ?? DateTime.UtcNow,
                OrderStatus = vm.OrderStatus
            };

            order.OrderProducts = _orderProductMapper.FromCreateVms(order.Id, vm.Products ?? Enumerable.Empty<CreateOrderProductVm>());

            return order;
        }
    }
}