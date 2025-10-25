using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    /// <summary>
    /// Repository interface for customer-related operations.
    /// Extends <see cref="IBaseRepository{CustomerModel}"/>.
    /// </summary>
    public interface ICustomerRepository : IBaseRepository<CustomerModel>
    {
        /// <summary>
        /// Lists all customers. No-tracking by default - intended for read-only operations.
        /// </summary>
        Task<List<CustomerModel>> ListAsync(CancellationToken ct = default);
    }
}