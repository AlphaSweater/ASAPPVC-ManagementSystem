using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;
        public CustomerRepository(AppDbContext db) => _db = db;

        public async Task<CustomerModel> AddAsync(CustomerModel customer, CancellationToken ct = default)
        {
            var entry = await _db.Customer.AddAsync(customer, ct);
            return entry.Entity;
        }

        public Task SaveAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
