using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    /// <summary>
    /// Repository interface for product-specific operations.
    /// Extends <see cref="IBaseRepository{T}"/> with helpers such as
    /// lookups by id or code, component-inclusive retrievals, bulk lookups,
    /// and simple search/list helpers used by the UI.
    /// </summary>
    public interface IProductRepository : IBaseRepository<Product>
    {
        /// <summary>
        /// Retrieves a product by its internal <paramref name="id"/> or by its human-friendly <paramref name="code"/>.<br/>
        /// Implementations should prefer <paramref name="id"/> when a valid non-empty guid is provided and<br/>
        /// fallback to <paramref name="code"/> when no valid id is available.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>The matching <see cref="Product"/> if found; otherwise <c>null</c>.</returns>
        Task<Product?> GetByIdOrCodeAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a product including its <see cref="Product.ProductComponents"/> and related component entities<br/>
        /// by either <paramref name="id"/> or <paramref name="code"/>. Useful for display/edit views that need the<br/>
        /// component relationships populated.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>The matching <see cref="Product"/> with components loaded if found; otherwise <c>null</c>.</returns>
        Task<Product?> GetByIdOrCodeWithComponentsAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple products by either a collection of <paramref name="ids"/> or a collection of <paramref name="codes"/>.<br/>
        /// Implementations may prefer ids when any valid guid is present and fallback to codes otherwise.<br/>
        /// </summary>
        /// <param name="ids">Optional collection of product ids to retrieve.</param>
        /// <param name="codes">Optional collection of product codes to retrieve.</param>
        /// <returns>List of matching <see cref="Product"/> instances; an empty list if none found.</returns>
        Task<List<Product>> GetListByIdsOrCodesAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple products by either a collection of <paramref name="ids"/> or a collection of <paramref name="codes"/>,<br/>
        /// including their <see cref="Product.ProductComponents"/> and related component entities. Useful for display/edit<br/>
        /// views that need the component relationships populated.<br/>
        /// </summary>
        /// <param name="ids">Optional collection of product ids to retrieve.</param>
        /// <param name="codes">Optional collection of product codes to retrieve.</param>
        /// <returns>List of matching <see cref="Product"/> instances with components loaded; an empty list if none found.</returns>
        Task<List<Product>> GetListByIdsOrCodesWithComponentsAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, CancellationToken ct = default);

        /// <summary>
        /// Lists all products ordered by <see cref="Product.ProductCode"/>. Intended for lookup and display<br/>
        /// scenarios where deterministic ordering is useful.<br/>
        /// </summary>
        /// <returns>Ordered list of all <see cref="Product"/> instances.</returns>
        Task<List<Product>> GetListAsync(CancellationToken ct = default);

        /// <summary>
        /// Lists all products with their components included, ordered by <see cref="Product.ProductCode"/>.<br/>
        /// Intended for lookup and display scenarios where deterministic ordering is useful and component<br/>
        /// information is required.<br/>
        /// </summary>
        /// <returns>Ordered list of all <see cref="Product"/> instances with components loaded.</returns>
        Task<List<Product>> GetListWithComponentsAsync(CancellationToken ct = default);

        /// <summary>
        /// Performs a simple search for products using <paramref name="term"/> against <see cref="Product.Name"/><br/>
        /// and <see cref="Product.ProductCode"/>. When <paramref name="term"/> is null or whitespace the<br/>
        /// implementation may return the full ordered list.<br/>
        /// </summary>
        /// <param name="term">Search term to match against name or product code.</param>
        /// <returns>List of matching <see cref="Product"/> instances; an empty list if none match.</returns>
        Task<List<Product>> SearchAsync(string term, CancellationToken ct = default);
    }
}