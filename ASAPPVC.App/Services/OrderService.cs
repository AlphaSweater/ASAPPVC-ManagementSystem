using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Utils;

namespace ASAPPVC.App.Services
{
    #region Interface

    /// <summary>
    /// Service contract for order-related business logic.
    /// Provides higher-level operations that orchestrate validation, normalization, mapping,
    /// and repository interactions for <see cref="Order"/> instances.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order from the supplied form view model.
        /// The service validates input, maps the VM to a domain entity using the order mapper,
        /// verifies customer and product existence, and delegates persistence to the repository.
        /// </summary>
        /// <param name="vm">Form view model containing order details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the created <see cref="Order"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Order>> CreateAsync(OrderFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing order from the supplied form view model.
        /// The service validates input, fetches the existing order, applies the form VM using the mapper,
        /// and saves changes to the repository.
        /// </summary>
        /// <param name="vm">Form view model containing updated order details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the updated <see cref="Order"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Order>> UpdateAsync(OrderFormVm vm, CancellationToken ct = default);

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
        /// Loads the full domain graph for a single order (aggregate root) by Id:
        /// Order → Customer, OrderProducts → Product → ProductComponents → Component.
        /// </summary>
        Task<Result<Order>> GetFullDomainAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the full list of orders mapped to lightweight list view models.
        /// Suitable for display in tables, cards, or summary views.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="OrderListVm"/> instances.
        /// </returns>
        Task<Result<List<OrderListVm>>> ListAsync(CancellationToken ct = default);

        /// <summary>
        /// Searches orders using a free-text term against order code and customer information.
        /// </summary>
        /// <param name="term">
        /// The search term, which may match part of the order code or customer name/email.
        /// Case-insensitive. If null or empty, all orders are returned.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="OrderListVm"/> instances that match the search criteria.
        /// </returns>
        Task<Result<List<OrderListVm>>> SearchAsync(string? term, CancellationToken ct = default);

        /// <summary>
        /// Deletes an order by its internal identifier.
        /// </summary>
        /// <param name="id">The internal GUID identifier of the order to delete.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating success or failure with an error message.
        /// </returns>
        Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    }

    #endregion Interface

    public class OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IOrderMapper orderMapper) : IOrderService
    {
        private readonly IOrderRepository _orders = orderRepository;
        private readonly ICustomerRepository _customers = customerRepository;
        private readonly IProductRepository _products = productRepository;
        private readonly IOrderMapper _mapper = orderMapper;

        // ---------- Input validation + normalization helpers ----------

        private static Result ValidateCreateVm(OrderFormVm? vm)
        {
            if (vm is null)
                return Result.Fail("Create view model is required.");
            if (vm.CustomerId == Guid.Empty)
                return Result.Fail("Customer is required.");
            if (vm.Products is null || vm.Products.Count == 0)
                return Result.Fail("An order requires at least one product line.");

            // Validate product entries
            foreach (var product in vm.Products)
            {
                if (product.ProductId == Guid.Empty)
                    return Result.Fail("All products must have valid IDs.");
                if (product.Quantity < 1)
                    return Result.Fail("Product quantities must be at least 1.");
            }

            return Result.Success();
        }

        private static Result ValidateEditVm(OrderFormVm? vm)
        {
            if (vm is null)
                return Result.Fail("Edit view model is required.");
            if (vm.Id is null || vm.Id == Guid.Empty)
                return Result.Fail("Order ID is required.");
            if (string.IsNullOrWhiteSpace(vm.OrderCode))
                return Result.Fail("Order code is required.");
            if (vm.CustomerId == Guid.Empty)
                return Result.Fail("Customer is required.");
            if (vm.Products is null || vm.Products.Count == 0)
                return Result.Fail("An order requires at least one product line.");

            // Validate product entries
            foreach (var product in vm.Products)
            {
                if (product.ProductId == Guid.Empty)
                    return Result.Fail("All products must have valid IDs.");
                if (product.Quantity < 1)
                    return Result.Fail("Product quantities must be at least 1.");
            }

            return Result.Success();
        }

        private static void NormalizeCreate(OrderFormVm vm)
        {
            // Ensure order date is set
            if (vm.OrderDate is null)
                vm.OrderDate = DateTime.UtcNow;

            // Normalize notes if provided
            if (!string.IsNullOrWhiteSpace(vm.Notes))
                vm.Notes = vm.Notes.Trim();
        }

        private static void NormalizeEdit(OrderFormVm vm)
        {
            if (vm.OrderCode is not null)
                vm.OrderCode = vm.OrderCode.Trim();

            // Normalize notes if provided
            if (!string.IsNullOrWhiteSpace(vm.Notes))
                vm.Notes = vm.Notes.Trim();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Creates a new order from view model
        public async Task<Result<Order>> CreateAsync(OrderFormVm vm, CancellationToken ct = default)
        {
            var validation = ValidateCreateVm(vm);
            if (!validation.Ok)
                return Result<Order>.Fail(validation.Error!);

            NormalizeCreate(vm);

            try
            {
                // Verify customer exists
                var customer = await _customers.GetByIdAsync(vm.CustomerId, asNoTracking: true, ct);
                if (customer is null)
                    return Result<Order>.Fail($"Customer with ID '{vm.CustomerId}' does not exist.");

                // Verify all products exist
                var productIds = vm.Products.Select(p => p.ProductId).Distinct().ToList();
                var products = await _products.GetListByIdsAsync(productIds, asNoTracking: true, ct);
                var foundProductIds = products.Select(p => p.Id).ToHashSet();

                var missingIds = productIds.Where(id => !foundProductIds.Contains(id)).ToList();
                if (missingIds.Any())
                    return Result<Order>.Fail($"Some products do not exist: {string.Join(", ", missingIds)}");

                // Map VM to domain entity (mapper handles code generation and order line creation)
                var order = _mapper.FromFormVm(vm);

                // Persist
                var added = await _orders.AddAsync(order, ct);
                await _orders.SaveAsync(ct);

                return Result<Order>.Success(added);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<Order>.Fail("Operation was canceled.");
            }
            catch (Exception ex)
            {
                return Result<Order>.Fail($"Failed to create order: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Updates an existing order from edit view model
        public async Task<Result<Order>> UpdateAsync(OrderFormVm vm, CancellationToken ct = default)
        {
            var validation = ValidateEditVm(vm);
            if (!validation.Ok)
                return Result<Order>.Fail(validation.Error!);

            NormalizeEdit(vm);

            try
            {
                // Fetch existing order with details (tracking enabled for update)
                var existing = await _orders.GetByIdOrCodeWithDetailsAsync(vm.Id!.Value, asNoTracking: false, ct: ct);
                if (existing is null)
                    return Result<Order>.Fail("Order not found.");

                // Check if code changed and conflicts with another order
                if (existing.OrderCode != vm.OrderCode)
                {
                    var conflictingOrder = await _orders.GetByCodeAsync(vm.OrderCode!, asNoTracking: true, ct);
                    if (conflictingOrder is not null && conflictingOrder.Id != vm.Id)
                        return Result<Order>.Fail($"Order code '{vm.OrderCode}' is already in use.");
                }

                // Verify customer exists
                var customer = await _customers.GetByIdAsync(vm.CustomerId, asNoTracking: true, ct);
                if (customer is null)
                    return Result<Order>.Fail($"Customer with ID '{vm.CustomerId}' does not exist.");

                // Verify all products exist
                var productIds = vm.Products.Select(p => p.ProductId).Distinct().ToList();
                var products = await _products.GetListByIdsAsync(productIds, asNoTracking: true, ct);
                var foundProductIds = products.Select(p => p.Id).ToHashSet();

                var missingIds = productIds.Where(id => !foundProductIds.Contains(id)).ToList();
                if (missingIds.Any())
                    return Result<Order>.Fail($"Some products do not exist: {string.Join(", ", missingIds)}");

                // Apply changes via mapper (handles property updates and OrderProducts reconciliation)
                _mapper.ApplyUpdate(existing, vm);

                // Persist changes
                _orders.Update(existing);
                await _orders.SaveAsync(ct);

                return Result<Order>.Success(existing);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<Order>.Fail("Operation was canceled.");
            }
            catch (Exception ex)
            {
                return Result<Order>.Fail($"Failed to update order: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a single order detail (mapped to detail VM)
        public async Task<Result<OrderDetailVm>> GetDetailAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                return Result<OrderDetailVm>.Fail("Order ID is required.");

            try
            {
                var order = await _orders.GetByIdOrCodeWithDetailsAsync(id, asNoTracking: true, ct: ct);
                if (order is null)
                    return Result<OrderDetailVm>.Fail("Order not found.");

                var detailVm = _mapper.ToDetailVm(order, includeProducts: true);
                return Result<OrderDetailVm>.Success(detailVm);
            }
            catch (Exception ex)
            {
                return Result<OrderDetailVm>.Fail($"Failed to retrieve order detail: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a single order entity (raw domain model)
        // TODO: FIX ME PLEASE I AM BEGGING OH GOD THE AGONY
        public async Task<Result<Order>> GetDomainAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                return Result<Order>.Fail("Order ID is required.");

            try
            {
                var order = await _orders.GetByIdOrCodeWithDetailsAsync(id, asNoTracking: true, ct: ct);
                if (order is null)
                    return Result<Order>.Fail("Order not found.");

                return Result<Order>.Success(order);
            }
            catch (Exception ex)
            {
                return Result<Order>.Fail($"Failed to retrieve order: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves the full domain graph for an order
        public async Task<Result<Order>> GetFullDomainAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                return Result<Order>.Fail("Order ID is required.");

            try
            {
                var order = await _orders.GetFullDomainAsync(id, asNoTracking: true, ct: ct);

                if (order is null)
                    return Result<Order>.Fail("Order not found.");

                return Result<Order>.Success(order);
            }
            catch (Exception ex)
            {
                return Result<Order>.Fail($"Failed to retrieve order: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves full order list (mapped to list VMs)
        public async Task<Result<List<OrderListVm>>> ListAsync(CancellationToken ct = default)
        {
            try
            {
                var orders = await _orders.GetListWithDetailsAsync(asNoTracking: true, ct);
                var listVms = orders.Select(o => _mapper.ToListVm(o)).ToList();
                return Result<List<OrderListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<OrderListVm>>.Fail($"Failed to list orders: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Searches orders by term (order code, customer name/email, case-insensitive)
        public async Task<Result<List<OrderListVm>>> SearchAsync(string? term, CancellationToken ct = default)
        {
            try
            {
                term ??= string.Empty;
                var orders = await _orders.SearchAsync(term, asNoTracking: true, ct);
                var listVms = orders.Select(o => _mapper.ToListVm(o)).ToList();
                return Result<List<OrderListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<OrderListVm>>.Fail($"Failed to search orders: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Deletes an order by ID
        public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                return Result.Fail("Order ID is required.");

            try
            {
                var removed = await _orders.RemoveByIdAsync(id, ct);
                if (!removed)
                    return Result.Fail("Order not found.");

                await _orders.SaveAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to delete order: {ex.Message}");
            }
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\