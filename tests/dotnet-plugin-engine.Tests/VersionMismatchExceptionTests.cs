using Xunit;

namespace dotnet_plugin_engine.Tests;

public class VersionMismatchExceptionTests
{
    [Fact]
    public void Constructor_NoMessage_NoPropertiesSet()
    {
        // Arrange
        var exception = new VersionMismatchException();

        // Assert
        Assert.Null(exception.ExpectedVersion);
        Assert.Null(exception.ActualVersion);
        Assert.Null(exception.ComponentType);
        Assert.Null(exception.ComponentName);
    }

    [Fact]
    public void Constructor_Message_NoPropertiesSet()
    {
        // Arrange
        var message = "Test message";
        var exception = new VersionMismatchException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.ExpectedVersion);
        Assert.Null(exception.ActualVersion);
        Assert.Null(exception.ComponentType);
        Assert.Null(exception.ComponentName);
    }

    [Fact]
    public void Constructor_FullDetails_PropertiesSet()
    {
        // Arrange
        var expectedVersion = "1.2.3";
        var actualVersion = "4.5.6";
        var componentType = "Plugin";
        var componentName = "Test Plugin";
        var exception = new VersionMismatchException("Test message", expectedVersion, actualVersion, componentType, componentName);

        // Assert
        Assert.Equal(expectedVersion, exception.ExpectedVersion);
        Assert.Equal(actualVersion, exception.ActualVersion);
        Assert.Equal(componentType, exception.ComponentType);
        Assert.Equal(componentName, exception.ComponentName);
    }

    [Fact]
    public void Constructor_FullDetailsWithInnerException_PropertiesSet()
    {
        // Arrange
        var expectedVersion = "1.2.3";
        var actualVersion = "4.5.6";
        var componentType = "Plugin";
        var componentName = "Test Plugin";
        var innerException = new Exception("Inner exception");
        var exception = new VersionMismatchException("Test message", expectedVersion, actualVersion, componentType, componentName, innerException);

        // Assert
        Assert.Equal(expectedVersion, exception.ExpectedVersion);
        Assert.Equal(actualVersion, exception.ActualVersion);
        Assert.Equal(componentType, exception.ComponentType);
        Assert.Equal(componentName, exception.ComponentName);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void ToString_NoProperties_ReturnsMessage()
    {
        // Arrange
        var exception = new VersionMismatchException("Test message");

        // Act
        var result = exception.ToString();

        // Assert
        Assert.Equal("Test message", result);
    }

    [Fact]
    public void ToString_FullDetails_ReturnsDetailedMessage()
    {
        // Arrange
        var expectedVersion = "1.2.3";
        var actualVersion = "4.5.6";
        var componentType = "Plugin";
        var componentName = "Test Plugin";
        var exception = new VersionMismatchException("Test message", expectedVersion, actualVersion, componentType, componentName);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.Contains(expectedVersion, result);
        Assert.Contains(actualVersion, result);
        Assert.Contains(componentType, result);
        Assert.Contains(componentName, result);
    }
}
