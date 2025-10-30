using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.App.Repositories
{
    #region Interface

    /// <summary>
    /// Repository interface for order-specific operations.
    /// Extends <see cref="IBaseRepository{T}"/> with order-focused helpers such as
    /// lookups by id or code, customer/product-inclusive retrievals, and search capabilities.
    /// </summary>
    public interface IOrderRepository : IBaseRepository<Order>
    {
        /// <summary>
        /// Retrieves an order by its internal <paramref name="id"/> or by its human-friendly <paramref name="code"/>.<br/>
        /// Implementations should prefer <paramref name="id"/> when a valid non-empty guid is provided and<br/>
        /// fallback to <paramref name="code"/> when no valid id is available.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the order.</param>
        /// <param name="code">Optional human-friendly order code.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>The matching <see cref="Order"/> if found; otherwise <c>null</c>.</returns>
        Task<Order?> GetByIdOrCodeAsync(Guid? id = null, string? code = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves an order including its <see cref="Order.Customer"/>, <see cref="Order.OrderProducts"/>,<br/>
        /// and related product entities by either <paramref name="id"/> or <paramref name="code"/>.<br/>
        /// Useful for display/edit views that need the full order context populated.<br/>
        /// </summary>
        /// <param name="id">Optional internal identifier of the order.</param>
        /// <param name="code">Optional human-friendly order code.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>The matching <see cref="Order"/> with details loaded if found; otherwise <c>null</c>.</returns>
        Task<Order?> GetByIdOrCodeWithDetailsAsync(Guid? id = null, string? code = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Loads the full domain graph for a single order (aggregate root) by Id:
        /// Order → Customer, OrderProducts → Product → ProductComponents → Component.
        /// </summary>
        /// <param name="id">Order Id (Guid).</param>
        /// <param name="asNoTracking">Return untracked entities when true (default).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<Order?> GetFullDomainAsync(Guid id, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple orders by either a collection of <paramref name="ids"/> or a collection of <paramref name="codes"/>.<br/>
        /// Implementations may prefer ids when any valid guid is present and fallback to codes otherwise.<br/>
        /// </summary>
        /// <param name="ids">Optional collection of order ids to retrieve.</param>
        /// <param name="codes">Optional collection of order codes to retrieve.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Order"/> instances; an empty list if none found.</returns>
        Task<List<Order>> GetListByIdsOrCodesAsync(IEnumerable<Guid>? ids = null, IEnumerable<string>? codes = null, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all orders with customer and order product details included, ordered by <see cref="Order.OrderDate"/> descending.<br/>
        /// Intended for lookup and display scenarios where deterministic ordering is useful.<br/>
        /// </summary>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>Ordered list of all <see cref="Order"/> instances with details loaded.</returns>
        Task<List<Order>> GetListWithDetailsAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all orders ordered by <see cref="Order.OrderDate"/> descending.<br/>
        /// Intended for lookup and display scenarios where deterministic ordering is useful.<br/>
        /// </summary>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>Ordered list of all <see cref="Order"/> instances.</returns>
        Task<List<Order>> GetListAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Performs a simple search for orders using <paramref name="term"/> against <see cref="Order.OrderCode"/>,<br/>
        /// customer name, and related fields. When <paramref name="term"/> is null or whitespace the<br/>
        /// implementation may return the full ordered list.<br/>
        /// </summary>
        /// <param name="term">Search term to match against order code or customer information.</param>
        /// <param name="asNoTracking">If true returns untracked entities suitable for read-only operations. Defaults to true.</param>
        /// <returns>List of matching <see cref="Order"/> instances; an empty list if none match.</returns>
        Task<List<Order>> SearchAsync(string term, bool asNoTracking = true, CancellationToken ct = default);
    }

    #endregion Interface

    public class OrderRepository(AppDbContext db) : BaseRepository<Order>(db), IOrderRepository
    {
        protected override string? CodePropertyName => "OrderCode";

        // ---------- Reads ----------

        public async Task<Order?> GetByIdOrCodeAsync(
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

        public async Task<Order?> GetByIdOrCodeWithDetailsAsync(
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
                    .Include(o => o.Customer)
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .FirstOrDefaultAsync(o => o.Id == id.Value, ct);
            }

            // Fallback to Code lookups if any non-blank string provided
            if (!string.IsNullOrWhiteSpace(code))
            {
                var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();

                return await query
                    .Include(o => o.Customer)
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .FirstOrDefaultAsync(o => o.OrderCode == code, ct);
            }

            // Neither provided => nothing to fetch
            return null;
        }

        public async Task<Order?> GetFullDomainAsync(
        Guid id,
        bool asNoTracking = true,
        CancellationToken ct = default)
        {
            if (id == Guid.Empty) return null;

            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();

            return await query
                .AsSplitQuery()
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                        .ThenInclude(p => p.ProductComponents)
                            .ThenInclude(pc => pc.Component)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task<List<Order>> GetListByIdsOrCodesAsync(
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
            return new List<Order>(0);
        }

        public async Task<List<Order>> GetListWithDetailsAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();

            return await query
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(ct);
        }

        public async Task<List<Order>> GetListAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            var list = await ListAsync(asNoTracking, ct);

            return list.OrderByDescending(o => o.OrderDate).ToList();
        }

        // ---------- Search ----------

        public Task<List<Order>> SearchAsync(string term, bool asNoTracking = true, CancellationToken ct = default)
        {
            term = (term ?? string.Empty).Trim();
            if (term.Length == 0)
            {
                // When no term, return full list ordered by OrderDate descending
                return GetListWithDetailsAsync(asNoTracking, ct);
            }

            // Simple contains search on OrderCode and Customer Name/Surname; push to DB with tracking control
            var query = asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();
            return query
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Where(o => EF.Functions.Like(o.OrderCode, $"%{term}%")
                     || (o.Customer != null && (EF.Functions.Like(o.Customer.Name, $"%{term}%")
                                              || EF.Functions.Like(o.Customer.Surname, $"%{term}%"))))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\