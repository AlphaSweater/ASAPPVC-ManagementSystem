using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
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
            var vm = new ProductFormVm { Name = "", Description = "", Price = 0m, Components = null };
            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_MissingComponents_ReturnsFailure()
        {
            var compId = Guid.NewGuid();
            var vm = new ProductFormVm
            {
                Name = "P",
                Description = "D",
                Price = 1m,
                Components = new List<ProductComponentFormVm> { new() { ComponentId = compId, } }
            };

            _components.Setup(c => c.GetListByIdsAsync(It.IsAny<IEnumerable<Guid>>(), true, It.IsAny<System.Threading.CancellationToken>()))
                       .ReturnsAsync(new List<Component>());

            var res = await _sut.CreateAsync(vm);
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Some components do not exist");
        }

        [Fact]
        public async Task ExistsAsync_EmptyCode_ReturnsFailure()
        {
            var res = await _sut.ExistsAsync("");
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Product code is required");
        }
    }
}