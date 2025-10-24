using ASAPPVC.UI.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASAPPVC.UI.Repositories.Interfaces
{
    public interface ICustomerRepository : IBaseRepository<CustomerModel>
    {
        // Returns all customers ordered by name (separate from GetAllAsync which is unordered)
        Task<List<CustomerModel>> ListAsync(CancellationToken ct = default);
    }
}
