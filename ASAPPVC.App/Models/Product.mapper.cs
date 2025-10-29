using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Models.General;
using ASAPPVC.App.Models.Mappers;
using ASAPPVC.App.Services;

namespace ASAPPVC.App.Models
{
    #region Interface

    /// <summary>
    /// Converts between <see cref="Product"/> domain entities and their ViewModels
    /// (Create/Update via <see cref="ProductFormVm"/>, <see cref="ProductListVm"/>, <see cref="ProductDetailVm"/>).
    /// Uses <see cref="IProductComponentMapper"/> for component-line mapping to keep behavior consistent.
    /// </summary>
    public interface IProductMapper
    {
        /// <summary>
        /// Converts a <see cref="Product"/> to a lightweight <see cref="ProductListVm"/> for lists/cards.
        /// Counts component lines from <see cref="Product.ProductComponents"/> (null-safe).
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var listVms = products.Select(p => _mapper.ToListVm(p)).ToList();
        /// </code>
        /// </summary>
        ProductListVm ToListVm(Product product);

        /// <summary>
        /// Converts a <see cref="Product"/> to a full <see cref="ProductDetailVm"/>.
        /// Delegates line mapping to <see cref="IProductComponentMapper"/>.
        /// Optionally includes an inline Base64 image data URL.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var detailVm = _mapper.ToDetailVm(product);
        /// </code>
        /// </summary>
        ProductDetailVm ToDetailVm(Product product, bool includeImageDataUrl = true);

        /// <summary>
        /// Converts a Product domain entity into a ProductFormVm for use in edit forms.
        /// </summary>
        ProductFormVm ToFormVm(Product product);

        /// <summary>
        /// Creates a new Product from a ProductFormVm.
        /// </summary>
        Task<Product> FromCreateVmAsync(ProductFormVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default);

        /// <summary>
        /// Applies an update to an existing Product from a ProductFormVm and returns the modified entity.
        /// </summary>
        Task<Product> ApplyUpdateAsync(Product existing, ProductFormVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default);
    }

    #endregion Interface

    /// <summary>
    /// Implementation of <see cref="IProductMapper"/>. Inherits shared helpers from <see cref="MapperBase"/>.
    /// </summary>
    public class ProductMapper(IProductComponentMapper productComponentMapper, ICodeGenerationService? codeGenerationService = null, IImageService? imageService = null) : MapperBase(codeGenerationService, imageService), IProductMapper
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

        public ProductFormVm ToFormVm(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var components = _productComponentMapper.ToBridgeVms(product.ProductComponents ?? Enumerable.Empty<ProductComponent>());

            return new ProductFormVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ExistingImageUrl = product.Image?.Data is { Length: > 0 } ? $"/products/{product.Id}/image" : null,
                Category = product.Category,
                Material = product.Material,
                Colour = product.Colour,
                Components = components
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
            product.ProductComponents = _productComponentMapper.FromBridgeVms(product.Id, vm.Components ?? Enumerable.Empty<ProductComponentVm>(), lookup);

            return product;
        }

        public async Task<Product> ApplyUpdateAsync(Product existing, ProductFormVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default)
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
            existing.ProductComponents = _productComponentMapper.ApplyUpdateToBridgeVms(existing.ProductComponents, existing.Id, vm.Components ?? Enumerable.Empty<ProductComponentVm>(), lookup);

            return existing;
        }
    }
}