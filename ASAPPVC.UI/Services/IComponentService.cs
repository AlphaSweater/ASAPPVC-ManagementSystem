using ASAPPVC.App.Models;
using ASAPPVC.App.Utils;

namespace ASAPPVC.App.Services
{
    /// <summary>
    /// Service contract for component-related business logic.
    /// Provides higher-level operations that orchestrate validation, normalization, mapping,
    /// and repository interactions for <see cref="Component"/> instances.
    /// </summary>
    public interface IComponentService
    {
        /// <summary>
        /// Creates a new component from the supplied view model.
        /// The service validates input, maps the VM to a domain entity using the component mapper,
        /// processes any uploaded image, and delegates persistence to the repository.
        /// </summary>
        /// <param name="vm">Create view model containing component details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the created <see cref="Component"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Component>> CreateAsync(ComponentFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing component from the supplied edit view model.
        /// The service validates input, fetches the existing component, applies the edit VM using the mapper,
        /// and saves changes to the repository.
        /// </summary>
        /// <param name="vm">Edit view model containing updated component details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the updated <see cref="Component"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Component>> UpdateAsync(ComponentFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single component by its internal identifier or by its human-friendly code.
        /// Returns the component mapped to a detail view model suitable for display.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="ComponentDetailVm"/>, or a failure result
        /// if the component does not exist or an error occurs.
        /// </returns>
        Task<Result<ComponentDetailVm>> GetDetailAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single component entity (not mapped) by its internal identifier or by its human-friendly code.
        /// Useful for operations that need the raw domain entity.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="Component"/>, or a failure result
        /// if the component does not exist or an error occurs.
        /// </returns>
        Task<Result<Component>> GetDomainAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the full list of components mapped to lightweight list view models.
        /// Suitable for display in tables, cards, or summary views.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="ComponentListVm"/> instances.
        /// </returns>
        Task<Result<List<ComponentListVm>>> ListAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple components by a collection of internal ids and/or human-friendly codes.
        /// Returns components mapped to list view models.
        /// </summary>
        /// <param name="ids">Optional collection of internal component ids to retrieve.</param>
        /// <param name="codes">Optional collection of human-friendly component codes to retrieve.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of matching <see cref="ComponentListVm"/> instances.
        /// </returns>
        Task<Result<List<ComponentListVm>>> GetListByIdsOrCodesAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default);

        /// <summary>
        /// Searches components using a free-text term against name and component code.
        /// Returns matching components mapped to list view models.
        /// </summary>
        /// <param name="term">Search term to match against component properties. Null or empty means no filter.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of components that match the search term;
        /// an empty list indicates no matches.
        /// </returns>
        Task<Result<List<ComponentListVm>>> SearchAsync(string? term, CancellationToken ct = default);

        /// <summary>
        /// Deletes a component by its internal identifier.
        /// </summary>
        /// <param name="id">The internal GUID identifier of the component to delete.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating success or failure with an error message.
        /// </returns>
        Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Checks if a component with the given code already exists.
        /// Useful for validation before creating or updating components.
        /// </summary>
        /// <param name="code">The component code to check.</param>
        /// <param name="excludeId">Optional component ID to exclude from the check (useful for updates).</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing true if the code exists, false otherwise.
        /// </returns>
        Task<Result<bool>> ExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    }
}