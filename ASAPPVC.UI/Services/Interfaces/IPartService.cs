// Services/Interfaces/IPartService.cs
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Inventory;

namespace ASAPPVC.UI.Services.Interfaces
{
    public interface IPartService
    {
        Task<(bool Ok, string? Error, PartModel? Part)> CreateAsync(CreatePartViewModel vm, CancellationToken ct = default);
        Task<PartModel?> GetAsync(int id, CancellationToken ct = default);
        Task<List<PartModel>> ListAsync(CancellationToken ct = default);
    }
}
