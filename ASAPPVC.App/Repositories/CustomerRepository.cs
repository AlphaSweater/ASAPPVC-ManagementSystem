using ASAPPVC.App.Data;
using ASAPPVC.App.Models;

namespace ASAPPVC.App.Repositories
{
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