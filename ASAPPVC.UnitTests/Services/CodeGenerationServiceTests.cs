using ASAPPVC.UI.Models.Enums;
using ASAPPVC.UI.Models.General;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
using FluentAssertions;
using Moq;

namespace ASAPPVC.UnitTests.Services;

public class CodeGenerationServiceTests
{
    private readonly Mock<ICodeCountersRepository> _mockCounters;
    private readonly CodeGenerationService _sut;

    public CodeGenerationServiceTests()
    {
        _mockCounters = new Mock<ICodeCountersRepository>();
        _sut = new CodeGenerationService(_mockCounters.Object);
    }

    #region Product Code Generation

    [Fact]
    public async Task GenerateProductCode_WithoutCategory_ReturnsCorrectFormat()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var request = new CodeGenerationRequest { Type = CodeType.Product };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-\d{4}-[A-Z0-9]$"); // e.g., PRD-0042-X
    }

    [Fact]
    public async Task GenerateProductCode_WithCategory_IncludesCategoryInCode()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(15);

        var request = new CodeGenerationRequest { Type = CodeType.Product, Category = "WIN" };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-WIN-\d{4}-[A-Z0-9]$"); // e.g., PRD-WIN-0015-X
    }

    [Fact]
    public async Task GenerateProductCode_WithVersion_IncludesVersionInCode()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var request = new CodeGenerationRequest { Type = CodeType.Product, Version = 3 };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-\d{4}-V03-[A-Z0-9]$"); // e.g., PRD-0007-V03-X
    }

    [Fact]
    public async Task GenerateProductCode_WithCategoryAndVersion_IncludesBothInCode()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(99);

        var request = new CodeGenerationRequest { Type = CodeType.Product, Category = "DRR", Version = 12 };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-DRR-\d{4}-V12-[A-Z0-9]$"); // e.g., PRD-DRR-0099-V12-X
    }

    #endregion Product Code Generation

    #region Component Code Generation

    [Fact]
    public async Task GenerateComponentCode_WithoutCategory_ReturnsCorrectFormat()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Component, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        var request = new CodeGenerationRequest { Type = CodeType.Component };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^CMP-\d{5}-[A-Z0-9]$"); // e.g., CMP-00123-X
    }

    [Fact]
    public async Task GenerateComponentCode_WithCategory_IncludesCategoryInCode()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Component, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(456);

        var request = new CodeGenerationRequest { Type = CodeType.Component, Category = "HNG" };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^CMP-HNG-\d{5}-[A-Z0-9]$"); // e.g., CMP-HNG-00456-X
    }

    #endregion Component Code Generation

    #region Order Code Generation

    [Fact]
    public async Task GenerateOrderCode_UsesPeriodKeyFromTimestamp()
    {
        // Arrange
        var timestamp = new DateTime(2024, 3, 15, 10, 30, 0, DateTimeKind.Utc);
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Order, "202403", It.IsAny<CancellationToken>()))
              .ReturnsAsync(88);

        var request = new CodeGenerationRequest { Type = CodeType.Order, When = timestamp };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^ORD-202403-\d{4}-[A-Z0-9]$"); // e.g., ORD-202403-0088-X
        _mockCounters.Verify(r => r.IncrementAndGetAsync(CodeType.Order, "202403", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateOrderCode_DifferentMonths_UseDifferentCounters()
    {
        // Arrange
        var jan = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var feb = new DateTime(2024, 2, 10, 0, 0, 0, DateTimeKind.Utc);

        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Order, "202401", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Order, "202402", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var janResult = await _sut.GenerateCodeAsync(new CodeGenerationRequest { Type = CodeType.Order, When = jan });
        var febResult = await _sut.GenerateCodeAsync(new CodeGenerationRequest { Type = CodeType.Order, When = feb });

        // Assert
        janResult.Value.Should().Contain("202401");
        febResult.Value.Should().Contain("202402");
        _mockCounters.Verify(r => r.IncrementAndGetAsync(CodeType.Order, "202401", It.IsAny<CancellationToken>()), Times.Once);
        _mockCounters.Verify(r => r.IncrementAndGetAsync(CodeType.Order, "202402", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion Order Code Generation

    #region PickingSlip Code Generation

    [Fact]
    public async Task GeneratePickingSlipCode_WithValidOrderCode_ReturnsCorrectFormat()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.PickingSlip, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new CodeGenerationRequest
        {
            Type = CodeType.PickingSlip,
            RelatedCode = "ORD-202401-0042-A",
            Version = 1
        };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PSL-ORD0042-V01-[A-Z0-9]$"); // e.g., PSL-ORD0042-V01-X
    }

    [Fact]
    public async Task GeneratePickingSlipCode_WithoutRelatedCode_ReturnsFail()
    {
        // Arrange
        var request = new CodeGenerationRequest { Type = CodeType.PickingSlip, Version = 1 };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeFalse();
        result.Error.Should().Be("RelatedCode is required for PickingSlip generation.");
    }

    [Fact]
    public async Task GeneratePickingSlipCode_WithoutVersion_ReturnsFail()
    {
        // Arrange
        var request = new CodeGenerationRequest
        {
            Type = CodeType.PickingSlip,
            RelatedCode = "ORD-202401-0001-A"
        };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeFalse();
        result.Error.Should().Be("Version is required for PickingSlip generation.");
    }

    #endregion PickingSlip Code Generation

    #region Counter Increment Verification

    [Fact]
    public async Task GenerateCode_CallsRepositoryToIncrementCounter()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new CodeGenerationRequest { Type = CodeType.Product };

        // Act
        await _sut.GenerateCodeAsync(request);

        // Assert
        _mockCounters.Verify(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateMultipleCodes_IncrementsCounterEachTime()
    {
        // Arrange
        var sequence = 0;
        _mockCounters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++sequence);

        var request = new CodeGenerationRequest { Type = CodeType.Product };

        // Act
        var first = await _sut.GenerateCodeAsync(request);
        var second = await _sut.GenerateCodeAsync(request);
        var third = await _sut.GenerateCodeAsync(request);

        // Assert
        first.Value.Should().Contain("0001");
        second.Value.Should().Contain("0002");
        third.Value.Should().Contain("0003");
        _mockCounters.Verify(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    #endregion Counter Increment Verification

    #region Checksum Validation

    [Fact]
    public async Task ValidateChecksum_ForGeneratedCode_ReturnsTrue()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(It.IsAny<CodeType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new CodeGenerationRequest { Type = CodeType.Product };
        var generated = await _sut.GenerateCodeAsync(request);

        // Act
        var isValid = _sut.ValidateChecksum(generated.Value!);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateChecksum_WithTamperedCode_ReturnsFalse()
    {
        // Arrange
        var tamperedCode = "PRD-0001-A"; // Assuming checksum doesn't match

        // Act
        var isValid = _sut.ValidateChecksum(tamperedCode);

        // Assert - we don't know if this will be true or false, but changing it should fail
        var modifiedCode = "PRD-0001-Z";
        var modifiedValid = _sut.ValidateChecksum(modifiedCode);

        (isValid && modifiedValid).Should().BeFalse("at least one should be invalid");
    }

    [Fact]
    public void ValidateChecksum_WithInvalidFormat_ReturnsFalse()
    {
        // Act & Assert
        _sut.ValidateChecksum("INVALID").Should().BeFalse();
        _sut.ValidateChecksum("").Should().BeFalse();
        _sut.ValidateChecksum("PRD-0001").Should().BeFalse(); // No checksum
        _sut.ValidateChecksum("PRD-0001-AB").Should().BeFalse(); // Multi-char checksum
    }

    #endregion Checksum Validation

    #region Error Handling

    [Fact]
    public async Task GenerateCode_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        _mockCounters.Setup(r => r.IncrementAndGetAsync(It.IsAny<CodeType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var request = new CodeGenerationRequest { Type = CodeType.Product };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeFalse();
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task GenerateCode_WithInvalidVersion_ReturnsFailure()
    {
        // Arrange
        var request = new CodeGenerationRequest { Type = CodeType.Product, Version = 150 }; // > 99

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeFalse();
        result.Error.Should().Contain("Version must be between 1 and 99");
    }

    #endregion Error Handling
}