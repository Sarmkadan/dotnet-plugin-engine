using Xunit;
using System;
using PluginEngine.Exceptions;

public class VersionMismatchExceptionExtensionsTests
{
    [Fact]
    public void GetFormattedErrorMessage_ShouldReturnFormattedString_WhenExceptionHasDetails()
    {
        // Arrange
        var ex = new VersionMismatchException("Test error message", "1.0.0", "2.0.0", "Plugin", "MyPlugin", null);

        // Act
        var result = ex.GetFormattedErrorMessage();

        // Assert
        Assert.Contains("Version Mismatch Error:", result);
        Assert.Contains("Component: Plugin - MyPlugin", result);
        Assert.Contains("Expected Version: 1.0.0", result);
        Assert.Contains("Actual Version: 2.0.0", result);
        Assert.Contains("Details:", result);
        Assert.Contains("Test error message", result);
    }

    [Fact]
    public void GetFormattedErrorMessage_ShouldThrowArgumentNullException_WhenExceptionIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((VersionMismatchException)null).GetFormattedErrorMessage());
    }

    [Fact]
    public void IsCriticalVersionMismatch_ShouldReturnTrue_WhenMajorVersionsDiffer()
    {
        // Arrange
        var ex = new VersionMismatchException("Test", "1.0.0", "2.0.0", "Type", "Name", null);

        // Act
        var result = ex.IsCriticalVersionMismatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCriticalVersionMismatch_ShouldReturnFalse_WhenMajorVersionsMatch()
    {
        // Arrange
        var ex = new VersionMismatchException("Test", "1.0.0", "1.5.0", "Type", "Name", null);

        // Act
        var result = ex.IsCriticalVersionMismatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCriticalVersionMismatch_ShouldReturnFalse_WhenVersionsAreInvalidFormat()
    {
        // Arrange
        var ex = new VersionMismatchException("Test", "abc", "def", "Type", "Name", null);

        // Act
        var result = ex.IsCriticalVersionMismatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCriticalVersionMismatch_ShouldReturnFalse_WhenVersionsAreNullOrEmpty()
    {
        // Arrange
        var ex1 = new VersionMismatchException("Test", "", "1.0.0", "Type", "Name", null);
        var ex2 = new VersionMismatchException("Test", null, "1.0.0", "Type", "Name", null);

        // Act
        var result1 = ex1.IsCriticalVersionMismatch();
        var result2 = ex2.IsCriticalVersionMismatch();

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void WithContext_ShouldAddContext_WhenCalledWithValidKey()
    {
        // Arrange
        var ex = new VersionMismatchException("Test", "1.0.0", "1.0.0", "Type", "Name", null);

        // Act
        var newEx = ex.WithContext("CustomKey", "CustomValue");

        // Assert
        Assert.True(newEx.Context.ContainsKey("CustomKey"));
        Assert.Equal("CustomValue", newEx.Context["CustomKey"]);
    }

    [Fact]
    public void WithContext_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        var ex = new VersionMismatchException("Test", "1.0.0", "1.0.0", "Type", "Name", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ex.WithContext(null, "Value"));
    }

    [Fact]
    public void GetSimplifiedMessage_ShouldReturnCorrectFormat()
    {
        // Arrange
        var ex = new VersionMismatchException("Test", "1.0.0", "2.0.0", "Plugin", "TestPlugin", null);

        // Act
        var result = ex.GetSimplifiedMessage();

        // Assert
        Assert.Equal("VersionMismatch[Plugin:TestPlugin] Expected: 1.0.0, Actual: 2.0.0", result);
    }
}
