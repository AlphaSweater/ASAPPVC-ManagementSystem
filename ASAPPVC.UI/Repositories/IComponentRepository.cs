using ASAPPVC.App.Models;

namespace ASAPPVC.App.Repositories
{
    /// <summary>
    /// Repository interface for component-specific operations.
    /// Extends <see cref="IBaseRepository{T}"/> with component-focused helpers such as
    /// searches and lookups by both internal id and human-friendly component code.
    /// </summary>
    public interface IComponentRepository : IBaseRepository<Component>
    {
        /// <summary>
        /// Retrieves a single component either by its internal <paramref name="id"/> or by its human-friendly <paramref name="code"/>.<br/>
        /// Implementations should support null for either parameter; if both are provided the implementation may choose a precedence.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <param name="asNoTracking">If true, returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>The matching <see cref="Component"/> if found; otherwise null.</returns>
        Task<Component?> GetByIdOrCodeAsync(Guid? id = null, string? code = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple components by a collection of ids and/or codes.<br/>
        /// If both collections are provided, the implementation may return the union of matches.<br/>
        /// </summary>
        /// <param name="ids">Optional collection of internal component ids to retrieve.</param>
        /// <param name="codes">Optional collection of human-friendly component codes to retrieve.</param>
        /// <param name="asNoTracking">If true, returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Component"/> instances; an empty list if none found.</returns>
        Task<List<Component>> GetListByIdOrCodeAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all components. This is a read operation intended for lookups and display.<br/>
        /// </summary>
        /// <param name="asNoTracking">If true, returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of all <see cref="Component"/> instances.</returns>
        Task<List<Component>> GetListOrderedByCodeAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Searches components using a free-text <paramref name="term"/>. Implementations may search against name, code, or other fields.<br/>
        /// </summary>
        /// <param name="term">Search term to match against components.</param>
        /// <param name="asNoTracking">If true, returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of components matching the search term; an empty list if no matches.</returns>
        Task<List<Component>> SearchAsync(string term, bool asNoTracking = true, CancellationToken ct = default);
    }
}