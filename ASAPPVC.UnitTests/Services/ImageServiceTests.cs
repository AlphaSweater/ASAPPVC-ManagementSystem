using ASAPPVC.UI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ASAPPVC.UnitTests.Services
{
    public class ImageServiceTests
    {
        private readonly ImageServiceOptions _options = new()
        {
            MaxBytes = 5 * 1024 * 1024,
            AllowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" }
        };

        private readonly ImageService _sut;

        public ImageServiceTests()
        {
            _sut = new ImageService(Options.Create(_options));
        }

        [Fact]
        public async Task ProcessUploadAsync_NoFile_ReturnsFail()
        {
            // Arrange
            IFormFile? file = null;

            // Act
            var res = await _sut.ProcessUploadAsync(file!);

            // Assert
            res.Ok.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessBytesAsync_EmptyData_ReturnsFail()
        {
            var res = await _sut.ProcessBytesAsync(new byte[0], "image/png");
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Empty");
        }

        [Fact]
        public async Task ProcessBytesAsync_UnsupportedContentType_ReturnsFail()
        {
            var data = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var res = await _sut.ProcessBytesAsync(data, "image/xyz");
            res.Ok.Should().BeFalse();
            res.Error.Should().Contain("Unsupported");
        }
    }
}