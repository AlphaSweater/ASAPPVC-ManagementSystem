using System.Linq.Expressions;

namespace ASAPPVC.UI.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        Task<T?> GetByIdAsync(object id, CancellationToken ct = default);

        Task<List<T>> ListAsync(bool asNoTracking = true, CancellationToken ct = default);

        Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default);

        Task<int> CountAsync(CancellationToken ct = default);

        Task<bool> AnyAsync(CancellationToken ct = default);

        void Update(T entity);

        void Remove(T entity);

        Task<bool> RemoveByIdAsync(object id, CancellationToken ct = default);

        Task<int> SaveAsync(CancellationToken ct = default);
    }
}