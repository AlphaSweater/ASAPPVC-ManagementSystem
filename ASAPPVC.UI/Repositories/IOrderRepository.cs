using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    /// <summary>
    /// Repository interface for order-specific operations.
    /// Extends <see cref="IBaseRepository{T}"/> with order-focused helpers such as
    /// lookups by id or code, customer/product-inclusive retrievals, and search capabilities.
    /// </summary>
    public interface IOrderRepository : IBaseRepository<Order>
    {
        /// <summary>
        /// Retrieves an order by its internal <paramref name="id"/> or by its human-friendly <paramref name="code"/>.<br/>
        /// Implementations should prefer <paramref name="id"/> when a valid non-empty guid is provided and<br/>
        /// fallback to <paramref name="code"/> when no valid id is available.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the order.</param>
        /// <param name="code">Optional human-friendly order code.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>The matching <see cref="Order"/> if found; otherwise <c>null</c>.</returns>
        Task<Order?> GetByIdOrCodeAsync(Guid? id = null, string? code = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves an order including its <see cref="Order.Customer"/>, <see cref="Order.OrderProducts"/>,<br/>
        /// and related product entities by either <paramref name="id"/> or <paramref name="code"/>.<br/>
        /// Useful for display/edit views that need the full order context populated.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the order.</param>
        /// <param name="code">Optional human-friendly order code.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>The matching <see cref="Order"/> with details loaded if found; otherwise <c>null</c>.</returns>
        Task<Order?> GetByIdOrCodeWithDetailsAsync(Guid? id = null, string? code = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple orders by either a collection of <paramref name="ids"/> or a collection of <paramref name="codes"/>.<br/>
        /// Implementations may prefer ids when any valid guid is present and fallback to codes otherwise.<br/>
        /// </summary>
        /// <param name="ids">Optional collection of order ids to retrieve.</param>
        /// <param name="codes">Optional collection of order codes to retrieve.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Order"/> instances; an empty list if none found.</returns>
        Task<List<Order>> GetListByIdsOrCodesAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all orders with customer and order product details included, ordered by <see cref="Order.OrderDate"/> descending.<br/>
        /// Intended for lookup and display scenarios where deterministic ordering is useful.<br/>
        /// </summary>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>Ordered list of all <see cref="Order"/> instances with details loaded.</returns>
        Task<List<Order>> GetListWithDetailsAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all orders ordered by <see cref="Order.OrderDate"/> descending.<br/>
        /// Intended for lookup and display scenarios where deterministic ordering is useful.<br/>
        /// </summary>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>Ordered list of all <see cref="Order"/> instances.</returns>
        Task<List<Order>> GetListAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Performs a simple search for orders using <paramref name="term"/> against <see cref="Order.OrderCode"/>,<br/>
        /// customer name, and related fields. When <paramref name="term"/> is null or whitespace the<br/>
        /// implementation may return the full ordered list.<br/>
        /// </summary>
        /// <param name="term">Search term to match against order code or customer information.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Order"/> instances; an empty list if none match.</returns>
        Task<List<Order>> SearchAsync(string term, bool asNoTracking = true, CancellationToken ct = default);
    }
}