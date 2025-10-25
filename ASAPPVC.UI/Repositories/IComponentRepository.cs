using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    /// <summary>
    /// Repository interface for component-specific operations.
    /// Extends <see cref="IBaseRepository{T}"/> with component-focused helpers such as
    /// searches and lookups by both internal id and human-friendly component code.
    /// </summary>
    public interface IComponentRepository : IBaseRepository<ComponentModel>
    {
        /// <summary>
        /// Retrieves a single component either by its internal <paramref name="id"/> or by its human-friendly <paramref name="code"/>.
        /// Implementations should support null for either parameter; if both are provided the implementation may choose a precedence.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <returns>The matching <see cref="ComponentModel"/> if found; otherwise null.</returns>
        Task<ComponentModel?> GetByIdOrCodeAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple components by a collection of ids and/or codes.
        /// If both collections are provided, the implementation may return the union of matches.
        /// </summary>
        /// <param name="ids">Optional collection of internal component ids to retrieve.</param>
        /// <param name="codes">Optional collection of human-friendly component codes to retrieve.</param>
        /// <returns>List of matching <see cref="ComponentModel"/> instances; an empty list if none found.</returns>
        Task<List<ComponentModel>> GetListByIdOrCodeAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, CancellationToken ct = default);

        /// <summary>
        /// Lists all components. This is a read operation intended for lookups and display.
        /// </summary>
        /// <returns>List of all <see cref="ComponentModel"/> instances.</returns>
        Task<List<ComponentModel>> GetListAsync(CancellationToken ct = default);

        /// <summary>
        /// Searches components using a free-text <paramref name="term"/>. Implementations may search against name, code, or other fields.
        /// </summary>
        /// <param name="term">Search term to match against components.</param>
        /// <returns>List of components matching the search term; an empty list if no matches.</returns>
        Task<List<ComponentModel>> SearchAsync(string term, CancellationToken ct = default);
    }
}