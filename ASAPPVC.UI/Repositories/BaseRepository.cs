using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ASAPPVC.UI.Data;

namespace ASAPPVC.UI.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        protected BaseRepository(AppDbContext db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            _db = db;
            _set = _db.Set<T>();
        }

        // ============== Create ==============
        /// <summary>Add a single entity (does NOT save).</summary>
        public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var entry = await _set.AddAsync(entity, ct);
            return entry.Entity;
        }

        /// <summary>Add multiple entities (does NOT save).</summary>
        public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            return _set.AddRangeAsync(entities, ct);
        }

        // ============== Read ==============
        /// <summary>Find by primary key (single key). Returns null if not found.</summary>
        public virtual Task<T?> FindAsync(object id, CancellationToken ct = default)
        {
            return _set.FindAsync([id], ct).AsTask();
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
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _set.Update(entity);
        }

        // ============== Delete ==============
        /// <summary>Remove an entity (does NOT save).</summary>
        public virtual void Remove(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _set.Remove(entity);
        }

        /// <summary>Remove multiple entities (does NOT save).</summary>
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _set.RemoveRange(entities);
        }

        /// <summary>Find by id and remove if found (does NOT save). Returns true if removed.</summary>
        public virtual async Task<bool> RemoveByIdAsync(object id, CancellationToken ct = default)
        {
            var entity = await FindAsync(id, ct);
            if (entity == null) return false;
            _set.Remove(entity);
            return true;
        }

        // ============== Save ==============
        /// <summary>Persist pending changes.</summary>
        public virtual Task<int> SaveAsync(CancellationToken ct = default)
        {
            return _db.SaveChangesAsync(ct);
        }
    }
}