namespace ASAPPVC.UI.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        // Create
        Task<T> AddAsync(T entity, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        // Read
        Task<T?> FindAsync(object id, CancellationToken ct = default);

        Task<List<T>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default);

        Task<bool> AnyAsync(CancellationToken ct = default);

        Task<int> CountAsync(CancellationToken ct = default);

        Task<bool> ExistsByIdAsync(object id, CancellationToken ct = default);

        // Update
        void Update(T entity);

        // Delete
        void Delete(T entity);

        Task<bool> DeleteByIdAsync(object id, CancellationToken ct = default);

        void DeleteRange(IEnumerable<T> entities);

        // Save
        Task<int> SaveAsync(CancellationToken ct = default);
    }
}