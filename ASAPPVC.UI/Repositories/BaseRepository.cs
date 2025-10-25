using ASAPPVC.UI.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASAPPVC.UI.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        protected BaseRepository(AppDbContext db)
        {
            ArgumentNullException.ThrowIfNull(db);
            _db = db;
            _set = _db.Set<T>();
        }

        // ============== Create ==============
        /// <summary>
        /// Adds a single entity to the context (does NOT call SaveChanges).
        /// Returns the added entity instance (useful if EF populated identity values).
        /// </summary>
        /// <remarks>Does not persist to the database until <see cref="SaveAsync"/> is called.</remarks>
        public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var entry = await _set.AddAsync(entity, ct);
            return entry.Entity;
        }

        /// <summary>Add multiple entities (does NOT save).</summary>
        public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            return _set.AddRangeAsync(entities, ct);
        }

        // ============== Read ==============
        /// <summary>
        /// Find an entity by its primary key (single key).
        /// Returns null if no matching entity is tracked or exists in the database.
        /// </summary>
        /// <param name="id">The primary key value. Must not be null.</param>
        public virtual async Task<T?> FindAsync(object id, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(id);

            // DbSet.FindAsync expects an object[] of key values
            var entity = await _set.FindAsync(new object[] { id }, ct);
            return entity;
        }

        /// <summary>
        /// Get first entity matching predicate or null.
        /// </summary>
        public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            var query = asNoTracking ? _set.AsNoTracking() : _set;
            return query.FirstOrDefaultAsync(predicate, ct);
        }

        /// <summary>Get all items. AsNoTracking by default.</summary>
        public virtual Task<List<T>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            return (asNoTracking ? _set.AsNoTracking() : _set).ToListAsync(ct);
        }

        /// <summary>Get all items matching predicate. AsNoTracking by default.</summary>
        public virtual Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            var query = asNoTracking ? _set.AsNoTracking() : _set;
            return query.Where(predicate).ToListAsync(ct);
        }

        /// <summary>Returns true if the set contains any rows.</summary>
        public virtual Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return _set.AsNoTracking().AnyAsync(ct);
        }

        /// <summary>Returns true if any entity matches the predicate.</summary>
        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return _set.AsNoTracking().AnyAsync(predicate, ct);
        }

        /// <summary>Total row count.</summary>
        public virtual Task<int> CountAsync(CancellationToken ct = default)
        {
            return _set.CountAsync(ct);
        }

        /// <summary>Count entities matching predicate.</summary>
        public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return _set.CountAsync(predicate, ct);
        }

        /// <summary>Check if an item exists by id (single key).</summary>
        public virtual async Task<bool> ExistsByIdAsync(object id, CancellationToken ct = default)
        {
            return await FindAsync(id, ct) is not null;
        }

        // ============== Update ==============
        /// <summary>Marks an entity as modified (does NOT save).</summary>
        public virtual void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _set.Update(entity);
        }

        /// <summary>Marks multiple entities as modified (does NOT save).</summary>
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            _set.UpdateRange(entities);
        }

        // ============== Delete ==============
        /// <summary>delete an entity (does NOT save).</summary>
        public virtual void Delete(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _set.Remove(entity);
        }

        /// <summary>Find by id and delete if found (does NOT save). Returns true if deleted.</summary>
        public virtual async Task<bool> DeleteByIdAsync(object id, CancellationToken ct = default)
        {
            var entity = await FindAsync(id, ct);
            if (entity == null)
                return false;
            _set.Remove(entity);
            return true;
        }

        /// <summary>delete multiple entities (does NOT save).</summary>
        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            _set.RemoveRange(entities);
        }

        // ============== Save ==============
        /// <summary>Persist pending changes.</summary>
        public virtual Task<int> SaveAsync(CancellationToken ct = default)
        {
            return _db.SaveChangesAsync(ct);
        }
    }
}