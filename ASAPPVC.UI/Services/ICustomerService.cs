using ASAPPVC.App.Models;
using ASAPPVC.App.ViewModels.Customer;

namespace ASAPPVC.App.Services
{
    public interface ICustomerService
    {
        Task<(bool Ok, string? Error, CustomerModel? Customer)> CreateAsync(CreateCustomerViewModel vm, CancellationToken ct = default);

        Task<List<CustomerModel>> ListAsync(CancellationToken ct = default);
    }
}