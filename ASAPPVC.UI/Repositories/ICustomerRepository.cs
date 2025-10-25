using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface ICustomerRepository : IBaseRepository<CustomerModel>
    {
        // Returns all customers ordered by name (separate from GetAllAsync which is unordered)
        Task<List<CustomerModel>> ListAsync(CancellationToken ct = default);
    }
}