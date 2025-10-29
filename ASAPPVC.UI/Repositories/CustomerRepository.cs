using ASAPPVC.App.Data;
using ASAPPVC.App.Models;

namespace ASAPPVC.App.Repositories
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