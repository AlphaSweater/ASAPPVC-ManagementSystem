using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
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
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _sut = new OrderService(_orders.Object, _customers.Object, _products.Object, _mapper.Object);
        }

        [Fact]
        public async Task CreateAsync_InvalidVm_ReturnsFailure()
        {
            var vm = new ASAPPVC.UI.Models.OrderFormVm { CustomerId = Guid.Empty, Products = null };
            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_CustomerNotFound_ReturnsFailure()
        {
            var vm = new ASAPPVC.UI.Models.OrderFormVm
            {
                CustomerId = Guid.NewGuid(),
                Products = new List<ASAPPVC.UI.Models.OrderProductFormVm> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
            };

            _customers.Setup(c => c.GetByIdAsync(vm.CustomerId, true, It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync((CustomerModel?)null);

            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("does not exist");
        }

        [Fact]
        public async Task GetDetailAsync_EmptyId_ReturnsFailure()
        {
            var res = await _sut.GetDetailAsync(Guid.Empty);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Order ID is required");
        }

        [Fact]
        public async Task DeleteAsync_EmptyId_ReturnsFailure()
        {
            var res = await _sut.DeleteAsync(Guid.Empty);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Order ID is required");
        }
    }
}