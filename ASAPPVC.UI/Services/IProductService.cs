using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Services
{
    public interface IProductService
    {
        Task<(bool Ok, string? Error, Guid? ProductId)> CreateAsync(CreateProductVm vm, CancellationToken ct = default);

        Task<Product?> GetAsync(Guid id, CancellationToken ct = default);

        Task<List<Product>> ListAsync(CancellationToken ct = default);
    }
}