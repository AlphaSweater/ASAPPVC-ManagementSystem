using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories.Implementation
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;

        public CustomerRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CustomerModel> AddAsync(CustomerModel customer, CancellationToken ct = default)
        {
            var entry = await _db.Customer.AddAsync(customer, ct);
            return entry.Entity;
        }

        // retrieves a list of customers ordered by name
        public async Task<List<CustomerModel>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Customer.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
        }

        public async Task SaveAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}