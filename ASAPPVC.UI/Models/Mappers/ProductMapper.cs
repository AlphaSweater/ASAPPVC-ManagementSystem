using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Implementation of <see cref="IProductMapper"/>. Inherits shared helpers from <see cref="MapperBase"/>.
    /// </summary>
    public class ProductMapper(
        IProductComponentMapper productComponentMapper,
        ICodeGenerator? codeGenerator = null,
        IImageService? imageService = null) : MapperBase(codeGenerator, imageService), IProductMapper
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
                HasImage = product.ImageData is { Length: > 0 } && !string.IsNullOrWhiteSpace(product.ImageType),
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
                ImageBase64DataUrl = includeImageDataUrl ? AsDataUrlOrNull(product.ImageData, product.ImageType) : null,
                Components = components,
                Category = product.Category,
                Material = product.Material,
                Colour = product.Colour
            };
        }

        // ------------------------------------------------------------
        // ViewModels → Domain
        // ------------------------------------------------------------

        public Product FromCreateVm(CreateProductVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var product = new Product
            {
                ProductCode = NormalizeCodeOrGenerate(vm.ProductCode, "PROD"),
                Name = NormalizeString(vm.Name),
                Description = NormalizeString(vm.Description),
                Price = NormalizeMoney(vm.Price),
                ImageData = vm.ImageData ?? Array.Empty<byte>(),
                ImageType = NormalizeString(vm.ImageType),
                Category = vm.Category,
                Material = vm.Material,
                Colour = vm.Colour
            };

            // Lines via shared mapper (handles duplicate merge + unit resolution).
            var lookup = componentUnitLookup ?? new Dictionary<Guid, Unit>();
            product.ProductComponents = _productComponentMapper.FromCreateVms(
                product.Id,
                vm.Components ?? Enumerable.Empty<CreateProductComponentVm>(),
                lookup);

            return product;
        }

        public void ApplyEditVm(Product target, EditProductVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null)
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
                target.ImageData = Array.Empty<byte>();
                target.ImageType = string.Empty;
            }
            else
            {
                target.ImageData = vm.ImageData;
                target.ImageType = NormalizeString(vm.ImageType);
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