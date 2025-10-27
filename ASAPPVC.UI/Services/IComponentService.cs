using ASAPPVC.UI.Models;
using ASAPPVC.UI.Utils;

namespace ASAPPVC.UI.Services
{
    /// <summary>
    /// Service contract for component-related business logic.
    /// Provides higher-level operations that orchestrate validation, normalization and
    /// repository interactions for <see cref="Component"/> instances.
    /// </summary>
    public interface IComponentService
    {
        /// <summary>
        /// Creates a new component from the supplied view model.
        /// The service is responsible for validating and normalizing input, reading the
        /// optional image file into memory, and delegating persistence to the repository.
        /// </summary>
        /// <param name="vm">Create view model containing component details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the created <see cref="Component"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Component>> CreateComponentAsync(CreateComponentVm vm, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single component by its internal identifier or by its human-friendly code.
        /// Implementations may choose which parameter takes precedence when both are provided.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="Component"/>, or a failure result
        /// if the component does not exist or an error occurs.
        /// </returns>
        Task<Result<Component>> GetComponentByIdOrCodeAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple components by a collection of internal ids and/or human-friendly codes.
        /// The repository may return the union of matches when both collections are provided.
        /// </summary>
        /// <param name="ids">Optional collection of internal component ids to retrieve.</param>
        /// <param name="codes">Optional collection of human-friendly component codes to retrieve.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of matching <see cref="Component"/> instances;
        /// an empty list indicates no matches were found.
        /// </returns>
        Task<Result<List<Component>>> GetComponentsListByIdOrCodeAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the full list of components. This is a read-only operation suitable for lookups and display.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of all <see cref="Component"/> instances.
        /// </returns>
        Task<Result<List<Component>>> GetComponentsListAsync(CancellationToken ct = default);

        /// <summary>
        /// Searches components using a free-text term. Implementations may search against name, code,
        /// or other searchable fields and should perform case-insensitive matching.
        /// </summary>
        /// <param name="term">Search term to match against component properties. Null or empty means no filter.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of components that match the search term;
        /// an empty list indicates no matches.
        /// </returns>
        Task<Result<List<Component>>> SearchComponentsAsync(string? term, CancellationToken ct = default);
    }
}