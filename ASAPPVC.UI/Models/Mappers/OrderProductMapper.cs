namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Implementation of <see cref="IOrderProductMapper"/>.
    /// </summary>
    public class OrderProductMapper : IOrderProductMapper
    {
        // ------------------------------------------------------------
        // Domain → VM
        // ------------------------------------------------------------

        public OrderProductVm ToVm(OrderProduct orderProduct)
        {
            ArgumentNullException.ThrowIfNull(orderProduct);

            return new OrderProductVm
            {
                Id = orderProduct.Id,
                OrderId = orderProduct.OrderId,
                ProductId = orderProduct.ProductId,
                ProductCode = orderProduct.Product?.ProductCode ?? string.Empty,
                ProductName = orderProduct.Product?.Name ?? string.Empty,
                UnitPrice = orderProduct.Product?.Price ?? 0m,
                Quantity = orderProduct.Quantity
            };
        }

        public List<OrderProductVm> ToVms(IEnumerable<OrderProduct> items)
        {
            if (items is null)
                return new();

            return items.Select(ToVm).ToList();
        }

        // ------------------------------------------------------------
        // Create VM → Domain
        // ------------------------------------------------------------

        public OrderProduct FromCreateVm(Guid orderId, CreateOrderProductVm vm)
        {
            ArgumentNullException.ThrowIfNull(vm);

            return new OrderProduct
            {
                OrderId = orderId,
                ProductId = vm.ProductId,
                Quantity = NormalizeQuantity(vm.Quantity)
            };
        }

        public List<OrderProduct> FromCreateVms(Guid orderId, IEnumerable<CreateOrderProductVm> items)
        {
            return (items ?? Enumerable.Empty<CreateOrderProductVm>())
            .GroupBy(i => i.ProductId)
            .Select(g => new CreateOrderProductVm
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .Select(vm => FromCreateVm(orderId, vm))
            .ToList();
        }

        // ------------------------------------------------------------
        // Edit VM → Domain (apply to existing)
        // ------------------------------------------------------------

        public void ApplyEditVm(OrderProduct target, EditOrderProductVm vm)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);

            target.ProductId = vm.ProductId;
            target.Quantity = NormalizeQuantity(vm.Quantity);

            // No other fields to update on OrderProduct; price is taken from Product when mapping to VM.
            // If product changed, the associated Product navigation may be stale and should be reloaded by caller if needed.
        }

        public List<OrderProduct> ApplyEditVms(IEnumerable<OrderProduct> existingProducts, IEnumerable<EditOrderProductVm> editVms)
        {
            var existingByProduct = (existingProducts ?? Enumerable.Empty<OrderProduct>())
            .ToDictionary(x => x.ProductId, x => x);

            return (editVms ?? Enumerable.Empty<EditOrderProductVm>())
            .GroupBy(vm => vm.ProductId)
            .Select(g => new EditOrderProductVm
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .Select(vm =>
            {
                var line = existingByProduct.TryGetValue(vm.ProductId, out var existing)
                     ? existing
                     : new OrderProduct();

                ApplyEditVm(line, vm);
                return line;
            })
            .ToList();
        }

        // ------------------------------------------------------------
        // Utilities
        // ------------------------------------------------------------

        private static int NormalizeQuantity(int q)
        {
            return q < 1 ? 1 : q;
        }
    }
}