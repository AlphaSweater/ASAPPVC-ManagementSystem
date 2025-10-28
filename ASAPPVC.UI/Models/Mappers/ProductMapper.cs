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
        IImageService? imageService = null)
        : MapperBase(codeGenerationService, imageService), IProductMapper
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

            var components = _productComponentMapper.ToBridgeVms(product.ProductComponents ?? Enumerable.Empty<ProductComponent>());

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
        // ViewModels → Domain (Create / Update)
        // ------------------------------------------------------------

        public async Task<Product> FromCreateVmAsync(ProductFormVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default)
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
            if (vm.Image is not null)
            {
                if (ImageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await ImageService.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error);

                product.Image = processed.Value!.ToAppImage();
            }

            var lookup = componentUnitLookup ?? new Dictionary<Guid, Unit>();
            product.ProductComponents = _productComponentMapper.FromCreateBridgeVms(product.Id, vm.Components ?? Enumerable.Empty<ProductComponentFormVm>(), lookup);

            return product;
        }

        public async Task<Product> ApplyUpdateVmAsync(Product existing, ProductFormVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(vm);
            if (existing.Id != vm.Id)
                throw new InvalidOperationException("Mismatched product Id.");

            existing.ProductCode = NormalizeString(vm.ProductCode);
            existing.Name = NormalizeString(vm.Name);
            existing.Description = NormalizeString(vm.Description);
            existing.Price = NormalizeMoney(vm.Price);

            // Image semantics:
            // null → leave unchanged
            // empty → clear
            // filled → replace
            if (vm.Image is null)
            {
                // leave as-is
            }
            else if (vm.Image.Length == 0)
            {
                existing.Image = null;
            }
            else
            {
                if (ImageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await ImageService.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error);

                existing.Image = processed.Value!.ToAppImage();
            }

            // Simple modifiers
            existing.Category = vm.Category;
            existing.Material = vm.Material;
            existing.Colour = vm.Colour;

            var lookup = componentUnitLookup ?? new Dictionary<Guid, Unit>();
            existing.ProductComponents = _productComponentMapper.FromCreateBridgeVms(existing.Id, vm.Components ?? Enumerable.Empty<ProductComponentFormVm>(), lookup);

            return existing;
        }
    }
}