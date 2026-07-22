using Xunit;
using System;
using PluginEngine.Exceptions;

public class PluginIncompatibleExceptionTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var constraint = ">=1.0.0";
        var hostVersion = "2.0.0";

        // Act
        var exception = new PluginIncompatibleException(pluginName, constraint, hostVersion);

        // Assert
        Assert.Equal(constraint, exception.DeclaredConstraint);
        Assert.Equal(hostVersion, exception.HostEngineVersion);
        Assert.Contains(pluginName, exception.Message);
        Assert.Contains(hostVersion, exception.Message);
        Assert.Contains(constraint, exception.Message);
        Assert.Equal("PLUGIN_INCOMPATIBLE", exception.ErrorCode);
    }

    [Theory]
    [InlineData(null, ">=1.0.0", "2.0.0")]
    [InlineData("", ">=1.0.0", "2.0.0")]
    [InlineData("TestPlugin", null, "2.0.0")]
    [InlineData("TestPlugin", "", "2.0.0")]
    [InlineData("TestPlugin", ">=1.0.0", null)]
    [InlineData("TestPlugin", ">=1.0.0", "")]
    public void Constructor_WithNullOrEmptyParameters_ThrowsArgumentException(string pluginName, string constraint, string hostVersion)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PluginIncompatibleException(pluginName, constraint, hostVersion));
    }

    [Fact]
    public void DeclaredConstraint_Getter_ReturnsCorrectValue()
    {
        // Arrange
        var constraint = "<2.0.0";
        var exception = new PluginIncompatibleException("TestPlugin", constraint, "1.5.0");

        // Act & Assert
        Assert.Equal(constraint, exception.DeclaredConstraint);
    }

    [Fact]
    public void HostEngineVersion_Getter_ReturnsCorrectValue()
    {
        // Arrange
        var hostVersion = "1.2.3";
        var exception = new PluginIncompatibleException("TestPlugin", ">=1.0.0", hostVersion);

        // Act & Assert
        Assert.Equal(hostVersion, exception.HostEngineVersion);
    }

    [Fact]
    public void ToString_ContainsExpectedInformation()
    {
        // Arrange
        var pluginName = "MyPlugin";
        var constraint = ">=1.0.0";
        var hostVersion = "2.0.0";
        var exception = new PluginIncompatibleException(pluginName, constraint, hostVersion);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.Contains("PLUGIN_INCOMPATIBLE", result);
        Assert.Contains(pluginName, result);
        Assert.Contains(hostVersion, result);
        Assert.Contains(constraint, result);
        Assert.Contains("Declared Constraint:", result);
        Assert.Contains("Host Engine Version:", result);
    }

    [Fact]
    public void ToString_WithEntityId_ContainsEntityId()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var exception = new PluginIncompatibleException("TestPlugin", ">=1.0.0", "2.0.0");
        exception.WithEntityId(entityId);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.Contains(entityId.ToString(), result);
    }

    [Fact]
    public void ToString_WithContext_ContainsContext()
    {
        // Arrange
        var exception = new PluginIncompatibleException("TestPlugin", ">=1.0.0", "2.0.0");
        exception.WithContext("PluginId", "12345");
        exception.WithContext("Severity", "High");

        // Act
        var result = exception.ToString();

        // Assert
        Assert.Contains("PluginId=12345", result);
        Assert.Contains("Severity=High", result);
    }

    [Fact]
    public void ToString_WithInnerException_ContainsInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new PluginIncompatibleException("TestPlugin", ">=1.0.0", "2.0.0", innerException);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.Contains("Inner error", result);
    }

    [Fact]
    public void ErrorCode_IsSetCorrectly()
    {
        // Arrange & Act
        var exception = new PluginIncompatibleException("TestPlugin", ">=1.0.0", "2.0.0");

        // Assert
        Assert.Equal("PLUGIN_INCOMPATIBLE", exception.ErrorCode);
    }

    [Fact]
    public void InheritsFromPluginException()
    {
        // Arrange & Act
        var exception = new PluginIncompatibleException("TestPlugin", ">=1.0.0", "2.0.0");

        // Assert
        Assert.IsType<PluginException>(exception);
    }

    [Fact]
    public void Message_ContainsAllRelevantInformation()
    {
        // Arrange
        var pluginName = "AwesomePlugin";
        var constraint = ">=3.0.0";
        var hostVersion = "2.5.0";
        var exception = new PluginIncompatibleException(pluginName, constraint, hostVersion);

        // Act
        var message = exception.Message;

        // Assert
        Assert.Equal($"Plugin '{pluginName}' is incompatible with the host engine version {hostVersion}. Constraint '{constraint}' is not satisfied.", message);
    }

    [Fact]
    public void Constructor_WithFourParameters_IncludesInnerException()
    {
        // Arrange
        var innerException = new ArgumentNullException("paramName");
        var exception = new PluginIncompatibleException("TestPlugin", ">=1.0.0", "2.0.0", innerException);

        // Assert
        Assert.Same(innerException, exception.InnerException);
    }
}
