using System;
using PluginEngine.Exceptions;
using Xunit;

/// <summary>
/// xUnit tests for the extension methods that operate on <c>VersionMismatchException</c>:
/// formatted error message, critical-mismatch detection, context attachment and simplified message.
/// </summary>
public class VersionMismatchExceptionExtensionsTests
{
    /// <summary>
    /// Verifies that <c>GetFormattedErrorMessage</c> produces a multi-line report containing the header,
    /// component type and name, expected and actual versions, and the details section with the exception message.
    /// </summary>
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

    /// <summary>
    /// Verifies that calling <c>GetFormattedErrorMessage</c> on a null exception reference throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void GetFormattedErrorMessage_ShouldThrowArgumentNullException_WhenExceptionIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((VersionMismatchException)null!).GetFormattedErrorMessage());
    }

    /// <summary>
    /// Verifies that <c>IsCriticalVersionMismatch</c> returns <c>true</c> when the expected and actual major versions differ (1.x vs 2.x).
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>IsCriticalVersionMismatch</c> returns <c>false</c> when the major versions match and only the minor version differs (1.0.0 vs 1.5.0).
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>IsCriticalVersionMismatch</c> returns <c>false</c> when either version string cannot be parsed as a version (e.g. "abc" and "def").
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>IsCriticalVersionMismatch</c> returns <c>false</c> when the expected version is an empty string,
    /// and when the expected version property has been cleared to <c>null</c> after construction.
    /// </summary>
    [Fact]
    public void IsCriticalVersionMismatch_ShouldReturnFalse_WhenVersionsAreNullOrEmpty()
    {
        // Arrange
        var ex1 = new VersionMismatchException("Test", "", "1.0.0", "Type", "Name", null);
        var ex2 = new VersionMismatchException("Test", "1.0.0", "1.0.0", "Type", "Name", null);
        ex2.ExpectedVersion = null!; // Constructors reject null; the public property can still be cleared

        // Act
        var result1 = ex1.IsCriticalVersionMismatch();
        var result2 = ex2.IsCriticalVersionMismatch();

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    /// <summary>
    /// Verifies that <c>WithContext</c> returns an exception whose <c>Context</c> dictionary contains the supplied key and value.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>WithContext</c> throws <see cref="ArgumentNullException"/> when the context key is <c>null</c>.
    /// </summary>
    [Fact]
    public void WithContext_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        var ex = new VersionMismatchException("Test", "1.0.0", "1.0.0", "Type", "Name", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ex.WithContext(null!, "Value"));
    }

    /// <summary>
    /// Verifies that <c>GetSimplifiedMessage</c> returns the single-line form
    /// <c>VersionMismatch[Type:Name] Expected: X, Actual: Y</c>.
    /// </summary>
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
