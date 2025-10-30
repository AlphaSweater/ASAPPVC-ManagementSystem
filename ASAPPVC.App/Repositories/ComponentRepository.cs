using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.App.Repositories
{
    #region Interface

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

    #endregion Interface

    public class ComponentRepository(AppDbContext db) : BaseRepository<Component>(db), IComponentRepository
    {
        protected override string? CodePropertyName => "ComponentCode";

        // ---------- Reads ----------

        public async Task<Component?> GetByIdOrCodeAsync(
            Guid? id = null,
            string? code = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer Id lookups if any valid Guid provided
            if (id.HasValue && id.Value != Guid.Empty)
                return await GetByIdAsync(id.Value, asNoTracking, ct);

            // Fallback to Code lookups if any non-blank string provided
            if (!string.IsNullOrWhiteSpace(code))
                return await GetByCodeAsync(code!, asNoTracking, ct);

            // Neither provided => nothing to fetch
            return null;
        }

        public async Task<List<Component>> GetListByIdOrCodeAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer IDs when any valid Guid is present
            var hasValidIds = ids?.Any(g => g != Guid.Empty) == true;
            if (hasValidIds)
                return await GetListByIdsAsync(ids!, asNoTracking, ct);

            // Fallback to codes when any non-blank code is present
            var hasValidCodes = codes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true;
            if (hasValidCodes)
                return await GetListByCodesAsync(codes!, asNoTracking, ct);

            // Neither provided => empty
            return new List<Component>(0);
        }

        public async Task<List<Component>> GetListOrderedByCodeAsync(
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            var list = await ListAsync(asNoTracking, ct);
            return list.OrderBy(c => c.ComponentCode).ToList();
        }

        // ---------- Search ----------

        public Task<List<Component>> SearchAsync(
            string term,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
            {
                // When no term, return full list ordered by ComponentCode (align with new default list behavior)
                return GetListOrderedByCodeAsync(asNoTracking, ct);
            }

            // Simple contains search on Name/ComponentCode; push to DB with tracking control
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
            return query.Where(c => EF.Functions.Like(c.ComponentName, $"%{term}%")
                                || EF.Functions.Like(c.ComponentCode, $"%{term}%"))
                       .OrderBy(c => c.ComponentName)
                       .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\