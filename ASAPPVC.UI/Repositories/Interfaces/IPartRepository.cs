using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories.Interfaces
{
    public interface IPartRepository
    {
        Task<PartModel> AddAsync(PartModel part, CancellationToken ct = default);
        Task<PartModel?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<PartModel>> ListAsync(CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
    }
}
