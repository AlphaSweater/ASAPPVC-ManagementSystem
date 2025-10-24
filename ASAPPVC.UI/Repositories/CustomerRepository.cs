using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class CustomerRepository : BaseRepository<CustomerModel>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext db) : base(db)
        {
        }

        // keeps an ordered list specific to customers
        public async Task<List<CustomerModel>> ListAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
        }
    }
}
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\