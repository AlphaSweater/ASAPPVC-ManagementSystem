namespace ASAPPVC.App.Models
{
    #region Interface

    /// <summary>
    /// Converts between <see cref="OrderProduct"/> bridge entities and their ViewModels
    /// (<see cref="CreateOrderProductVm"/>, <see cref="EditOrderProductVm"/>, <see cref="OrderProductVm"/>).
    /// Provides bulk helpers for merging duplicates and normalizing quantities.
    /// </summary>
    public interface IOrderProductMapper
    {
        /// <summary>
        /// Converts a single OrderProduct to a view model.
        /// </summary>
        OrderProductVm ToBridgeVm(OrderProduct orderProduct);

        /// <summary>
        /// Converts a collection of OrderProduct entities to view models.
        /// </summary>
        List<OrderProductVm> ToBridgeVms(IEnumerable<OrderProduct> items);

        /// <summary>
        /// Creates a new OrderProduct from a create view model.
        /// </summary>
        OrderProduct FromCreateBridgeVm(Guid orderId, OrderProductFormVm vm);

        /// <summary>
        /// Bulk helper: merges duplicates (by ProductId) and creates OrderProduct rows.
        /// Automatically sums quantities.
        /// </summary>
        List<OrderProduct> FromCreateBridgeVms(Guid orderId, IEnumerable<OrderProductFormVm> items);

        /// <summary>
        /// Applies an edit view model to an existing OrderProduct.
        /// </summary>
        void ApplyUpdateBridgeVm(OrderProduct target, OrderProductFormVm vm);

        /// <summary>
        /// Bulk helper: applies edit view models to existing order products.
        /// Updates existing lines, creates missing ones, and merges duplicates by ProductId.
        /// </summary>
        List<OrderProduct> ApplyUpdateBridgeVms(IEnumerable<OrderProduct> existingProducts, IEnumerable<OrderProductFormVm> editVms);
    }

    #endregion Interface

    /// <summary>
    /// Implementation of <see cref="IOrderProductMapper"/>.
    /// </summary>
    public class OrderProductMapper : IOrderProductMapper
    {
        // ------------------------------------------------------------
        // Domain → VM
        // ------------------------------------------------------------

        public OrderProductVm ToBridgeVm(OrderProduct orderProduct)
        {
            ArgumentNullException.ThrowIfNull(orderProduct);

            return new OrderProductVm
            {
                OrderId = orderProduct.OrderId,
                ProductId = orderProduct.ProductId,
                ProductCode = orderProduct.Product?.ProductCode ?? string.Empty,
                ProductName = orderProduct.Product?.Name ?? string.Empty,
                UnitPrice = orderProduct.Product?.Price ?? 0m,
                Quantity = orderProduct.Quantity
            };
        }

        public List<OrderProductVm> ToBridgeVms(IEnumerable<OrderProduct> items)
        {
            if (items is null)
                return new();

            return items.Select(ToBridgeVm).ToList();
        }

        // ------------------------------------------------------------
        // Create VM → Domain
        // ------------------------------------------------------------

        public OrderProduct FromCreateBridgeVm(Guid orderId, OrderProductFormVm vm)
        {
            ArgumentNullException.ThrowIfNull(vm);

            return new OrderProduct
            {
                OrderId = orderId,
                ProductId = vm.ProductId,
                Quantity = NormalizeQuantity(vm.Quantity)
            };
        }

        public List<OrderProduct> FromCreateBridgeVms(Guid orderId, IEnumerable<OrderProductFormVm> items)
        {
            return (items ?? Enumerable.Empty<OrderProductFormVm>())
            .Where(i => !i.Remove) // drop lines marked for removal
            .GroupBy(i => i.ProductId)
            .Select(g => new OrderProductFormVm
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .Select(vm => FromCreateBridgeVm(orderId, vm))
            .ToList();
        }

        // ------------------------------------------------------------
        // Edit VM → Domain (apply to existing)
        // ------------------------------------------------------------

        public void ApplyUpdateBridgeVm(OrderProduct target, OrderProductFormVm vm)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);

            target.ProductId = vm.ProductId;
            target.Quantity = NormalizeQuantity(vm.Quantity);
        }

        public List<OrderProduct> ApplyUpdateBridgeVms(IEnumerable<OrderProduct> existingProducts, IEnumerable<OrderProductFormVm> editVms)
        {
            var existingByProduct = (existingProducts ?? Enumerable.Empty<OrderProduct>())
            .ToDictionary(x => x.ProductId, x => x);

            return (editVms ?? Enumerable.Empty<OrderProductFormVm>())
            .Where(i => !i.Remove) // drop lines marked for removal
            .GroupBy(vm => vm.ProductId)
            .Select(g => new OrderProductFormVm
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .Select(vm =>
            {
                var line = existingByProduct.TryGetValue(vm.ProductId, out var existing)
                     ? existing
                     : new OrderProduct();

                ApplyUpdateBridgeVm(line, vm);
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