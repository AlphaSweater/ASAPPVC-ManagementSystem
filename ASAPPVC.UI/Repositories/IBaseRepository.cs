using System.Linq.Expressions;

namespace ASAPPVC.UI.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        // Create
        Task<T> AddAsync(T entity, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        // Read
        Task<T?> FindAsync(object id, CancellationToken ct = default);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default);

        Task<List<T>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default);

        Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default);

        Task<bool> AnyAsync(CancellationToken ct = default);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        Task<int> CountAsync(CancellationToken ct = default);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        Task<bool> ExistsByIdAsync(object id, CancellationToken ct = default);

        // Update
        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        // Delete
        void Delete(T entity);

        Task<bool> DeleteByIdAsync(object id, CancellationToken ct = default);

        void DeleteRange(IEnumerable<T> entities);

        // Save
        Task<int> SaveAsync(CancellationToken ct = default);
    }
}