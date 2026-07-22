using System;
using PluginEngine.Exceptions;
using Xunit;

namespace PluginEngine.Tests
{
    public class PluginIncompatibleExceptionTests
    {
        [Fact]
        public void Constructor_SetsPropertiesAndBaseMessage()
        {
            // Arrange
            var pluginName = "MyPlugin";
            var constraint = ">=1.0.0";
            var hostVersion = "2.0.0";

            // Act
            var ex = new PluginIncompatibleException(pluginName, constraint, hostVersion);

            // Assert
            Assert.Equal(constraint, ex.DeclaredConstraint);
            Assert.Equal(hostVersion, ex.HostEngineVersion);
            Assert.Equal(
                $"Plugin '{pluginName}' is incompatible with the host engine version {hostVersion}. Constraint '{constraint}' is not satisfied.",
                ex.Message);
            Assert.Equal("PLUGIN_INCOMPATIBLE", ex.ErrorCode);
        }

        [Fact]
        public void ToString_ContainsBaseMessageAndAdditionalInfo()
        {
            // Arrange
            var pluginName = "TestPlugin";
            var constraint = "<2.0.0";
            var hostVersion = "1.5.0";
            var ex = new PluginIncompatibleException(pluginName, constraint, hostVersion);

            // Act
            var result = ex.ToString();

            // Assert
            // Base ToString includes the exception type and message
            Assert.Contains(ex.GetType().FullName, result);
            Assert.Contains(ex.Message, result);
            // Additional lines added by the override
            Assert.Contains($"Declared Constraint: {constraint}", result);
            Assert.Contains($"Host Engine Version: {hostVersion}", result);
        }

        [Fact]
        public void Constructor_AllowsNullValues_PropertiesReflectNull()
        {
            // Arrange
            string pluginName = null!;
            string constraint = null!;
            string hostVersion = null!;

            // Act
            var ex = new PluginIncompatibleException(pluginName, constraint, hostVersion);

            // Assert
            Assert.Null(ex.DeclaredConstraint);
            Assert.Null(ex.HostEngineVersion);
            // Message will contain the word "null" where the values were interpolated
            Assert.Contains("null", ex.Message);
        }

        [Fact]
        public void InheritsFromPluginException()
        {
            // Arrange & Act
            var ex = new PluginIncompatibleException("p", "c", "v");

            // Assert
            Assert.IsAssignableFrom<PluginException>(ex);
        }
    }
}
