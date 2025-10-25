using ASAPPVC.UI.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASAPPVC.UI.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        // Property names (override in derived repos if needed)
        protected virtual string IdPropertyName => "Id";

        // Return null by default -> "this entity has no code"
        protected virtual string? CodePropertyName => null;

        protected BaseRepository(AppDbContext db)
        {
            ArgumentNullException.ThrowIfNull(db);

            _db = db;
            _set = db.Set<T>();
        }

        // ---------- Create ----------

        public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var entry = await _set.AddAsync(entity, ct);
            return entry.Entity;
        }

        public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            return _set.AddRangeAsync(entities, ct);
        }

        public virtual Task AddRangeToBridgeAsync<U>(DbSet<U> bridgeSet, IEnumerable<U> entities, CancellationToken ct = default)
            where U : class
        {
            ArgumentNullException.ThrowIfNull(bridgeSet);
            ArgumentNullException.ThrowIfNull(entities);

            return bridgeSet.AddRangeAsync(entities, ct);
        }

        // ---------- Read ----------

        public virtual async Task<T?> GetByIdAsync(Guid id, bool asNoTracking = false, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                return null;

            if (!asNoTracking)
                return await _set.FindAsync(new object[] { id }, ct);

            return await ApplyTracking(_set, asNoTracking)
                .SingleOrDefaultAsync(e => EF.Property<Guid>(e, IdPropertyName) == id, ct);
        }

        public virtual async Task<T?> GetByCodeAsync(string code, bool asNoTracking = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            if (CodePropertyName is null)
                throw new NotSupportedException($"{typeof(T).Name} does not support code lookups.");

            return await ApplyTracking(_set, asNoTracking)
                .SingleOrDefaultAsync(e => EF.Property<string>(e, CodePropertyName) == code, ct);
        }

        public virtual async Task<List<T>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idSet = ids.Where(g => g != Guid.Empty).ToHashSet();
            if (idSet.Count == 0)
                return new List<T>(0);

            return await ApplyTracking(_set, asNoTracking)
                .Where(e => idSet.Contains(EF.Property<Guid>(e, IdPropertyName)))
                .ToListAsync(ct);
        }

        public virtual async Task<List<T>> GetByCodesAsync(
            IEnumerable<string> codes,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(codes);
            if (CodePropertyName is null)
                throw new NotSupportedException($"{typeof(T).Name} does not support code lookups.");

            var codeSet = codes
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (codeSet.Count == 0)
                return new List<T>(0);

            return await ApplyTracking(_set, asNoTracking)
                .Where(e => codeSet.Contains(EF.Property<string>(e, CodePropertyName)))
                .ToListAsync(ct);
        }

        public virtual Task<List<T>> ListAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            return ApplyTracking(_set, asNoTracking).ToListAsync(ct);
        }

        public virtual Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return ApplyTracking(_set, asNoTracking)
                .Where(predicate)
                .ToListAsync(ct);
        }

        public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return ApplyTracking(_set, asNoTracking)
                .FirstOrDefaultAsync(predicate, ct);
        }

        public virtual Task<int> CountAsync(CancellationToken ct = default)
        {
            return _set.CountAsync(ct);
        }

        public virtual Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return _set.AnyAsync(ct);
        }

        // ---------- Update ----------

        public virtual void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _set.Update(entity);
        }

        // ---------- Delete ----------

        public virtual void Remove(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _set.Remove(entity);
        }

        public virtual async Task<bool> RemoveByIdAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Parameter 'id' must not be Guid.Empty.", nameof(id));

            var entity = await GetByIdAsync(id, false, ct);
            if (entity is null)
                return false;

            _set.Remove(entity);
            return true;
        }

        // ---------- Save ----------

        public virtual Task<int> SaveAsync(CancellationToken ct = default)
        {
            return _db.SaveChangesAsync(ct);
        }

        // ---------- Internal Helpers ----------

        private static IQueryable<T> ApplyTracking(IQueryable<T> source, bool asNoTracking)
        {
            return asNoTracking ? source.AsNoTracking() : source;
        }
    }
}