using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface IProductRepository : IBaseRepository<ProductModel>
    {
        // Domain-flavoured CRUD (compose base ops + Save)
        Task<ProductModel> AddProductAsync(ProductModel product, CancellationToken ct = default);

        Task<ProductModel> AddProductWithComponentsAsync(
            ProductModel product,
            IEnumerable<ProductComponentModel> components,
            CancellationToken ct = default);

        Task<bool> UpdateProductAsync(ProductModel product, CancellationToken ct = default);

        Task<bool> DeleteProductAsync(Guid id, CancellationToken ct = default);

        // Components (bridge) helpers
        Task<bool> ReplaceComponentsAsync(Guid productId, IEnumerable<ProductComponentModel> components, CancellationToken ct = default);

        // Reads with small interpretations
        Task<ProductModel?> GetWithComponentsAsync(Guid id, CancellationToken ct = default);

        Task<List<ProductModel>> ListOrderedByNameAsync(CancellationToken ct = default);

        Task<ProductModel?> GetByProductCodeAsync(string productCode, CancellationToken ct = default);

        Task<List<ProductModel>> SearchAsync(string term, CancellationToken ct = default);

        // Utility (optional)
        Task<List<ComponentModel>> GetComponentsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}