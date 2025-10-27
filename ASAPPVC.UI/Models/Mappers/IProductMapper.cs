using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Converts between <see cref="Product"/> domain entities and their ViewModels
    /// (<see cref="CreateProductVm"/>, <see cref="EditProductVm"/>, <see cref="ProductListVm"/>, <see cref="ProductDetailVm"/>).
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
        /// Creates a new <see cref="Product"/> from a <see cref="CreateProductVm"/>.
        /// Component lines are created via <see cref="IProductComponentMapper"/>,
        /// which merges duplicate components and resolves units using the given lookup (defaults to <see cref="Unit.Piece"/> when missing).
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var domain = await _mapper.FromCreateVmAsync(createVm, unitLookup, ct);
        /// await _repo.AddAsync(domain, ct);
        /// </code>
        /// </summary>
        Task<Product> FromCreateVmAsync(CreateProductVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default);

        /// <summary>
        /// Applies an <see cref="EditProductVm"/> to an existing <see cref="Product"/>.
        /// Component lines are updated via <see cref="IProductComponentMapper"/>,
        /// which updates/creates/merges by <c>ComponentId</c> and re-resolves units when needed.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// await _mapper.ApplyEditVmAsync(existingProduct, editVm, unitLookup, ct);
        /// await _repo.SaveAsync(ct);
        /// </code>
        /// </summary>
        Task ApplyEditVmAsync(Product target, EditProductVm vm, IDictionary<Guid, Unit>? componentUnitLookup = null, CancellationToken ct = default);
    }
}
