using System.Collections.Generic;
using Xunit;
using PluginEngine.Configuration;

namespace dotnet_plugin_engine.Tests
{
    public class PluginEngineOptionsTests
    {
        [Fact]
        public void DefaultOptions_ShouldBeValid_AndHaveNoValidationErrors()
        {
            // Arrange
            var options = new PluginEngineOptions();

            // Act
            var isValid = options.IsValid();
            var errors = options.GetValidationErrors();

            // Assert
            Assert.True(isValid, "Default options should be considered valid.");
            Assert.Empty(errors);
        }

        [Fact]
        public void InvalidPluginDirectory_ShouldBeInvalid_AndReportError()
        {
            // Arrange
            var options = new PluginEngineOptions
            {
                PluginDirectory = "   " // whitespace only
            };

            // Act
            var isValid = options.IsValid();
            var errors = options.GetValidationErrors();

            // Assert
            Assert.False(isValid);
            Assert.Contains("Plugin directory cannot be empty.", errors);
        }

        [Fact]
        public void InvalidHotReloadCheckInterval_ShouldBeInvalid_AndReportError()
        {
            // Arrange
            var options = new PluginEngineOptions
            {
                HotReloadCheckIntervalMs = 0
            };

            // Act
            var isValid = options.IsValid();
            var errors = options.GetValidationErrors();

            // Assert
            Assert.False(isValid);
            Assert.Contains("Hot reload check interval must be greater than 0.", errors);
        }

        [Fact]
        public void InvalidOperationTimeout_ShouldBeInvalid_AndReportError()
        {
            // Arrange
            var options = new PluginEngineOptions
            {
                OperationTimeoutMs = -1
            };

            // Act
            var isValid = options.IsValid();
            var errors = options.GetValidationErrors();

            // Assert
            Assert.False(isValid);
            Assert.Contains("Operation timeout must be greater than 0.", errors);
        }

        [Fact]
        public void InvalidMaxConcurrentPluginLoads_ShouldBeInvalid_AndReportError()
        {
            // Arrange
            var options = new PluginEngineOptions
            {
                MaxConcurrentPluginLoads = 0
            };

            // Act
            var isValid = options.IsValid();
            var errors = options.GetValidationErrors();

            // Assert
            Assert.False(isValid);
            Assert.Contains("Max concurrent plugin loads must be greater than 0.", errors);
        }

        [Fact]
        public void InvalidDependencyCacheTtlMinutes_ShouldBeInvalid_AndReportError()
        {
            // Arrange
            var options = new PluginEngineOptions
            {
                DependencyCacheTtlMinutes = 0
            };

            // Act
            var isValid = options.IsValid();
            var errors = options.GetValidationErrors();

            // Assert
            Assert.False(isValid);
            Assert.Contains("Dependency cache TTL must be greater than 0.", errors);
        }

        [Fact]
        public void InvalidMaxDependencyResolutionAttempts_ShouldBeInvalid_AndReportError()
        {
            // Arrange
            var options = new PluginEngineOptions
            {
                MaxDependencyResolutionAttempts = -5
            };

            // Act
            var isValid = options.IsValid();
            var errors = options.GetValidationErrors();

            // Assert
            Assert.False(isValid);
            Assert.Contains("Max dependency resolution attempts must be greater than 0.", errors);
        }

        [Fact]
        public void SettingLogLevel_ShouldPersistValue()
        {
            // Arrange
            var options = new PluginEngineOptions
            {
                LogLevel = LogLevel.Error
            };

            // Act & Assert
            Assert.Equal(LogLevel.Error, options.LogLevel);
        }
    }
}
