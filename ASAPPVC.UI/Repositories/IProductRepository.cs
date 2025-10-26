using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    /// <summary>
    /// Repository interface for product-specific operations.
    /// Extends <see cref="IBaseRepository{T}"/> with helpers such as
    /// lookups by id or code, component-inclusive retrievals, bulk lookups,
    /// and simple search/list helpers used by the UI.
    /// </summary>
    public interface IProductRepository : IBaseRepository<ProductModel>
    {
        /// <summary>
        /// Retrieves a product by its internal <paramref name="id"/> or by its human-friendly <paramref name="code"/>.
        /// Implementations should prefer <paramref name="id"/> when a valid non-empty guid is provided and
        /// fallback to <paramref name="code"/> when no valid id is available.
        /// </summary>
        /// <param name="id">Optional internal identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>The matching <see cref="ProductModel"/> if found; otherwise <c>null</c>.</returns>
        Task<ProductModel?> GetByIdOrCodeAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a product including its <see cref="ProductModel.ProductComponents"/> and related component entities
        /// by either <paramref name="id"/> or <paramref name="code"/>. Useful for display/edit views that need the
        /// component relationships populated.
        /// </summary>
        /// <param name="id">Optional internal identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>The matching <see cref="ProductModel"/> with components loaded if found; otherwise <c>null</c>.</returns>
        Task<ProductModel?> GetByIdOrCodeWithComponentsAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple products by either a collection of <paramref name="ids"/> or a collection of <paramref name="codes"/>.
        /// Implementations may prefer ids when any valid guid is present and fallback to codes otherwise.
        /// </summary>
        /// <param name="ids">Optional collection of product ids to retrieve.</param>
        /// <param name="codes">Optional collection of product codes to retrieve.</param>
        /// <returns>List of matching <see cref="ProductModel"/> instances; an empty list if none found.</returns>
        Task<List<ProductModel>> GetListByIdsOrCodesAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, CancellationToken ct = default);

        /// <summary>
        /// Lists all products ordered by <see cref="ProductModel.ProductCode"/>. Intended for lookup and display
        /// scenarios where deterministic ordering is useful.
        /// </summary>
        /// <returns>Ordered list of all <see cref="ProductModel"/> instances.</returns>
        Task<List<ProductModel>> GetListAsync(CancellationToken ct = default);

        /// <summary>
        /// Performs a simple search for products using <paramref name="term"/> against <see cref="ProductModel.Name"/>
        /// and <see cref="ProductModel.ProductCode"/>. When <paramref name="term"/> is null or whitespace the
        /// implementation may return the full ordered list.
        /// </summary>
        /// <param name="term">Search term to match against name or product code.</param>
        /// <returns>List of matching <see cref="ProductModel"/> instances; an empty list if none match.</returns>
        Task<List<ProductModel>> SearchAsync(string term, CancellationToken ct = default);
    }
}