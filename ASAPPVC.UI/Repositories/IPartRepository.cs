using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    // Inherit the common CRUD contract from IBaseRepository<T> and
    // expose only part-specific convenience methods.
    public interface IPartRepository : IBaseRepository<PartModel>
    {
        // part-specific ordered list
        Task<List<PartModel>> ListAsync(CancellationToken ct = default);
    }
}