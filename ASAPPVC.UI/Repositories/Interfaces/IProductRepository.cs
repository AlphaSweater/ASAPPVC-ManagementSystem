using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<ProductModel> AddProductAsync(ProductModel product, CancellationToken ct = default);
        Task AddProductPartsAsync(IEnumerable<ProductPartModel> lines, CancellationToken ct = default);
        Task<List<PartModel>> GetPartsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
    }
}
