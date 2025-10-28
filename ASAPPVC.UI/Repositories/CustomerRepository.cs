using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public class CustomerRepository(AppDbContext db) : BaseRepository<CustomerModel>(db), ICustomerRepository
    {
        public async Task<List<CustomerModel>> ListAsync(CancellationToken ct = default)
        {
            var list = await base.ListAsync(asNoTracking: true, ct);
            return list.OrderBy(c => c.Name).ToList();
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\