using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class CustomerRepository(AppDbContext db) : BaseRepository<CustomerModel>(db), ICustomerRepository
    {
        // keeps an ordered list specific to customers
        public async Task<List<CustomerModel>> ListAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\