using ASAPPVC.UI.Models;
using ASAPPVC.UI.Utils;

namespace ASAPPVC.UI.Services
{
    /// <summary>
    /// Service contract for product-related business logic.
    /// Provides higher-level operations that orchestrate validation, normalization, mapping,
    /// and repository interactions for <see cref="Product"/> instances.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Creates a new product from the supplied view model.
        /// The service validates input, maps the VM to a domain entity using the product mapper,
        /// resolves component units, and delegates persistence to the repository.
        /// </summary>
        /// <param name="vm">Create view model containing product details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the created <see cref="Product"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Product>> CreateAsync(CreateProductVm vm, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing product from the supplied edit view model.
        /// The service validates input, fetches the existing product, applies the edit VM using the mapper,
        /// and saves changes to the repository.
        /// </summary>
        /// <param name="vm">Edit view model containing updated product details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the updated <see cref="Product"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Product>> UpdateAsync(EditProductVm vm, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single product by its internal identifier or by its human-friendly code.
        /// Returns the product mapped to a detail view model suitable for display.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="ProductDetailVm"/>, or a failure result
        /// if the product does not exist or an error occurs.
        /// </returns>
        Task<Result<ProductDetailVm>> GetDetailAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single product entity (not mapped) by its internal identifier or by its human-friendly code.
        /// Useful for operations that need the raw domain entity.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="Product"/>, or a failure result
        /// if the product does not exist or an error occurs.
        /// </returns>
        Task<Result<Product>> GetAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the full list of products mapped to lightweight list view models.
        /// Suitable for display in tables, cards, or summary views.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="ProductListVm"/> instances.
        /// </returns>
        Task<Result<List<ProductListVm>>> ListAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple products by a collection of internal ids and/or human-friendly codes.
        /// Returns products mapped to list view models.
        /// </summary>
        /// <param name="ids">Optional collection of internal product ids to retrieve.</param>
        /// <param name="codes">Optional collection of human-friendly product codes to retrieve.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of matching <see cref="ProductListVm"/> instances.
        /// </returns>
        Task<Result<List<ProductListVm>>> GetListByIdsOrCodesAsync(
        IEnumerable<Guid>? ids = null,
        IEnumerable<string>? codes = null,
        CancellationToken ct = default);

        /// <summary>
        /// Searches products using a free-text term against name and product code.
        /// Returns matching products mapped to list view models.
        /// </summary>
        /// <param name="term">Search term to match against product properties. Null or empty means no filter.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of products that match the search term;
        /// an empty list indicates no matches.
        /// </returns>
        Task<Result<List<ProductListVm>>> SearchAsync(string? term, CancellationToken ct = default);

        /// <summary>
        /// Deletes a product by its internal identifier.
        /// </summary>
        /// <param name="id">The internal GUID identifier of the product to delete.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating success or failure with an error message.
        /// </returns>
        Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Checks if a product with the given code already exists.
        /// Useful for validation before creating or updating products.
        /// </summary>
        /// <param name="code">The product code to check.</param>
        /// <param name="excludeId">Optional product ID to exclude from the check (useful for updates).</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing true if the code exists, false otherwise.
        /// </returns>
        Task<Result<bool>> ExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    }
}