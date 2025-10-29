using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
using ASAPPVC.UI.ViewModels.Customer;
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

            _repo.Setup(r => r.AddAsync(It.IsAny<CustomerModel>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            _repo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

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

        [Fact]
        public async Task ListAsync_ReturnsRepositoryList()
        {
            // Arrange
            var list = new List<CustomerModel> { new CustomerModel { Id = System.Guid.NewGuid(), Name = "A", Surname = "B" } };
            _repo.Setup(r => r.ListAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(list);

            // Act
            var res = await _sut.ListAsync();

            // Assert
            res.Should().BeSameAs(list);
        }
    }
}