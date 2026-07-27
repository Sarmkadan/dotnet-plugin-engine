using FluentAssertions;
using PluginEngine.Results;
using Xunit;

namespace PluginEngine.Tests;

public class PluginOperationResultExtensionsTests
{
    [Fact]
    public void ToBatchResult_NullResults_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((IEnumerable<PluginOperationResult>)null!).ToBatchResult();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToBatchResult_WithValidResults_ReturnsBatchResult()
    {
        // Arrange
        var results = new List<PluginOperationResult>
        {
            new() { Success = true, DurationMs = 10 },
            new() { Success = false, DurationMs = 20 }
        };

        // Act
        var batch = results.ToBatchResult();

        // Assert
        batch.Should().NotBeNull();
        batch.TotalDurationMs.Should().Be(30);
    }

    [Fact]
    public void HasFailures_NullResult_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((PluginOperationResult)null!).HasFailures();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasFailures_SuccessResult_ReturnsFalse()
    {
        // Arrange
        var result = new PluginOperationResult { Success = true };

        // Act
        var hasFailures = result.HasFailures();

        // Assert
        hasFailures.Should().BeFalse();
    }

    [Fact]
    public void HasFailures_FailureResult_ReturnsTrue()
    {
        // Arrange
        var result = new PluginOperationResult { Success = false };

        // Act
        var hasFailures = result.HasFailures();

        // Assert
        hasFailures.Should().BeTrue();
    }

    [Fact]
    public void GetDetailedSummary_SuccessResult_ReturnsFormattedSummary()
    {
        // Arrange
        var result = new PluginOperationResult { Success = true, DurationMs = 100, Message = "OK" };

        // Act
        var summary = result.GetDetailedSummary();

        // Assert
        summary.Should().Contain("Status: SUCCESS");
        summary.Should().Contain("Duration: 100ms");
        summary.Should().Contain("Message: OK");
    }

    [Fact]
    public void ToNonGeneric_WithGenericResult_ReturnsNonGenericResult()
    {
        // Arrange
        var genericResult = new PluginOperationResult<string> { Success = true, Message = "Data" };

        // Act
        var nonGeneric = ((PluginOperationResult)genericResult).ToNonGeneric();

        // Assert
        nonGeneric.Should().NotBeOfType<PluginOperationResult<string>>();
        nonGeneric.Should().BeOfType<PluginOperationResult>();
        nonGeneric.Success.Should().BeTrue();
        nonGeneric.Message.Should().Be("Data");
    }
}
