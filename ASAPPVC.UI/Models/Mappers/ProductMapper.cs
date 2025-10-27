using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Converts between <see cref="Product"/> domain entities and their ViewModels
    /// (<see cref="CreateProductVm"/>, <see cref="EditProductVm"/>, <see cref="ProductListVm"/>, <see cref="ProductDetailVm"/>).
    /// Uses <see cref="ProductComponentMapper"/> for component-line mapping to keep behavior consistent.
    /// </summary>
    public static class ProductMapper
    {
        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        /// <summary>
        /// Converts a <see cref="Product"/> to a lightweight <see cref="ProductListVm"/> for lists/cards.
        /// Counts component lines from <see cref="Product.ProductComponents"/> (null-safe).
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var listVms = products.Select(ProductMapper.ToListVm).ToList();
        /// </code>
        /// </summary>
        public static ProductListVm ToListVm(Product product)
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

        /// <summary>
        /// Converts a <see cref="Product"/> to a full <see cref="ProductDetailVm"/>.
        /// Delegates line mapping to <see cref="ProductComponentMapper.ToVms(System.Collections.Generic.IEnumerable{ProductComponent})"/>.
        /// Optionally includes an inline Base64 image data URL.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var detailVm = ProductMapper.ToDetailVm(product);
        /// </code>
        /// </summary>
        public static ProductDetailVm ToDetailVm(Product product, bool includeImageDataUrl = true)
        {
            ArgumentNullException.ThrowIfNull(product);

            var components = ProductComponentMapper.ToVms(product.ProductComponents ?? Enumerable.Empty<ProductComponent>());

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

        /// <summary>
        /// Creates a new <see cref="Product"/> from a <see cref="CreateProductVm"/>.
        /// Component lines are created via <see cref="ProductComponentMapper.FromCreateVms(Guid,System.Collections.Generic.IEnumerable{CreateProductComponentVm},System.Collections.Generic.IDictionary{System.Guid, Unit})"/>,
        /// which merges duplicate components and resolves units using the given lookup (defaults to <see cref="Unit.Piece"/> when missing).
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var domain = ProductMapper.FromCreateVm(createVm, () => CodeGenerator.Next(), unitLookup);
        /// await _repo.AddAsync(domain, ct);
        /// </code>
        /// </summary>
        public static Product FromCreateVm(
            CreateProductVm vm,
            Func<string>? codeGenerator = null,
            IDictionary<Guid, Unit>? componentUnitLookup = null)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var product = new Product
            {
                ProductCode = NormalizeCodeOrGenerate(vm.ProductCode, codeGenerator),
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
            product.ProductComponents = ProductComponentMapper.FromCreateVms(
                product.Id,
                vm.Components ?? Enumerable.Empty<CreateProductComponentVm>(),
                lookup);

            return product;
        }

        /// <summary>
        /// Applies an <see cref="EditProductVm"/> to an existing <see cref="Product"/>.
        /// Component lines are updated via <see cref="ProductComponentMapper.ApplyEditVms(System.Collections.Generic.IEnumerable{ProductComponent},System.Collections.Generic.IEnumerable{EditProductComponentVm},System.Collections.Generic.IDictionary{System.Guid, Unit})"/>,
        /// which updates/creates/merges by <c>ComponentId</c> and re-resolves units when needed.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// ProductMapper.ApplyEditVm(existingProduct, editVm, unitLookup);
        /// await _repo.SaveAsync(ct);
        /// </code>
        /// </summary>
        public static void ApplyEditVm(
            Product target,
            EditProductVm vm,
            IDictionary<Guid, Unit>? componentUnitLookup = null)
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

            target.ProductComponents = ProductComponentMapper.ApplyEditVms(existingLines, editLines, lookup);
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

            // Fallback: short stable unique-ish code prefix
            return $"PROD-{Guid.NewGuid():N}".Substring(0, 13);
        }

        private static decimal NormalizeMoney(decimal price)
        {
            return price < 0 ? 0 : decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        }

        private static string? AsDataUrlOrNull(byte[]? data, string? mime)
        {
            if (data is not { Length: > 0 })
                return null;
            var safeType = string.IsNullOrWhiteSpace(mime) ? "image/png" : mime.Trim();
            var b64 = Convert.ToBase64String(data);
            return $"data:{safeType};base64,{b64}";
        }
    }
}