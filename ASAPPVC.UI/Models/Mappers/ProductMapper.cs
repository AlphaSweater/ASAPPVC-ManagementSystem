using ASAPPVC.UI.Models.Enums;
using ASAPPVC.UI.Services;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Implementation of <see cref="IProductMapper"/>. Inherits shared helpers from <see cref="MapperBase"/>.
    /// </summary>
    public class ProductMapper(
        IProductComponentMapper productComponentMapper,
    ICodeGenerationService? codeGenerationService = null,
     IImageService? imageService = null) : MapperBase(codeGenerationService, imageService), IProductMapper
    {
        private readonly IProductComponentMapper _productComponentMapper = productComponentMapper ?? throw new ArgumentNullException(nameof(productComponentMapper));

        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        public ProductListVm ToListVm(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var pcs = product.ProductComponents ?? new List<ProductComponent>();

            return new ProductListVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                HasImage = product.Image?.Data is { Length: > 0 },
                ThumbUrl = product.Image?.Data is { Length: > 0 } ? $"/products/{product.Id}/image/thumb" : null,
                ComponentCount = pcs.Count,
                Category = product.Category,
                Material = product.Material,
                Colour = product.Colour
            };
        }

        public ProductDetailVm ToDetailVm(Product product, bool includeImageDataUrl = true)
        {
            ArgumentNullException.ThrowIfNull(product);

            var components = _productComponentMapper.ToVms(product.ProductComponents ?? Enumerable.Empty<ProductComponent>());

            return new ProductDetailVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = product.Image?.Data is { Length: > 0 } ? $"/products/{product.Id}/image" : null,
                Components = components,
                Category = product.Category,
                Material = product.Material,
                Colour = product.Colour
            };
        }

        // ------------------------------------------------------------
        // ViewModels → Domain
        // ------------------------------------------------------------

        public async Task<Product> FromCreateVmAsync(CreateProductVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var product = new Product
            {
                ProductCode = NormalizeCodeOrGenerate(vm.ProductCode, "PROD"),
                Name = NormalizeString(vm.Name),
                Description = NormalizeString(vm.Description),
                Price = NormalizeMoney(vm.Price),
                Category = vm.Category,
                Material = vm.Material,
                Colour = vm.Colour
            };

            // Process image upload if provided
            if (vm.ImageData is { Length: > 0 } && !string.IsNullOrWhiteSpace(vm.ImageType))
            {
                if (ImageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await ImageService.ProcessBytesAsync(vm.ImageData, vm.ImageType, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error);

                product.Image = processed.Value!.ToAppImage();
            }

            // Lines via shared mapper (handles duplicate merge + unit resolution).
            var lookup = componentUnitLookup ?? new Dictionary<Guid, Unit>();
            product.ProductComponents = _productComponentMapper.FromCreateVms(
        product.Id,
                 vm.Components ?? Enumerable.Empty<CreateProductComponentVm>(),
             lookup);

            return product;
        }

        public async Task ApplyEditVmAsync(Product target, EditProductVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);
            if (target.Id != vm.Id)
                throw new InvalidOperationException("Mismatched product Id.");

            target.ProductCode = NormalizeString(vm.ProductCode);
            target.Name = NormalizeString(vm.Name);
            target.Description = NormalizeString(vm.Description);
            target.Price = NormalizeMoney(vm.Price);

            // Image semantics:
            // null   → leave unchanged
            // empty  → clear
            // filled → replace
            if (vm.ImageData is null)
            {
                // leave as-is
            }
            else if (vm.ImageData.Length == 0)
            {
                target.Image = null;
            }
            else if (!string.IsNullOrWhiteSpace(vm.ImageType))
            {
                if (ImageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await ImageService.ProcessBytesAsync(vm.ImageData, vm.ImageType, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error);

                target.Image = processed.Value!.ToAppImage();
            }

            // Simple modifiers
            target.Category = vm.Category;
            target.Material = vm.Material;
            target.Colour = vm.Colour;

            // Delegate edit-line mapping to shared mapper (null-safe on both sides).
            var existingLines = target.ProductComponents ?? Enumerable.Empty<ProductComponent>();
            var editLines = vm.Components ?? Enumerable.Empty<EditProductComponentVm>();
            var lookup = componentUnitLookup ?? new Dictionary<Guid, Unit>();

            target.ProductComponents = _productComponentMapper.ApplyEditVms(existingLines, editLines, lookup);
        }
    }
}