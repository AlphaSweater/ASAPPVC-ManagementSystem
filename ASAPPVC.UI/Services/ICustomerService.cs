using ASAPPVC.UI.Models;
using ASAPPVC.UI.ViewModels.Customer;

namespace ASAPPVC.UI.Services
{
    public interface ICustomerService
    {
        Task<(bool Ok, string? Error, CustomerModel? Customer)> CreateAsync(CreateCustomerViewModel vm, CancellationToken ct = default);
        Task<List<CustomerModel>> ListAsync(CancellationToken ct = default);
    }
}
