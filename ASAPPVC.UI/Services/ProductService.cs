using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Enums;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Utils;

namespace ASAPPVC.UI.Services
{
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
            if (string.IsNullOrWhiteSpace(vm.Name))
                return Result.Fail("Product name is required.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                return Result.Fail("Product description is required.");
            if (vm.Price <= 0)
                return Result.Fail("Product price must be greater than zero.");
            if (vm.Components is null || vm.Components.Count == 0)
                return Result.Fail("A product requires at least one component.");

            // Validate component entries
            foreach (var comp in vm.Components)
            {
                if (comp.ComponentId == Guid.Empty)
                    return Result.Fail("All components must have valid IDs.");
                if (comp.Quantity <= 0)
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
            if (string.IsNullOrWhiteSpace(vm.Name))
                return Result.Fail("Product name is required.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                return Result.Fail("Product description is required.");
            if (string.IsNullOrWhiteSpace(vm.ProductCode))
                return Result.Fail("Product code is required.");
            if (vm.Price <= 0)
                return Result.Fail("Product price must be greater than zero.");
            if (vm.Components is null || vm.Components.Count == 0)
                return Result.Fail("A product requires at least one component.");

            // Validate component entries
            foreach (var comp in vm.Components)
            {
                if (comp.ComponentId == Guid.Empty)
                    return Result.Fail("All components must have valid IDs.");
                if (comp.Quantity <= 0)
                    return Result.Fail("Component quantities must be greater than zero.");
            }

            return Result.Success();
        }

        private static void Normalize(ProductFormVm vm)
        {
            vm.Name = vm.Name.Trim();
            vm.Description = vm.Description.Trim();
            if (!string.IsNullOrWhiteSpace(vm.ProductCode))
                vm.ProductCode = vm.ProductCode.Trim();
        }

        // ---------- Build component unit lookup helper ----------

        private async Task<IDictionary<Guid, Unit>> BuildComponentUnitLookupAsync(
            IEnumerable<Guid> componentIds,
            CancellationToken ct)
        {
            var distinctIds = componentIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (distinctIds.Count == 0)
                return new Dictionary<Guid, Unit>();

            var components = await _components.GetListByIdsAsync(distinctIds, asNoTracking: true, ct);
            return components.ToDictionary(c => c.Id, c => c.Unit);
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
                // Build unit lookup for components
                var componentIds = vm.Components.Select(c => c.ComponentId).ToList();
                var unitLookup = await BuildComponentUnitLookupAsync(componentIds, ct);

                // Verify all components exist
                var missingIds = componentIds.Where(id => !unitLookup.ContainsKey(id)).ToList();
                if (missingIds.Any())
                    return Result<Product>.Fail($"Some components do not exist: {string.Join(", ", missingIds)}");

                // Map VM to domain entity (mapper handles code generation and component mapping and image processing)
                var product = await _mapper.FromCreateVmAsync(vm, unitLookup, ct);

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
                var componentIds = vm.Components.Select(c => c.ComponentId).ToList();
                var unitLookup = await BuildComponentUnitLookupAsync(componentIds, ct);

                // Verify all components exist
                var missingIds = componentIds.Where(id => !unitLookup.ContainsKey(id)).ToList();
                if (missingIds.Any())
                    return Result<Product>.Fail($"Some components do not exist: {string.Join(", ", missingIds)}");

                // Apply changes via mapper (handles component reconciliation and image processing)
                await _mapper.ApplyUpdateVmAsync(existing, vm, unitLookup, ct);

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