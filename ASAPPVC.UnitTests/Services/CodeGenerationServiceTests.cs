using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Models.General;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Services;
using FluentAssertions;
using Moq;

namespace ASAPPVC.UnitTests.Services;

public class CodeGenerationServiceTests
{
    private readonly Mock<ICodeCountersRepository> _counters = new();
    private readonly CodeGenerationService _sut;

    public CodeGenerationServiceTests()
    {
        _sut = new CodeGenerationService(_counters.Object);
    }

    // ---------------- Product: Generate without category ----------------
    [Fact]
    public async Task GenerateProductCode_WithoutCategory_ReturnsCorrectFormat()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var request = new CodeGenerationRequest { Type = CodeType.Product };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-\d{4}-[A-Z0-9]$"); // e.g., PRD-0042-X
    }

    // ---------------- Product: Generate with category ----------------
    [Fact]
    public async Task GenerateProductCode_WithCategory_IncludesCategory()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        var request = new CodeGenerationRequest { Type = CodeType.Product, Category = "WIN" };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-WIN-\d{4}-[A-Z0-9]$"); // e.g., PRD-WIN-0015-X
    }

    // ---------------- Product: Generate with version ----------------
    [Fact]
    public async Task GenerateProductCode_WithVersion_IncludesVersion()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(7);

        var request = new CodeGenerationRequest { Type = CodeType.Product, Version = 3 };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-\d{4}-V03-[A-Z0-9]$"); // e.g., PRD-0007-V03-X
    }

    // ---------------- Product: Generate with category and version ----------------
    [Fact]
    public async Task GenerateProductCode_WithCategoryAndVersion_IncludesBoth()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(99);

        var request = new CodeGenerationRequest { Type = CodeType.Product, Category = "DRR", Version = 12 };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^PRD-DRR-\d{4}-V12-[A-Z0-9]$"); // e.g., PRD-DRR-0099-V12-X
    }

    // ---------------- Component: Generate Code----------------
    [Fact]
    public async Task GenerateComponentCode_WithoutCategory_ReturnsCorrectFormat()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Component, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(123);

        var request = new CodeGenerationRequest { Type = CodeType.Component };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^CMP-\d{5}-[A-Z0-9]$"); // e.g., CMP-00123-X
    }

    // ---------------- Order: Uses period key from timestamp ----------------
    [Fact]
    public async Task GenerateOrderCode_UsesPeriodKeyFromTimestamp()
    {
        // Arrange
        var when = new DateTime(2024, 3, 15, 10, 30, 0, DateTimeKind.Utc);
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Order, "202403", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(88);

        var request = new CodeGenerationRequest { Type = CodeType.Order, When = when };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^ORD-202403-\d{4}-[A-Z0-9]$"); // e.g., ORD-202403-0088-X
        _counters.Verify(r => r.IncrementAndGetAsync(CodeType.Order, "202403", CancellationToken.None), Times.Once);
    }

    // ---------------- Order: Different months use different counters ----------------
    [Fact]
    public async Task GenerateOrderCode_DifferentMonths_UseDifferentCounters()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Order, "202401", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Order, "202402", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var jan = new CodeGenerationRequest { Type = CodeType.Order, When = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) };
        var feb = new CodeGenerationRequest { Type = CodeType.Order, When = new DateTime(2024, 2, 10, 0, 0, 0, DateTimeKind.Utc) };

        // Act
        var janResult = await _sut.GenerateCodeAsync(jan);
        var febResult = await _sut.GenerateCodeAsync(feb);

        // Assert
        janResult.Value.Should().Contain("202401");
        febResult.Value.Should().Contain("202402");
        _counters.Verify(r => r.IncrementAndGetAsync(CodeType.Order, "202401", CancellationToken.None), Times.Once);
        _counters.Verify(r => r.IncrementAndGetAsync(CodeType.Order, "202402", CancellationToken.None), Times.Once);
    }

    // ---------------- PickingSlip: With related order and version ----------------
    [Fact]
    public async Task GeneratePickingSlipCode_WithOrderAndVersion_ReturnsCorrectFormat()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.PickingSlip, null, It.IsAny<CancellationToken>()))
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
        result.Value.Should().MatchRegex(@"^PSL-ORD0042-V01-[A-Z0-9]$");
    }

    // ---------------- PickingSlip: Missing related code fails ----------------
    [Fact]
    public async Task GeneratePickingSlipCode_WithoutRelatedCode_Fails()
    {
        // Arrange
        var request = new CodeGenerationRequest { Type = CodeType.PickingSlip, Version = 1 };

        // Act
        var result = await _sut.GenerateCodeAsync(request);

        // Assert
        result.Ok.Should().BeFalse();
        result.Error.Should().Be("RelatedCode is required for PickingSlip generation.");
    }

    // ---------------- PickingSlip: Missing version fails ----------------
    [Fact]
    public async Task GeneratePickingSlipCode_WithoutVersion_Fails()
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

    // ---------------- Counter: Calls repository to increment counter ----------------
    [Fact]
    public async Task GenerateCode_CallsRepositoryToIncrementCounter()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var request = new CodeGenerationRequest { Type = CodeType.Product };

        // Act
        await _sut.GenerateCodeAsync(request);

        // Assert
        _counters.Verify(r => r.IncrementAndGetAsync(CodeType.Product, null, CancellationToken.None), Times.Once);
        _counters.VerifyNoOtherCalls();
    }

    // ---------------- Counter: Increments on each call ----------------
    [Fact]
    public async Task GenerateMultipleCodes_IncrementsCounterEachTime()
    {
        // Arrange
        var sequence = 0;
        _counters.Setup(r => r.IncrementAndGetAsync(CodeType.Product, null, It.IsAny<CancellationToken>()))
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
        _counters.Verify(r => r.IncrementAndGetAsync(CodeType.Product, null, CancellationToken.None), Times.Exactly(3));
    }

    // ---------------- Checksum: Validates generated code ----------------
    [Fact]
    public async Task ValidateChecksum_ForGeneratedCode_IsTrue()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(It.IsAny<CodeType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var request = new CodeGenerationRequest { Type = CodeType.Product };
        var generated = await _sut.GenerateCodeAsync(request);

        // Act
        var isValid = _sut.ValidateChecksum(generated.Value!);

        // Assert
        isValid.Should().BeTrue();
    }

    // ---------------- Checksum: Detects tampered checksum ----------------
    [Fact]
    public async Task ValidateChecksum_TamperedChecksum_IsFalse()
    {
        // Arrange: generate a valid code first
        _counters.Setup(r => r.IncrementAndGetAsync(It.IsAny<CodeType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var generated = await _sut.GenerateCodeAsync(new CodeGenerationRequest { Type = CodeType.Product });
        generated.Ok.Should().BeTrue();

        // Act: flip ONLY the checksum character deterministically
        var code = generated.Value!;
        var lastDash = code.LastIndexOf('-');
        var prefix = code[..(lastDash + 1)];
        var originalChecksum = code[(lastDash + 1)..]; // 1 char
        var tamperedChecksum = originalChecksum == "Z" ? "A" : "Z";
        var tampered = prefix + tamperedChecksum;

        // Assert
        _sut.ValidateChecksum(code).Should().BeTrue("control check (original should be valid)");
        _sut.ValidateChecksum(tampered).Should().BeFalse("modified checksum should fail validation");
    }

    // ---------------- Checksum: Invalid formats return false ----------------
    [Fact]
    public void ValidateChecksum_InvalidFormats_ReturnFalse()
    {
        _sut.ValidateChecksum("INVALID").Should().BeFalse();
        _sut.ValidateChecksum("").Should().BeFalse();
        _sut.ValidateChecksum("PRD-0001").Should().BeFalse();     // Missing checksum
        _sut.ValidateChecksum("PRD-0001-AB").Should().BeFalse();  // Multi-char checksum
    }

    // ---------------- Error Handling: Repository exception returns failure ----------------
    [Fact]
    public async Task GenerateCode_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        _counters.Setup(r => r.IncrementAndGetAsync(It.IsAny<CodeType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.GenerateCodeAsync(new CodeGenerationRequest { Type = CodeType.Product });

        // Assert
        result.Ok.Should().BeFalse();
        result.Error.Should().Contain("Database connection failed");
    }

    // ---------------- Error Handling: Invalid version returns failure ----------------
    [Fact]
    public async Task GenerateCode_WithInvalidVersion_ReturnsFailure()
    {
        // Act
        var result = await _sut.GenerateCodeAsync(new CodeGenerationRequest { Type = CodeType.Product, Version = 150 });

        // Assert
        result.Ok.Should().BeFalse();
        result.Error.Should().Contain("Version must be between 1 and 99");
    }
}