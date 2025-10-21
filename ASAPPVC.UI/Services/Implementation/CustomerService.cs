using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Customer;
using ASAPPVC.UI.Repositories.Interfaces;
using ASAPPVC.UI.Services.Interfaces;

namespace ASAPPVC.UI.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;
        public CustomerService(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<(bool Ok, string? Error, CustomerModel? Customer)> CreateAsync(CreateCustomerViewModel vm, CancellationToken ct = default)
        {
            var entity = new CustomerModel
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

        public async Task<List<CustomerModel>> ListAsync(CancellationToken ct = default)
        {
            return await _repo.ListAsync(ct);
        }
    }
}
