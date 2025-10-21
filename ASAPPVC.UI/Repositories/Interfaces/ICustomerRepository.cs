using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<CustomerModel> AddAsync(CustomerModel customer, CancellationToken ct = default);
        Task<List<CustomerModel>> ListAsync(CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
    }
}
