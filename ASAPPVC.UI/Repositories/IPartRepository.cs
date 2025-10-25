using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    // Inherit the common CRUD contract from IBaseRepository<T> and
    // expose only part-specific convenience methods.
    public interface IPartRepository : IBaseRepository<PartModel>
    {
        // convenience: strongly-typed lookup by int id (FindAsync exists on base)
        Task<PartModel?> GetByIdAsync(int id, CancellationToken ct = default);

        // part-specific ordered list
        Task<List<PartModel>> ListAsync(CancellationToken ct = default);
    }
}