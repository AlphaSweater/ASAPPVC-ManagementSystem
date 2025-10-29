using ASAPPVC.App.Data;
using ASAPPVC.App.Models;

namespace ASAPPVC.App.Repositories
{
    #region Interface

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

    #endregion Interface

    public class CustomerRepository(AppDbContext db) : BaseRepository<Customer>(db), ICustomerRepository
    {
        public async Task<List<Customer>> ListAsync(CancellationToken ct = default)
        {
            var list = await base.ListAsync(asNoTracking: true, ct);
            return list.OrderBy(c => c.Name).ToList();
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\