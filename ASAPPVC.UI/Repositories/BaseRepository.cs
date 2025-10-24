using ASAPPVC.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public abstract class BaseRepository<T> where T : class
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

            var entity = await _set.FindAsync([id], ct);
            return entity;
        }

        /// <summary>Get all items. AsNoTracking by default.</summary>
        public virtual Task<List<T>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            return (asNoTracking ? _set.AsNoTracking() : _set).ToListAsync(ct);
        }

        /// <summary>Returns true if the set contains any rows.</summary>
        public virtual Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return _set.AsNoTracking().AnyAsync(ct);
        }

        /// <summary>Total row count.</summary>
        public virtual Task<int> CountAsync(CancellationToken ct = default)
        {
            return _set.CountAsync(ct);
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