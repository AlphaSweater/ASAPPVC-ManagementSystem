using ASAPPVC.App.Models;
using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Services;
using FluentAssertions;
using Moq;

namespace ASAPPVC.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _products = new();
        private readonly Mock<IComponentRepository> _components = new();
        private readonly Mock<IProductMapper> _mapper = new();
        private readonly ProductService _sut;

        public ProductServiceTests()
        {
            _sut = new ProductService(_products.Object, _components.Object, _mapper.Object);
        }

        [Fact]
        public async Task CreateAsync_InvalidVm_ReturnsFailure()
        {
            var vm = new ProductFormVm { ProductName = "", Description = "", SellingPrice = 0m, SelectedProductComponents = new() };
            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeFalse();
            // Accept either of the validation messages produced by the service
            (res.Error?.Contains("Product name is required") == true || res.Error?.Contains("Product price must be greater than zero") == true)
                .Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_Valid_ReturnsCreatedProduct()
        {
            var compId = Guid.NewGuid();
            var vm = new ProductFormVm
            {
                ProductName = "P123",
                Description = "A valid description for test",
                SellingPrice = 9.99m,
                SelectedProductComponents = new List<ProductComponentVm> { new() { ComponentId = compId, RequiredQuantity = 1m } }
            };

            var component = new Component { Id = compId, UnitCost = 1m, UnitOfMeasure = Unit.Piece };
            _components.Setup(c => c.GetListByIdsAsync(It.IsAny<IEnumerable<Guid>>(), true, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Component> { component });

            var product = new Product { Id = Guid.NewGuid(), ProductName = vm.ProductName, Description = vm.Description, SellingPrice = vm.SellingPrice };
            _mapper.Setup(m => m.FromCreateVmAsync(vm, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(product);

            _products.Setup(p => p.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _products.Setup(p => p.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeTrue();
            res.Value.Should().NotBeNull();
            res.Value!.ProductName.Should().Be(vm.ProductName);
        }

        [Fact]
        public async Task ExistsAsync_EmptyCode_ReturnsFailure()
        {
            var res = await _sut.ExistsAsync("");
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Product code is required");
        }

        [Fact]
        public async Task ExistsAsync_CodeNotFound_ReturnsFalse()
        {
            _products.Setup(p => p.GetByCodeAsync(It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Product?)null);

            var res = await _sut.ExistsAsync("X-001");
            res.Ok.Should().BeTrue();
            res.Value.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_CodeFound_ReturnsTrue()
        {
            var prod = new Product { Id = Guid.NewGuid(), ProductCode = "X-001" };
            _products.Setup(p => p.GetByCodeAsync("X-001", true, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(prod);

            var res = await _sut.ExistsAsync("X-001");
            res.Ok.Should().BeTrue();
            res.Value.Should().BeTrue();
        }

        [Fact]
        public async Task GetDomainAsync_NotFound_ReturnsFailure()
        {
            _products.Setup(p => p.GetByIdOrCodeAsync(It.IsAny<Guid?>(), It.IsAny<string?>(), false, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Product?)null);

            var res = await _sut.GetDomainAsync(Guid.NewGuid());
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Product not found");
        }

        [Fact]
        public async Task GetDetailAsync_Found_ReturnsVm()
        {
            var prod = new Product { Id = Guid.NewGuid(), ProductCode = "C1", ProductName = "P1", Description = "D1", SellingPrice = 1m };
            _products.Setup(p => p.GetByIdOrCodeWithComponentsAsync(It.IsAny<Guid?>(), It.IsAny<string?>(), false, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(prod);

            var vm = new ProductDetailVm { Id = prod.Id, ProductCode = prod.ProductCode, ProductName = prod.ProductName, Description = prod.Description };
            _mapper.Setup(m => m.ToDetailVm(prod, true)).Returns(vm);

            var res = await _sut.GetDetailAsync(prod.Id);
            res.Ok.Should().BeTrue();
            res.Value.Should().BeEquivalentTo(vm);
        }

        [Fact]
        public async Task ListAsync_ReturnsMappedList()
        {
            var p1 = new Product { Id = Guid.NewGuid(), ProductCode = "A1", ProductName = "AA", Description = "D", SellingPrice = 1m };
            var p2 = new Product { Id = Guid.NewGuid(), ProductCode = "B1", ProductName = "BB", Description = "D2", SellingPrice = 2m };
            _products.Setup(p => p.GetListAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Product> { p1, p2 });

            _mapper.Setup(m => m.ToListVm(p1)).Returns(new ProductListVm { Id = p1.Id, ProductCode = p1.ProductCode, ProductName = p1.ProductName, Description = p1.Description, SellingPrice = p1.SellingPrice });
            _mapper.Setup(m => m.ToListVm(p2)).Returns(new ProductListVm { Id = p2.Id, ProductCode = p2.ProductCode, ProductName = p2.ProductName, Description = p2.Description, SellingPrice = p2.SellingPrice });

            var res = await _sut.ListAsync();
            res.Ok.Should().BeTrue();
            res.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsFailure()
        {
            var res = await _sut.DeleteAsync(Guid.Empty);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Product ID is required");
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFailure()
        {
            _products.Setup(p => p.RemoveByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var res = await _sut.DeleteAsync(Guid.NewGuid());
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Product not found");
        }

        [Fact]
        public async Task DeleteAsync_Found_ReturnsSuccess()
        {
            _products.Setup(p => p.RemoveByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _products.Setup(p => p.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var res = await _sut.DeleteAsync(Guid.NewGuid());
            res.Ok.Should().BeTrue();
        }
    }
}