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
            _set = db.Set<T>();
        }

        // ---------- Create ----------
        public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var entry = await _set.AddAsync(entity, ct);
            return entry.Entity; // identity values may be populated
        }

        public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entities);

            return _set.AddRangeAsync(entities, ct);
        }

        // ---------- Read ----------
        public virtual async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(id);

            return await _set.FindAsync(new object[] { id }, ct);
        }

        public virtual Task<List<T>> ListAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            return (asNoTracking ? _set.AsNoTracking() : _set).ToListAsync(ct);
        }

        public virtual Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var q = asNoTracking ? _set.AsNoTracking() : _set;
            return q.Where(predicate).ToListAsync(ct);
        }

        public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var q = asNoTracking ? _set.AsNoTracking() : _set;
            return q.FirstOrDefaultAsync(predicate, ct);
        }

        public virtual Task<int> CountAsync(CancellationToken ct = default)
        {
            return _set.CountAsync(ct);
        }

        public virtual Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return _set.AsNoTracking().AnyAsync(ct);
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

        public virtual async Task<bool> RemoveByIdAsync(object id, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(id, ct);
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
    }
}