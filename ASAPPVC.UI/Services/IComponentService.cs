using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Inventory;

namespace ASAPPVC.UI.Services
{
    public interface IComponentService
    {
        Task<(bool Ok, string? Error, ComponentModel? Part)> CreateAsync(CreateComponentViewModel vm, CancellationToken ct = default);

        Task<ComponentModel?> GetAsync(Guid id, CancellationToken ct = default);

        Task<List<ComponentModel>> ListAsync(CancellationToken ct = default);
    }
}