using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
using FluentAssertions;
using Moq;

namespace ASAPPVC.UnitTests.Services
{
    public class ComponentServiceTests
    {
        private readonly Mock<IComponentRepository> _repo = new();
        private readonly Mock<IComponentMapper> _mapper = new();
        private readonly ComponentService _sut;

        public ComponentServiceTests()
        {
            _sut = new ComponentService(_repo.Object, _mapper.Object);
        }

        // ---------------- Create: Invalid VM ----------------
        [Fact]
        public async Task CreateAsync_InvalidVm_ReturnsFailure()
        {
            // Arrange
            var vm = new ComponentFormVm { Name = "", StorageLocation = "  ", UnitCost = 0m, CurrentAmount = -1m };

            // Act
            var res = await _sut.CreateAsync(vm);

            // Assert
            res.Ok.Should().BeFalse();
            res.Error.Should().NotBeNull();
        }

        // ---------------- Create: Valid (calls mapper + repo) ----------------
        [Fact]
        public async Task CreateAsync_Valid_CallsMapperAndRepository_ReturnsCreated()
        {
            // Arrange
            var vm = new ComponentFormVm { Name = "Bolt", StorageLocation = "A1", UnitCost = 1.5m, CurrentAmount = 10m };
            var created = new Component { Id = Guid.NewGuid(), Name = "Bolt" };

            _mapper.Setup(m => m.FromCreateVmAsync(vm, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(created);

            _repo.Setup(r => r.AddAsync(created, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(created);

            // Act
            var res = await _sut.CreateAsync(vm);

            // Assert
            res.Ok.Should().BeTrue();
            res.Value.Should().Be(created);
            _repo.Verify(r => r.AddAsync(created, It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // ---------------- Exists: Empty code validation ----------------
        [Fact]
        public async Task ExistsAsync_EmptyCode_ReturnsFailure()
        {
            var res = await _sut.ExistsAsync("");
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Component code is required");
        }

        // ---------------- Exists: Component not found ----------------
        [Fact]
        public async Task ExistsAsync_ComponentNotFound_ReturnsFalse()
        {
            _repo.Setup(r => r.GetByCodeAsync("X", true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Component?)null);

            var res = await _sut.ExistsAsync("X");
            res.Ok.Should().BeTrue();
            res.Value.Should().BeFalse();
        }

        // ---------------- Exists: Component found but excluded by id ----------------
        [Fact]
        public async Task ExistsAsync_ComponentFoundButExcluded_ReturnsFalse()
        {
            var id = Guid.NewGuid();
            var comp = new Component { Id = id, ComponentCode = "X" };
            _repo.Setup(r => r.GetByCodeAsync("X", true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(comp);

            var res = await _sut.ExistsAsync("X", excludeId: id);
            res.Ok.Should().BeTrue();
            res.Value.Should().BeFalse();
        }

        // ---------------- Exists: Component found returns true ----------------
        [Fact]
        public async Task ExistsAsync_ComponentFound_ReturnsTrue()
        {
            var comp = new Component { Id = Guid.NewGuid(), ComponentCode = "X" };
            _repo.Setup(r => r.GetByCodeAsync("X", true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(comp);

            var res = await _sut.ExistsAsync("X");
            res.Ok.Should().BeTrue();
            res.Value.Should().BeTrue();
        }
    }
}