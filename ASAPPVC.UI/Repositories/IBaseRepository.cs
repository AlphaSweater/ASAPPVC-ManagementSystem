using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASAPPVC.UI.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Adds an entity to the set and returns the tracked entity. Identity values may be populated after add.<br/>
        /// Note: You must call <see cref="SaveAsync(CancellationToken)"/> to persist this change to the database.
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken ct = default);

        /// <summary>
        /// Adds multiple entities to the set.<br/>
        /// Note: You must call <see cref="SaveAsync(CancellationToken)"/> to persist these changes to the database.
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        /// <summary>
        /// Adds a range to an arbitrary bridge DbSet (for related entities).<br/>
        /// Note: You must call <see cref="SaveAsync(CancellationToken)"/> to persist these changes to the database.
        /// </summary>
        Task AddRangeToBridgeAsync<U>(DbSet<U> bridgeSet, IEnumerable<U> entities, CancellationToken ct = default)
            where U : class;

        /// <summary>
        /// Gets a single entity by ID (Guid). Tracking ON by default - suitable for edits.<br/>
        /// If <paramref name="asNoTracking"/> is true, the entity will be returned as no-tracking.<br/>
        /// </summary>
        Task<T?> GetByIdAsync(Guid id, bool asNoTracking = false, CancellationToken ct = default);

        /// <summary>
        /// Gets a single entity by its human-friendly code (if supported). No-tracking by default - suitable for lookups.<br/>
        /// Note: Not all repositories support code lookups; calling this on an unsupported repository may throw <see cref="NotSupportedException"/>.
        /// </summary>
        Task<T?> GetByCodeAsync(string code, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Gets multiple entities by ID. No-tracking by default - used mostly for display or lookups.<br/>
        /// </summary>
        Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Gets multiple entities by their codes. No-tracking by default - used mostly for display or lookups.<br/>
        /// Note: Not all repositories support code lookups; calling this on an unsupported repository may throw <see cref="NotSupportedException"/>.
        /// </summary>
        Task<List<T>> GetByCodesAsync(IEnumerable<string> codes, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Lists all entities. No-tracking by default - intended for read-only operations.<br/>
        /// Note: This is a read operation; no call to <see cref="SaveAsync(CancellationToken)"/> is required.
        /// </summary>
        Task<List<T>> ListAsync(bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Filters entities by predicate. No-tracking by default - intended for queries.<br/>
        /// Note: This is a read operation; no call to <see cref="SaveAsync(CancellationToken)"/> is required.
        /// </summary>
        Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Gets the first entity matching a predicate. No-tracking by default – safe for lookups.<br/>
        /// Note: This is a read operation; no call to <see cref="SaveAsync(CancellationToken)"/> is required.
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default);

        /// <summary>
        /// Returns the total number of entities in the set.<br/>
        /// Note: This is a read operation; no call to <see cref="SaveAsync(CancellationToken)"/> is required.
        /// </summary>
        Task<int> CountAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns whether any entities exist in the set.<br/>
        /// Note: This is a read operation; no call to <see cref="SaveAsync(CancellationToken)"/> is required.
        /// </summary>
        Task<bool> AnyAsync(CancellationToken ct = default);

        /// <summary>
        /// Marks the entity as updated in the DbSet (tracking update).<br/>
        /// Note: You must call <see cref="SaveAsync(CancellationToken)"/> to persist this change to the database.
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Removes the specified entity from the DbSet.<br/>
        /// Note: You must call <see cref="SaveAsync(CancellationToken)"/> to persist this deletion to the database.
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// Removes an entity by its identifier. Returns true if the entity was found and removed, false otherwise.<br/>
        /// Note: You must call <see cref="SaveAsync(CancellationToken)"/> to persist this deletion to the database.
        /// </summary>
        Task<bool> RemoveByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Persists all pending changes to the database.<br/>
        /// Note: Call this after performing any add/update/remove operations that should be saved.
        /// </summary>
        Task<int> SaveAsync(CancellationToken ct = default);
    }
}