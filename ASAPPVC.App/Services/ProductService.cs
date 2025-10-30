using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Utils;

namespace ASAPPVC.App.Services
{
    #region Interface

    /// <summary>
    /// Service contract for product-related business logic.
    /// Provides higher-level operations that orchestrate validation, normalization, mapping,
    /// and repository interactions for <see cref="Product"/> instances.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Creates a new product from the supplied view model.
        /// The service validates input, maps the VM to a domain entity using the product mapper,
        /// resolves component units, and delegates persistence to the repository.
        /// </summary>
        /// <param name="vm">Create view model containing product details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the created <see cref="Product"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Product>> CreateAsync(ProductFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing product from the supplied edit view model.
        /// The service validates input, fetches the existing product, applies the edit VM using the mapper,
        /// and saves changes to the repository.
        /// </summary>
        /// <param name="vm">Edit view model containing updated product details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the updated <see cref="Product"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Product>> UpdateAsync(ProductFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single product by its internal identifier or by its human-friendly code.
        /// Returns the product mapped to a detail view model suitable for display.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="ProductDetailVm"/>, or a failure result
        /// if the product does not exist or an error occurs.
        /// </returns>
        Task<Result<ProductDetailVm>> GetDetailAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single product entity (not mapped) by its internal identifier or by its human-friendly code.
        /// Useful for operations that need the raw domain entity.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="Product"/>, or a failure result
        /// if the product does not exist or an error occurs.
        /// </returns>
        Task<Result<Product>> GetDomainAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the full list of products mapped to lightweight list view models.
        /// Suitable for display in tables, cards, or summary views.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="ProductListVm"/> instances.
        /// </returns>
        Task<Result<List<ProductListVm>>> ListAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple products by a collection of internal ids and/or human-friendly codes.
        /// Returns products mapped to list view models.
        /// </summary>
        /// <param name="ids">Optional collection of internal product ids to retrieve.</param>
        /// <param name="codes">Optional collection of human-friendly product codes to retrieve.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of matching <see cref="ProductListVm"/> instances.
        /// </returns>
        Task<Result<List<ProductListVm>>> GetListByIdsOrCodesAsync(
        IEnumerable<Guid>? ids = null,
        IEnumerable<string>? codes = null,
        CancellationToken ct = default);

        /// <summary>
        /// Searches products using a free-text term against name and product code.
        /// Returns matching products mapped to list view models.
        /// </summary>
        /// <param name="term">Search term to match against product properties. Null or empty means no filter.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of products that match the search term;
        /// an empty list indicates no matches.
        /// </returns>
        Task<Result<List<ProductListVm>>> SearchAsync(string? term, CancellationToken ct = default);

        /// <summary>
        /// Deletes a product by its internal identifier.
        /// </summary>
        /// <param name="id">The internal GUID identifier of the product to delete.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating success or failure with an error message.
        /// </returns>
        Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Checks if a product with the given code already exists.
        /// Useful for validation before creating or updating products.
        /// </summary>
        /// <param name="code">The product code to check.</param>
        /// <param name="excludeId">Optional product ID to exclude from the check (useful for updates).</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing true if the code exists, false otherwise.
        /// </returns>
        Task<Result<bool>> ExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    }

    #endregion Interface

    public class ProductService(
        IProductRepository productRepository,
        IComponentRepository componentRepository,
        IProductMapper productMapper) : IProductService
    {
        private readonly IProductRepository _products = productRepository;
        private readonly IComponentRepository _components = componentRepository;
        private readonly IProductMapper _mapper = productMapper;

        // ---------- Input validation + normalization helpers ----------

        private static Result ValidateCreateVm(ProductFormVm? vm)
        {
            if (vm is null)
                return Result.Fail("Create view model is required.");
            if (string.IsNullOrWhiteSpace(vm.ProductName))
                return Result.Fail("Product name is required.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                return Result.Fail("Product description is required.");
            if (vm.SellingPrice <= 0)
                return Result.Fail("Product price must be greater than zero.");
            if (vm.SelectedProductComponents is null || vm.SelectedProductComponents.Count == 0)
                return Result.Fail("A product requires at least one component.");

            // Validate component entries
            foreach (var comp in vm.SelectedProductComponents)
            {
                if (comp.ComponentId == Guid.Empty)
                    return Result.Fail("All components must have valid IDs.");
                if (comp.RequiredQuantity <= 0)
                    return Result.Fail("Component quantities must be greater than zero.");
            }

            return Result.Success();
        }

        private static Result ValidateEditVm(ProductFormVm? vm)
        {
            if (vm is null)
                return Result.Fail("Edit view model is required.");
            if (vm.Id == Guid.Empty)
                return Result.Fail("Product ID is required.");
            if (string.IsNullOrWhiteSpace(vm.ProductName))
                return Result.Fail("Product name is required.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                return Result.Fail("Product description is required.");
            if (string.IsNullOrWhiteSpace(vm.ProductCode))
                return Result.Fail("Product code is required.");
            if (vm.SellingPrice <= 0)
                return Result.Fail("Product price must be greater than zero.");
            if (vm.SelectedProductComponents is null || vm.SelectedProductComponents.Count == 0)
                return Result.Fail("A product requires at least one component.");

            // Validate component entries
            foreach (var comp in vm.SelectedProductComponents)
            {
                if (comp.ComponentId == Guid.Empty)
                    return Result.Fail("All components must have valid IDs.");
                if (comp.RequiredQuantity <= 0)
                    return Result.Fail("Component quantities must be greater than zero.");
            }

            return Result.Success();
        }

        private static void Normalize(ProductFormVm vm)
        {
            vm.ProductName = vm.ProductName.Trim();
            vm.Description = vm.Description.Trim();
            if (!string.IsNullOrWhiteSpace(vm.ProductCode))
                vm.ProductCode = vm.ProductCode.Trim();
        }

        // ---------- Build component lookup helper ----------

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves all active components as a lookup dictionary for product component building
        private async Task<Result<Dictionary<Guid, ProductComponentVm>>> GetComponentLookupAsync(CancellationToken ct = default)
        {
            try
            {
                // Fetch all active components
                var components = await _components.ListAsync(asNoTracking: true, ct: ct);

                // Convert to ProductComponentVm and build dictionary
                var lookup = components.ToDictionary(
                    c => c.Id,
                    c => new ProductComponentVm
                    {
                        ProductId = Guid.Empty, // Not associated with a specific product yet
                        ComponentId = c.Id,
                        ComponentCode = c.ComponentCode,
                        ComponentName = c.ComponentName,
                        UnitCost = c.UnitCost,
                        RequiredQuantity = 1m, // Default quantity
                        UnitOfMeasure = c.UnitOfMeasure,
                        Remove = false,
                        UpdatedAt = DateTime.UtcNow
                    });

                return Result<Dictionary<Guid, ProductComponentVm>>.Success(lookup);
            }
            catch (Exception ex)
            {
                return Result<Dictionary<Guid, ProductComponentVm>>.Fail($"Failed to build component lookup: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Creates a new product from view model
        public async Task<Result<Product>> CreateAsync(ProductFormVm vm, CancellationToken ct = default)
        {
            var validation = ValidateCreateVm(vm);
            if (!validation.Ok)
                return Result<Product>.Fail(validation.Error!);

            Normalize(vm);

            try
            {
                // Map VM to domain entity (mapper handles code generation and component mapping and image processing)
                var product = await _mapper.FromCreateVmAsync(vm, ct);

                // Persist
                var added = await _products.AddAsync(product, ct);
                await _products.SaveAsync(ct);

                return Result<Product>.Success(added);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<Product>.Fail("Operation was canceled.");
            }
            catch (Exception ex)
            {
                return Result<Product>.Fail($"Failed to create product: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Updates an existing product from edit view model
        public async Task<Result<Product>> UpdateAsync(ProductFormVm vm, CancellationToken ct = default)
        {
            var validation = ValidateEditVm(vm);
            if (!validation.Ok)
                return Result<Product>.Fail(validation.Error!);

            Normalize(vm);

            try
            {
                // Fetch existing product with components (tracking enabled for update)
                var existing = await _products.GetByIdOrCodeWithComponentsAsync(vm.Id, null, asNoTracking: false, ct: ct);
                if (existing is null)
                    return Result<Product>.Fail("Product not found.");

                // Check if code changed and conflicts with another product
                if (existing.ProductCode != vm.ProductCode)
                {
                    var existsResult = await ExistsAsync(vm.ProductCode!, excludeId: vm.Id, ct);
                    if (!existsResult.Ok)
                        return Result<Product>.Fail($"Failed to check product code existence: {existsResult.Error}");

                    if (existsResult.Value)
                        return Result<Product>.Fail($"Product code '{vm.ProductCode}' is already in use.");
                }

                // Build unit lookup for components
                var componentIds = vm.SelectedProductComponents.Select(c => c.ComponentId).ToList();

                // Apply changes via mapper (handles component reconciliation and image processing)
                await _mapper.ApplyUpdateAsync(existing, vm, ct);

                // Persist changes
                _products.Update(existing);
                await _products.SaveAsync(ct);

                return Result<Product>.Success(existing);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<Product>.Fail("Operation was canceled.");
            }
            catch (Exception ex)
            {
                return Result<Product>.Fail($"Failed to update product: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a single product detail (mapped to detail VM)
        public async Task<Result<ProductDetailVm>> GetDetailAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            try
            {
                var product = await _products.GetByIdOrCodeWithComponentsAsync(id, code, asNoTracking: false, ct);
                if (product is null)
                    return Result<ProductDetailVm>.Fail("Product not found.");

                var detailVm = _mapper.ToDetailVm(product, includeImageDataUrl: true);
                return Result<ProductDetailVm>.Success(detailVm);
            }
            catch (Exception ex)
            {
                return Result<ProductDetailVm>.Fail($"Failed to retrieve product detail: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a single product entity (raw domain model)
        public async Task<Result<Product>> GetDomainAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            try
            {
                var product = await _products.GetByIdOrCodeAsync(id, code, asNoTracking: false, ct);
                if (product is null)
                    return Result<Product>.Fail("Product not found.");

                return Result<Product>.Success(product);
            }
            catch (Exception ex)
            {
                return Result<Product>.Fail($"Failed to retrieve product: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves full product list (mapped to list VMs)
        public async Task<Result<List<ProductListVm>>> ListAsync(CancellationToken ct = default)
        {
            try
            {
                var products = await _products.GetListAsync(asNoTracking: true, ct: ct);
                var listVms = products.Select(p => _mapper.ToListVm(p)).ToList();
                return Result<List<ProductListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<ProductListVm>>.Fail($"Failed to list products: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves multiple products by IDs or codes (mapped to list VMs)
        public async Task<Result<List<ProductListVm>>> GetListByIdsOrCodesAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default)
        {
            try
            {
                var filteredIds = ids?.Where(g => g != Guid.Empty);
                var filteredCodes = codes?.Where(s => !string.IsNullOrWhiteSpace(s));

                var products = await _products.GetListByIdsOrCodesAsync(filteredIds, filteredCodes, asNoTracking: true, ct: ct);
                var listVms = products.Select(p => _mapper.ToListVm(p)).ToList();
                return Result<List<ProductListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<ProductListVm>>.Fail($"Failed to retrieve products: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Searches products by term (name/code contains, case-insensitive)
        public async Task<Result<List<ProductListVm>>> SearchAsync(string? term, CancellationToken ct = default)
        {
            try
            {
                term ??= string.Empty;
                var products = await _products.SearchAsync(term, asNoTracking: true, ct: ct);
                var listVms = products.Select(p => _mapper.ToListVm(p)).ToList();
                return Result<List<ProductListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<ProductListVm>>.Fail($"Failed to search products: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Deletes a product by ID
        public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                return Result.Fail("Product ID is required.");

            try
            {
                var removed = await _products.RemoveByIdAsync(id, ct);
                if (!removed)
                    return Result.Fail("Product not found.");

                await _products.SaveAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to delete product: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Checks if a product code already exists (useful for validation)
        public async Task<Result<bool>> ExistsAsync(
            string code,
            Guid? excludeId = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result<bool>.Fail("Product code is required.");

            try
            {
                var product = await _products.GetByCodeAsync(code.Trim(), asNoTracking: true, ct);

                if (product is null)
                    return Result<bool>.Success(false);

                // If we're excluding an ID (for update scenarios), check if it's the same product
                if (excludeId.HasValue && product.Id == excludeId.Value)
                    return Result<bool>.Success(false);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Failed to check product existence: {ex.Message}");
            }
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\