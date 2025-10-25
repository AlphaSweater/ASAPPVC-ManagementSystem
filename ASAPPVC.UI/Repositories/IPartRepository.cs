using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface IPartRepository : IBaseRepository<PartModel>
    {
        Task<PartModel> AddPartAsync(PartModel part, CancellationToken ct = default);

        Task<bool> UpdatePartAsync(PartModel part, CancellationToken ct = default);

        Task<bool> DeletePartAsync(int id, CancellationToken ct = default);

        Task<List<PartModel>> ListOrderedByNameAsync(CancellationToken ct = default);

        Task<PartModel?> GetByPartCodeAsync(string sku, CancellationToken ct = default);

        Task<List<PartModel>> SearchAsync(string term, CancellationToken ct = default);
    }
}