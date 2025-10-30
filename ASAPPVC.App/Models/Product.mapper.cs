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
        Task<Product> FromCreateVmAsync(ProductFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Applies an update to an existing Product from a ProductFormVm and returns the modified entity.
        /// </summary>
        Task<Product> ApplyUpdateAsync(Product existing, ProductFormVm vm, CancellationToken ct = default);
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

            var productComponents = product.ProductComponents ?? new List<ProductComponent>();
            var hasImage = product.Image?.Data is { Length: > 0 };

            return new ProductListVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,

                ProductName = product.ProductName,
                Description = product.Description,
                HasImage = hasImage,
                ThumbUrl = hasImage ? $"/products/{product.Id}/image/thumb" : null,

                SellingPrice = product.SellingPrice,

                ReorderLevel = product.ReorderLevel,

                ComponentCount = productComponents.Count,
            };
        }

        public ProductDetailVm ToDetailVm(Product product, bool includeImageDataUrl = true)
        {
            ArgumentNullException.ThrowIfNull(product);

            var productComponents = _productComponentMapper.ToBridgeVms(product.ProductComponents ?? Enumerable.Empty<ProductComponent>());
            var hasImage = product.Image?.Data is { Length: > 0 };

            return new ProductDetailVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,

                ProductName = product.ProductName,
                Description = product.Description,
                HasImage = hasImage,
                ImageUrl = hasImage ? $"/products/{product.Id}/image" : null,

                Category = product.Category,
                MaterialType = product.MaterialType,
                ColourOption = product.ColourOption,

                SellingPrice = product.SellingPrice,
                ProductionCost = product.ProductionCost,

                ReorderLevel = product.ReorderLevel,

                ProductComponents = productComponents,
            };
        }

        public ProductFormVm ToFormVm(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var productComponents = _productComponentMapper.ToBridgeVms(product.ProductComponents ?? Enumerable.Empty<ProductComponent>());

            return new ProductFormVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,

                ProductName = product.ProductName,
                Description = product.Description,
                ExistingImageUrl = product.Image?.Data is { Length: > 0 } ? $"/products/{product.Id}/image" : null,

                Category = product.Category,
                MaterialType = product.MaterialType,
                ColourOption = product.ColourOption,

                SellingPrice = product.SellingPrice,

                ReorderLevel = product.ReorderLevel,

                IsActive = product.IsActive,

                SelectedProductComponents = productComponents
            };
        }

        // ------------------------------------------------------------
        // ViewModels → Domain (Create / Update)
        // ------------------------------------------------------------

        public async Task<Product> FromCreateVmAsync(ProductFormVm vm, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var product = new Product
            {
                ProductCode = NormalizeCodeOrGenerate(vm.ProductCode, "PROD", vm.Category.GetCode()),

                ProductName = NormalizeString(vm.ProductName),
                Description = NormalizeString(vm.Description),

                Category = vm.Category,
                MaterialType = vm.MaterialType,
                ColourOption = vm.ColourOption,

                SellingPrice = NormalizeMoney(vm.SellingPrice),

                ReorderLevel = vm.ReorderLevel,

                IsActive = true,
            };

            // Process image upload if provided
            if (vm.Image is not null)
            {
                if (_imageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await _imageService.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error);

                product.Image = processed.Value!.ToAppImage();
            }

            product.ProductComponents = _productComponentMapper.FromBridgeVms(product.Id, vm.SelectedProductComponents ?? Enumerable.Empty<ProductComponentVm>());

            return product;
        }

        public async Task<Product> ApplyUpdateAsync(Product existing, ProductFormVm vm, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(vm);
            if (existing.Id != vm.Id)
                throw new InvalidOperationException("Mismatched product Id.");

            existing.ProductCode = NormalizeString(vm.ProductCode);
            existing.ProductName = NormalizeString(vm.ProductName);
            existing.Description = NormalizeString(vm.Description);

            existing.Category = vm.Category;
            existing.MaterialType = vm.MaterialType;
            existing.ColourOption = vm.ColourOption;

            existing.SellingPrice = NormalizeMoney(vm.SellingPrice);

            existing.ReorderLevel = vm.ReorderLevel;

            existing.IsActive = vm.IsActive;

            // Process new image upload if provided
            if (vm.Image is not null)
            {
                if (_imageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await _imageService.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error ?? "Image processing failed.");

                existing.Image = processed.Value?.ToAppImage()
                    ?? throw new InvalidOperationException("Processed image returned null.");
            }

            existing.ProductComponents = _productComponentMapper.ApplyUpdateToBridgeVms(existing.ProductComponents, existing.Id, vm.SelectedProductComponents ?? Enumerable.Empty<ProductComponentVm>());

            return existing;
        }
    }
}