namespace ASAPPVC.UI.Services;

using ASAPPVC.UI.Models;
using ASAPPVC.UI.Utils;

/// <summary>
/// Service contract for order-related business logic.
/// Provides higher-level operations that orchestrate validation, normalization, mapping,
/// and repository interactions for <see cref="Order"/> instances.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order from the supplied view model.
    /// The service validates input, maps the VM to a domain entity using the order mapper,
    /// verifies customer and product existence, and delegates persistence to the repository.
    /// </summary>
    /// <param name="vm">Create view model containing order details. Must not be null.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> carrying the created <see cref="Order"/> on success
    /// or an error message on failure.
    /// </returns>
    Task<Result<Order>> CreateAsync(CreateOrderVm vm, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing order from the supplied edit view model.
    /// The service validates input, fetches the existing order, applies the edit VM using the mapper,
    /// and saves changes to the repository.
    /// </summary>
    /// <param name="vm">Edit view model containing updated order details. Must not be null.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> carrying the updated <see cref="Order"/> on success
    /// or an error message on failure.
    /// </returns>
    Task<Result<Order>> UpdateAsync(EditOrderVm vm, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single order by its internal identifier.
    /// Returns the order mapped to a detail view model suitable for display.
    /// </summary>
    /// <param name="id">Internal GUID identifier of the order.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the found <see cref="OrderDetailVm"/>, or a failure result
    /// if the order does not exist or an error occurs.
    /// </returns>
    Task<Result<OrderDetailVm>> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single order entity (not mapped) by its internal identifier.
    /// Useful for operations that need the raw domain entity.
    /// </summary>
    /// <param name="id">Internal GUID identifier of the order.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the found <see cref="Order"/>, or a failure result
    /// if the order does not exist or an error occurs.
    /// </returns>
    Task<Result<Order>> GetDomainAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the full list of orders mapped to lightweight list view models.
    /// Suitable for display in tables, cards, or summary views.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a list of <see cref="OrderListVm"/> instances.
    /// </returns>
    Task<Result<List<OrderListVm>>> ListAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes an order by its internal identifier.
    /// </summary>
    /// <param name="id">The internal GUID identifier of the order to delete.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success or failure with an error message.
    /// </returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}