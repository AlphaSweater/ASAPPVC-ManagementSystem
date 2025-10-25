using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface IComponentRepository : IBaseRepository<ComponentModel>
    {
        Task<ComponentModel> AddComponentAsync(ComponentModel component, CancellationToken ct = default);

        Task<bool> UpdateComponentAsync(ComponentModel component, CancellationToken ct = default);

        Task<bool> DeleteComponentAsync(Guid id, CancellationToken ct = default);

        Task<List<ComponentModel>> ListOrderedByNameAsync(CancellationToken ct = default);

        Task<ComponentModel?> GetByComponentCodeAsync(string componentCode, CancellationToken ct = default);

        Task<List<ComponentModel>> SearchAsync(string term, CancellationToken ct = default);
    }
}