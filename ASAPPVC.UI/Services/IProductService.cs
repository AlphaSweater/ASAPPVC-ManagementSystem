using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Inventory.Product;

namespace ASAPPVC.UI.Services
{
    public interface IProductService
    {
        Task<(bool Ok, string? Error, Guid? ProductId)> CreateAsync(CreateProductViewModel vm, CancellationToken ct = default);

        Task<Product?> GetAsync(Guid id, CancellationToken ct = default);
        Task<List<Product>> ListAsync(CancellationToken ct = default);
    }
}
