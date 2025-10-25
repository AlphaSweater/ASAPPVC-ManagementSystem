using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    // Inherit common CRUD from IBaseRepository<T> and expose only product-specific APIs
    public interface IProductRepository : IBaseRepository<ProductModel>
    {
        Task AddProductPartsAsync(IEnumerable<ProductComponentModel> lines, CancellationToken ct = default);

        Task<List<ComponentModel>> GetPartsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

        Task<List<ProductModel>> ListAsync(CancellationToken ct = default);

        Task<ProductModel?> GetProductWithPartsAsync(Guid id, CancellationToken ct = default);
    }
}