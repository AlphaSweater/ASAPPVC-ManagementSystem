using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Customer;
using FluentAssertions;
using Moq;

namespace ASAPPVC.UnitTests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _repo = new();
        private readonly CustomerService _sut;

        public CustomerServiceTests()
        {
            _sut = new CustomerService(_repo.Object);
        }

        // ---------------- Create: Trims and persists, returns created entity ----------------
        [Fact]
        public async Task CreateAsync_TrimsAndPersists_ReturnsTupleWithEntity()
        {
            // Arrange
            var vm = new CreateCustomerViewModel
            {
                FirstName = " John ",
                LastName = " Doe ",
                PhoneNumber = " 123 ",
                Email = " a@b.com ",
                Company = " Co "
            };

            // Return the same CustomerModel that the service passes into AddAsync
            _repo.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                 .Returns((Customer c, CancellationToken ct) => Task.FromResult(c));

            // SaveAsync returns an int (rows affected) -> return a completed int task
            _repo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

            // Act
            var (ok, error, customer) = await _sut.CreateAsync(vm);

            // Assert
            ok.Should().BeTrue();
            error.Should().BeNull();
            customer.Should().NotBeNull();
            customer!.Name.Should().Be("John");
            customer.Surname.Should().Be("Doe");
            customer.PhoneNumber.Should().Be("123");
            customer.Email.Should().Be("a@b.com");
        }

        // ---------------- List: Returns repository list unchanged ----------------
        [Fact]
        public async Task ListAsync_ReturnsRepositoryList()
        {
            // Arrange
            var list = new List<Customer> { new Customer { Id = System.Guid.NewGuid(), Name = "A", Surname = "B" } };
            _repo.Setup(r => r.ListAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(list);

            // Act
            var res = await _sut.ListAsync();

            // Assert
            res.Should().BeSameAs(list);
        }
    }
}