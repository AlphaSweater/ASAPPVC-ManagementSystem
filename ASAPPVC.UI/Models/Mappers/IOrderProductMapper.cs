namespace ASAPPVC.UI.Models.Mappers
{
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
}