using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Customer;

namespace ASAPPVC.UI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<(bool Ok, string? Error, CustomerModel? Customer)> CreateAsync(CreateCustomerViewModel vm, CancellationToken ct = default);
    }
}
