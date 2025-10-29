namespace ASAPPVC.App.Models.Mappers
{
    /// <summary>
    /// Converts between <see cref="Order"/> domain entities and view models
    /// defined in `Order.vm.cs` (e.g. <see cref="OrderListVm"/>, <see cref="OrderDetailVm"/>).
    /// Delegates bridge line mapping to <see cref="IOrderProductMapper"/>.
    /// </summary>
    public interface IOrderMapper
    {
        /// <summary>
        /// Converts an Order to a lightweight list VM for dashboards/grids.
        /// </summary>
        OrderListVm ToListVm(Order order);

        /// <summary>
        /// Converts an Order to a full detail VM with optional product lines.
        /// </summary>
        OrderDetailVm ToDetailVm(Order order, bool includeProducts = true);

        /// <summary>
        /// Builds an Order domain entity from the unified order form VM. Generates an order code
        /// if not supplied using the optional generator.
        /// </summary>
        Order FromFormVm(OrderFormVm vm);

        /// <summary>
        /// Applies an upsert form VM to an existing Order domain entity.
        /// Updates scalar fields and delegates product-line merging to <see cref="IOrderProductMapper"/>.
        /// Returns the updated Order instance (same reference as <paramref name="existing"/>).
        /// </summary>
        Order ApplyFormVm(Order existing, OrderFormVm vm);
    }
}