using ASAPPVC.UI.Models.ViewModels.Inventory;

namespace ASAPPVC.UI.Services.Interfaces
{
    public interface IProductService
    {
        Task<(bool Ok, string? Error, int? ProductId)> CreateAsync(CreateProductViewModel vm, CancellationToken ct = default);
    }
}
