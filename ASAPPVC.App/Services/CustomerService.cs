using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.ViewModels.Customer;
using ASAPPVC.App.Utils;

namespace ASAPPVC.App.Services
{
    #region Interface

    public interface ICustomerService
    {
        Task<(bool Ok, string? Error, Customer? Customer)> CreateAsync(CreateCustomerViewModel vm, CancellationToken ct = default);

        Task<List<Customer>> ListAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves customers for lookups (Id + basic display info).
        /// Returns domain Customer objects to match existing form VMs.
        /// </summary>
        Task<Result<List<Customer>>> GetAvailableCustomersAsync(CancellationToken ct = default);
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

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves customers for lookups (domain Customer objects)
        public async Task<Result<List<Customer>>> GetAvailableCustomersAsync(CancellationToken ct = default)
        {
            try
            {
                var customers = await _repo.ListAsync(ct);
                return Result<List<Customer>>.Success(customers);
            }
            catch (Exception ex)
            {
                return Result<List<Customer>>.Fail($"Failed to retrieve customers: {ex.Message}");
            }
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\