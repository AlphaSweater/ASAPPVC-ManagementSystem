using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Services;
using FluentAssertions;
using Moq;

namespace ASAPPVC.UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orders = new();
        private readonly Mock<ICustomerRepository> _customers = new();
        private readonly Mock<IProductRepository> _products = new();
        private readonly Mock<IOrderMapper> _mapper = new();
        private readonly Mock<IAuthService> _authService = new();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _sut = new OrderService(
                _orders.Object,
                _customers.Object,
                _products.Object,
                _mapper.Object,
                _authService.Object);
            // Return a non-empty GUID to simulate an authenticated user for create/update tests
            _authService.Setup(a => a.GetCurrentUserIdAsync(It.IsAny<System.Threading.CancellationToken>()))
                        .ReturnsAsync(Guid.NewGuid());
        }

        // ---------------- Create: Invalid view model returns failure ----------------
        [Fact]
        public async Task CreateAsync_InvalidVm_ReturnsFailure()
        {
            var vm = new OrderFormVm { CustomerId = Guid.Empty, Products = null };
            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeFalse();
        }

        // ---------------- Create: Customer not found returns failure ----------------
        [Fact]
        public async Task CreateAsync_CustomerNotFound_ReturnsFailure()
        {
            var vm = new OrderFormVm
            {
                CustomerId = Guid.NewGuid(),
                Products = new List<OrderProductVm> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };

            _customers.Setup(c => c.GetByIdAsync(vm.CustomerId, true, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Customer?)null);

            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("does not exist");
        }

        // ---------------- GetDetail: Empty id returns failure ----------------
        [Fact]
        public async Task GetDetailAsync_EmptyId_ReturnsFailure()
        {
            var res = await _sut.GetDetailAsync(Guid.Empty);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Order ID is required");
        }

        // ---------------- Delete: Empty id returns failure ----------------
        [Fact]
        public async Task DeleteAsync_EmptyId_ReturnsFailure()
        {
            var res = await _sut.DeleteAsync(Guid.Empty);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Order ID is required");
        }
    }
}