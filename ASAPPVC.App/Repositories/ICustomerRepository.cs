using ASAPPVC.App.Models;

namespace ASAPPVC.App.Repositories
{
    /// <summary>
    /// Repository interface for customer-related operations.
    /// Extends <see cref="IBaseRepository{CustomerModel}"/>.
    /// </summary>
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        /// <summary>
        /// Lists all customers. No-tracking by default - intended for read-only operations.<br/>
        /// </summary>
        Task<List<Customer>> ListAsync(CancellationToken ct = default);
    }
}