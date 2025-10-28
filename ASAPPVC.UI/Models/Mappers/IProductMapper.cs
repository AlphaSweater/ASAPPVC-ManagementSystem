using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
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
        /// Creates a new Product from a ProductFormVm.
        /// </summary>
        Task<Product> FromCreateVmAsync(ProductFormVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default);

        /// <summary>
        /// Applies an update to an existing Product from a ProductFormVm and returns the modified entity.
        /// </summary>
        Task<Product> ApplyUpdateVmAsync(Product existing, ProductFormVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default);
    }
}