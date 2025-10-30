using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.App.Repositories
{
    #region Interface

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
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>The matching <see cref="Product"/> if found; otherwise <c>null</c>.</returns>
        Task<Product?> GetByIdOrCodeAsync(Guid? id = null, string? code = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a product including its <see cref="Product.ProductComponents"/> and related component entities<br/>
        /// by either <paramref name="id"/> or <paramref name="code"/>. Useful for display/edit views that need the<br/>
        /// component relationships populated.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the product.</param>
        /// <param name="code">Optional human-friendly product code.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>The matching <see cref="Product"/> with components loaded if found; otherwise <c>null</c>.</returns>
        Task<Product?> GetByIdOrCodeWithComponentsAsync(Guid? id = null, string? code = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple products by either a collection of <paramref name="ids"/> or a collection of <paramref name="codes"/>.<br/>
        /// Implementations may prefer ids when any valid guid is present and fallback to codes otherwise.<br/>
        /// </summary>
        /// <param name="ids">Optional collection of product ids to retrieve.</param>
        /// <param name="codes">Optional collection of product codes to retrieve.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Product"/> instances; an empty list if none found.</returns>
        Task<List<Product>> GetListByIdsOrCodesAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple products by either a collection of <paramref name="ids"/> or a collection of <paramref name="codes"/>,<br/>
        /// including their <see cref="Product.ProductComponents"/> and related component entities. Useful for display/edit<br/>
        /// views that need the component relationships populated.<br/>
        /// </summary>
        /// <param name="ids">Optional collection of product ids to retrieve.</param>
        /// <param name="codes">Optional collection of product codes to retrieve.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Product"/> instances with components loaded; an empty list if none found.</returns>
        Task<List<Product>> GetListByIdsOrCodesWithComponentsAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all products ordered by <see cref="Product.ProductCode"/>. Intended for lookup and display<br/>
        /// scenarios where deterministic ordering is useful.<br/>
        /// </summary>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>Ordered list of all <see cref="Product"/> instances.</returns>
        Task<List<Product>> GetListAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all products with their components included, ordered by <see cref="Product.ProductCode"/>.<br/>
        /// Intended for lookup and display scenarios where deterministic ordering is useful and component<br/>
        /// information is required.<br/>
        /// </summary>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>Ordered list of all <see cref="Product"/> instances with components loaded.</returns>
        Task<List<Product>> GetListWithComponentsAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Performs a simple search for products using <paramref name="term"/> against <see cref="Product.Name"/><br/>
        /// and <see cref="Product.ProductCode"/>. When <paramref name="term"/> is null or whitespace the<br/>
        /// implementation may return the full ordered list.<br/>
        /// </summary>
        /// <param name="term">Search term to match against name or product code.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Product"/> instances; an empty list if none match.</returns>
        Task<List<Product>> SearchAsync(string term, bool asNoTracking = true, CancellationToken ct = default);
    }

    #endregion Interface

    public class ProductRepository(AppDbContext db) : BaseRepository<Product>(db), IProductRepository
    {
        // ---------- Reads ----------

        public async Task<Product?> GetByIdOrCodeAsync(
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

        public async Task<Product?> GetByIdOrCodeWithComponentsAsync(
            Guid? id = null,
            string? code = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer Id lookups if any valid Guid provided
            if (id.HasValue && id.Value != Guid.Empty)
            {
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
                    .Include(p => p.ProductComponents)
                    .ThenInclude(pc => pc.Component)
                    .FirstOrDefaultAsync(p => p.Id == id.Value, ct);
            }

            // Fallback to Code lookups if any non-blank string provided
            if (!string.IsNullOrWhiteSpace(code))
            {
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
                    .Include(p => p.ProductComponents)
                    .ThenInclude(pc => pc.Component)
                    .FirstOrDefaultAsync(p => p.ProductCode == code, ct);
            }

            // Neither provided => nothing to fetch
            return null;
        }

        public async Task<List<Product>> GetListByIdsOrCodesAsync(
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
            return new List<Product>(0);
        }

        public async Task<List<Product>> GetListByIdsOrCodesWithComponentsAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            // Prefer IDs when any valid Guid is present
            var hasValidIds = ids?.Any(g => g != Guid.Empty) == true;
            if (hasValidIds)
            {
                var idSet = ids!.Where(g => g != Guid.Empty).ToHashSet();
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
                    .Where(p => idSet.Contains(p.Id))
                    .Include(p => p.ProductComponents)
                    .ThenInclude(pc => pc.Component)
                    .OrderBy(p => p.ProductCode)
                    .ToListAsync(ct);
            }

            // Fallback to codes when any non-blank code is present
            var hasValidCodes = codes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true;
            if (hasValidCodes)
            {
                var codeSet = codes!.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
                return await query
                    .Where(p => codeSet.Contains(p.ProductCode))
                    .Include(p => p.ProductComponents)
                    .ThenInclude(pc => pc.Component)
                    .OrderBy(p => p.ProductCode)
                    .ToListAsync(ct);
            }

            // Neither provided => empty
            return new List<Product>(0);
        }

        public async Task<List<Product>> GetListAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            var list = await ListAsync(asNoTracking, ct);
            return list.OrderBy(p => p.ProductCode).ToList();
        }

        public async Task<List<Product>> GetListWithComponentsAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
            return await query
                .Include(p => p.ProductComponents)
                .ThenInclude(pc => pc.Component)
                .OrderBy(p => p.ProductCode)
                .ToListAsync(ct);
        }

        // ---------- Search ----------

        public Task<List<Product>> SearchAsync(string term, bool asNoTracking = true, CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
            {
                // When no term, return full list ordered by ComponentCode (align with new default list behavior)
                return GetListAsync(asNoTracking, ct);
            }

            // Simple contains search on Name/ComponentCode; push to DB with tracking control
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
            return query.Where(p => EF.Functions.Like(p.ProductName, $"%{term}%")
                                || EF.Functions.Like(p.ProductCode, $"%{term}%"))
                       .OrderBy(p => p.ProductName)
                       .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\