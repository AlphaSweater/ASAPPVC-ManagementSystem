using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.ViewModels.Customer;

namespace ASAPPVC.App.Services
{
    #region Interface

    public interface ICustomerService
    {
        Task<(bool Ok, string? Error, Customer? Customer)> CreateAsync(CreateCustomerViewModel vm, CancellationToken ct = default);

        Task<List<Customer>> ListAsync(CancellationToken ct = default);
    }

    #endregion Interface

    public class CustomerService(ICustomerRepository repo) : ICustomerService
    {
        private readonly ICustomerRepository _repo = repo;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //creates a new customer in the database
        public async Task<(bool Ok, string? Error, Customer? Customer)> CreateAsync(CreateCustomerViewModel vm, CancellationToken ct = default)
        {
            var entity = new Customer
            {
                Name = vm.FirstName.Trim(),
                Surname = vm.LastName.Trim(),
                Company = string.IsNullOrWhiteSpace(vm.Company) ? null : vm.Company.Trim(),
                PhoneNumber = vm.PhoneNumber.Trim(),
                Email = vm.Email.Trim()
            };

            await _repo.AddAsync(entity, ct);
            await _repo.SaveAsync(ct);

            return (true, null, entity);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves a list of customers from the database
        public async Task<List<Customer>> ListAsync(CancellationToken ct = default)
        {
            return await _repo.ListAsync(ct);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\