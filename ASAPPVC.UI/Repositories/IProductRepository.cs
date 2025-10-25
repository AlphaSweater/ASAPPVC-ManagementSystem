using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    // Inherit common CRUD from IBaseRepository<T> and expose only product-specific APIs
    public interface IProductRepository : IBaseRepository<ProductModel>
    {
        Task AddProductPartsAsync(IEnumerable<ProductPartModel> lines, CancellationToken ct = default);

        Task<List<PartModel>> GetPartsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

        Task<List<ProductModel>> ListAsync(CancellationToken ct = default);

        Task<ProductModel?> GetProductWithPartsAsync(int id, CancellationToken ct = default);
    }
}