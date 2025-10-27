using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
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