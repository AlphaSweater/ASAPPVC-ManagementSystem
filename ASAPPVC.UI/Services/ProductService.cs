using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Models.ViewModels.Inventory.Product;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Utils;

namespace ASAPPVC.UI.Services
{
    public class ProductService(IProductRepository productRepository, IComponentRepository componentRepository, ICodeGenerator codeGenerator) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;

        private readonly IComponentRepository _componentRepository = componentRepository;

        private readonly ICodeGenerator _codeGenerator = codeGenerator;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        /// <summary>
        /// Creates a product from the provided view model. Returns (Ok, Error, ProductId).
        /// </summary>
        public async Task<(bool Ok, string? Error, Guid? ProductId)> CreateAsync(CreateProductViewModel vm, CancellationToken ct = default)
        {
            // Basic validation — fail fast.
            if (string.IsNullOrWhiteSpace(vm.ProductName))
                return (false, "Product name is required.", null);
            if (string.IsNullOrWhiteSpace(vm.Description))
                return (false, "Description is required.", null);

            // Build the Product
            var product = vm.ToDomain();

            // Generate product code
            product.ProductCode = await _codeGenerator.GenerateAsync(CodeType.Product, product.Name);

            await AttachImageIfPresentAsync(product, vm, ct);

            var addedProduct = await _productRepository.AddAsync(product, ct);
            await _productRepository.SaveAsync(ct);
            return (true, null, addedProduct?.Id);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        /// <summary>Get product including its components.</summary>
        public async Task<Product?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _productRepository.GetByIdOrCodeWithComponentsAsync(id: id, code: null, ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        public async Task<List<Product>> ListAsync(CancellationToken ct = default)
        {
            return await _productRepository.GetListAsync(ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Helpers

        /// <summary>Σ(component.UnitCost × quantity) across requested components.</summary>
        private static decimal CalculateComponentsTotalCost(List<ProductComponentViewModel> productComponents, List<Component> allComponents)
        {
            // Handle empty lists early.
            if (productComponents.Count == 0 || allComponents.Count == 0)
                return 0m;

            // Lookup by ID for repeated access.
            var componentLookup = allComponents.ToDictionary(c => c.Id);

            decimal totalCost = 0m;

            // Straight loop for clarity; TryGetValue avoids KeyNotFound.
            foreach (var productComponent in productComponents)
            {
                if (componentLookup.TryGetValue(productComponent.ComponentId, out var component))
                    totalCost += component.UnitCost * productComponent.Quantity;
            }

            return totalCost;
        }

        /// <summary>
        /// Creates a ProductModel using the base price plus calculated parts total.
        /// </summary>
        private static Product BuildProductFromVm(CreateProductViewModel vm, decimal partsTotalCost)
        {
            return new Product
            {
                Name = vm.ProductName!.Trim(),
                Price = vm.BasePrice + partsTotalCost,
                Description = vm.Description!.Trim()
            };
        }

        /// <summary>
        /// Maps view-model component lines to bridge entities (ProductComponentModel).
        /// ProductId is assigned by repository when saving the aggregate.
        /// </summary>
        private static List<ProductComponent> BuildComponentRows(List<ProductComponentViewModel> productComponents)
        {
            return productComponents.Select(pc => new ProductComponent
            {
                // Repository will normalize ProductId when adding the product with components,
                // so we don't assign ProductId here to avoid confusion.
                ComponentId = pc.ComponentId,
                Quantity = pc.Quantity
            }).ToList();
        }

        /// <summary>
        /// Copies image from the posted file to the product entity if present.
        /// </summary>
        private static async Task AttachImageIfPresentAsync(Product product, CreateProductViewModel vm, CancellationToken ct)
        {
            if (vm.ImageFile is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await vm.ImageFile.CopyToAsync(ms, ct);
                product.ImageBytes = ms.ToArray();
                product.ImageContentType = vm.ImageFile.ContentType;
            }
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\