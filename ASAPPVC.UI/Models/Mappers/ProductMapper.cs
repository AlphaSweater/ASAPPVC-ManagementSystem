using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Provides conversion between <see cref="Product"/> domain entities and their corresponding
    /// ViewModels (<see cref="CreateProductVm"/>, <see cref="EditProductVm"/>, <see cref="ProductListVm"/>, <see cref="ProductDetailVm"/>).
    /// </summary>
    public static class ProductMapper
    {
        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        /// <summary>
        /// Converts a <see cref="Product"/> to a lightweight <see cref="ProductListVm"/> for table/card lists.
        /// </summary>
        public static ProductListVm ToListVm(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var pcs = product.ProductComponents ?? [];

            return new ProductListVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                HasImage = product.ImageData is { Length: > 0 } && !string.IsNullOrWhiteSpace(product.ImageType),
                ComponentCount = pcs.Count
            };
        }

        /// <summary>
        /// Converts a <see cref="Product"/> to a full <see cref="ProductDetailVm"/> with optional inline image data URL.
        /// </summary>
        public static ProductDetailVm ToDetailVm(Product product, bool includeImageDataUrl = true)
        {
            ArgumentNullException.ThrowIfNull(product);

            var components = (product.ProductComponents ?? new List<ProductComponent>())
                .Select(pc => new ProductComponentVm
                {
                    ComponentId = pc.ComponentId,
                    ComponentName = pc.Component?.Name ?? "(unknown)",
                    Unit = pc.Component?.Unit ?? Unit.Piece,
                    QuantityRequired = pc.QuantityRequired
                })
                .ToList();

            return new ProductDetailVm
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageBase64DataUrl = includeImageDataUrl ? AsDataUrlOrNull(product.ImageData, product.ImageType) : null,
                Components = components
            };
        }

        // ------------------------------------------------------------
        // ViewModels → Domain
        // ------------------------------------------------------------

        /// <summary>
        /// Creates a new <see cref="Product"/> domain entity from a <see cref="CreateProductVm"/>.
        /// </summary>
        public static Product FromCreateVm(CreateProductVm vm, Func<string>? codeGenerator = null)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var product = new Product
            {
                ProductCode = NormalizeCodeOrGenerate(vm.ProductCode, codeGenerator),
                Name = NormalizeString(vm.Name),
                Description = NormalizeString(vm.Description),
                Price = NormalizePrice(vm.Price),
                ImageData = vm.ImageData ?? Array.Empty<byte>(),
                ImageType = NormalizeString(vm.ImageType)
            };

            product.ProductComponents = MapCreateComponents(vm.Components, product.Id);
            return product;
        }

        /// <summary>
        /// Updates an existing <see cref="Product"/> with data from an <see cref="EditProductVm"/>.
        /// </summary>
        public static void ApplyEditVm(Product target, EditProductVm vm)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);
            if (target.Id != vm.Id)
                throw new InvalidOperationException("Mismatched product Id.");

            target.ProductCode = NormalizeString(vm.ProductCode);
            target.Name = NormalizeString(vm.Name);
            target.Description = NormalizeString(vm.Description);
            target.Price = NormalizePrice(vm.Price);

            // Image semantics:
            // null   → leave unchanged
            // empty  → clear
            // filled → replace
            if (vm.ImageData is null)
            {
                // no change
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

            target.ProductComponents = MapEditComponents(vm.Components, target.Id);
        }

        // ------------------------------------------------------------
        // Private mapping helpers
        // ------------------------------------------------------------

        private static List<ProductComponent> MapCreateComponents(List<CreateProductComponentVm> items, Guid productId)
        {
            if (items is null || items.Count == 0)
                return new();

            return items
                .GroupBy(i => i.ComponentId)
                .Select(g => new ProductComponent
                {
                    ProductId = productId,
                    ComponentId = g.Key,
                    QuantityRequired = g.Sum(x => x.QuantityRequired)
                })
                .ToList();
        }

        private static List<ProductComponent> MapEditComponents(List<EditProductComponentVm> items, Guid productId)
        {
            if (items is null || items.Count == 0)
                return new();

            return items
                .GroupBy(i => i.ComponentId)
                .Select(g => new ProductComponent
                {
                    ProductId = productId,
                    ComponentId = g.Key,
                    QuantityRequired = g.Sum(x => x.QuantityRequired)
                })
                .ToList();
        }

        // ------------------------------------------------------------
        // Utility helpers
        // ------------------------------------------------------------

        private static string NormalizeString(string? s)
        {
            return (s ?? string.Empty).Trim();
        }

        private static string NormalizeCodeOrGenerate(string? code, Func<string>? generator)
        {
            var trimmed = (code ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
                return trimmed;

            if (generator is not null)
            {
                var gen = (generator() ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(gen))
                    return gen;
            }

            return $"PROD-{Guid.NewGuid():N}".Substring(0, 13);
        }

        private static decimal NormalizePrice(decimal price)
        {
            return price < 0 ? 0 : decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        }

        private static string? AsDataUrlOrNull(byte[]? data, string? type)
        {
            if (data is not { Length: > 0 })
                return null;
            var safeType = string.IsNullOrWhiteSpace(type) ? "image/png" : type.Trim();
            var b64 = Convert.ToBase64String(data);
            return $"data:{safeType};base64,{b64}";
        }

        // ------------------------------------------------------------
        // Usage Examples
        // ------------------------------------------------------------

        /// <example>
        /// <code>
        /// // Create → Domain
        /// var domain = ProductMapper.FromCreateVm(createVm, () => CodeGenerator.Next());
        /// await _repo.AddAsync(domain, ct);
        ///
        /// // Domain → List
        /// var listVms = products.Select(ProductMapper.ToListVm).ToList();
        ///
        /// // Domain → Detail
        /// var detailVm = ProductMapper.ToDetailVm(product);
        ///
        /// // Edit → Apply
        /// ProductMapper.ApplyEditVm(existingProduct, editVm);
        /// await _repo.SaveAsync(ct);
        /// </code>
        /// </example>
        public static void __UsageDocOnly()
        { /* documentation only */ }
    }
}