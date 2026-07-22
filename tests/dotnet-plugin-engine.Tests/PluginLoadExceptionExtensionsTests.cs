using System;
using PluginEngine.Exceptions;
using Xunit;

namespace PluginEngine.Tests
{
    public class PluginLoadExceptionExtensionsTests
    {
        [Fact]
        public void IsLoadStage_ReturnsTrue_WhenExceptionLoadStageMatchesGivenStage()
        {
            // Arrange
            var exception = new PluginLoadException(
                "Test message",
                "TestPlugin",
                "/path/to/plugin.dll",
                PluginLoadStage.TypeLoading);

            // Act
            var result = exception.IsLoadStage(PluginLoadStage.TypeLoading);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsLoadStage_ReturnsFalse_WhenExceptionLoadStageDoesNotMatchGivenStage()
        {
            // Arrange
            var exception = new PluginLoadException(
                "Test message",
                "TestPlugin",
                "/path/to/plugin.dll",
                PluginLoadStage.AssemblyResolution);

            // Act
            var result = exception.IsLoadStage(PluginLoadStage.TypeLoading);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsLoadStage_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            PluginLoadException exception = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.IsLoadStage(PluginLoadStage.Unknown));
        }

        [Fact]
        public void GetLoadFailureSummary_ReturnsFormattedString_WithAllProperties()
        {
            // Arrange
            var exception = new PluginLoadException(
                "Assembly not found",
                "MyAwesomePlugin",
                "/plugins/MyAwesomePlugin.dll",
                PluginLoadStage.AssemblyResolution);

            // Act
            var result = exception.GetLoadFailureSummary();

            // Assert
            Assert.Equal(
                "Failed to load plugin 'MyAwesomePlugin' from '/plugins/MyAwesomePlugin.dll' at stage AssemblyResolution.",
                result);
        }

        [Fact]
        public void GetLoadFailureSummary_ReturnsFormattedString_WithEmptyStrings()
        {
            // Arrange
            var exception = new PluginLoadException(
                "Test message",
                string.Empty,
                string.Empty,
                PluginLoadStage.Unknown);

            // Act
            var result = exception.GetLoadFailureSummary();

            // Assert
            Assert.Equal(
                "Failed to load plugin '' from '' at stage Unknown.",
                result);
        }

        [Fact]
        public void GetLoadFailureSummary_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            PluginLoadException exception = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.GetLoadFailureSummary());
        }

        [Fact]
        public void WithLoadStage_ReturnsNewException_WithSameMessageAndProperties()
        {
            // Arrange
            var originalException = new PluginLoadException(
                "Original message",
                "OriginalPlugin",
                "/original/path.dll",
                PluginLoadStage.MetadataValidation,
                new InvalidOperationException("Inner error"));

            // Act
            var newException = originalException.WithLoadStage(PluginLoadStage.TypeLoading);

            // Assert
            Assert.NotSame(originalException, newException);
            Assert.Equal(originalException.Message, newException.Message);
            Assert.Equal(originalException.PluginName, newException.PluginName);
            Assert.Equal(originalException.AssemblyPath, newException.AssemblyPath);
            Assert.Equal(originalException.InnerException, newException.InnerException);
            Assert.Equal(PluginLoadStage.TypeLoading, newException.LoadStage);
        }

        [Fact]
        public void WithLoadStage_ReturnsNewException_WithDifferentStages()
        {
            // Arrange
            var exception = new PluginLoadException(
                "Test message",
                "TestPlugin",
                "/test/path.dll",
                PluginLoadStage.AssemblyResolution);

            // Act
            var result = exception.WithLoadStage(PluginLoadStage.Initialization);

            // Assert
            Assert.Equal(PluginLoadStage.Initialization, result.LoadStage);
            Assert.Equal(PluginLoadStage.AssemblyResolution, exception.LoadStage); // Original unchanged
        }

        [Fact]
        public void WithLoadStage_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            PluginLoadException exception = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.WithLoadStage(PluginLoadStage.Unknown));
        }

        [Fact]
        public void WithLoadStage_CopiesAllPropertiesCorrectly()
        {
            // Arrange
            var innerException = new FormatException("Inner format error");
            var originalException = new PluginLoadException(
                "Detailed error message",
                "ComplexPlugin",
                "/complex/path/to/plugin.dll",
                PluginLoadStage.DependencyResolution,
                innerException);

            // Act
            var newException = originalException.WithLoadStage(PluginLoadStage.Instantiation);

            // Assert - all properties should be copied except LoadStage
            Assert.Equal(originalException.Message, newException.Message);
            Assert.Equal(originalException.PluginName, newException.PluginName);
            Assert.Equal(originalException.AssemblyPath, newException.AssemblyPath);
            Assert.Equal(originalException.InnerException, newException.InnerException);
            Assert.Equal(PluginLoadStage.Instantiation, newException.LoadStage);
        }

        [Fact]
        public void WithLoadStage_ReturnsNewException_WithNullInnerException()
        {
            // Arrange
            var originalException = new PluginLoadException(
                "Simple error",
                "SimplePlugin",
                "/simple/path.dll",
                PluginLoadStage.InterfaceValidation);

            // Act
            var newException = originalException.WithLoadStage(PluginLoadStage.Initialization);

            // Assert
            Assert.Null(newException.InnerException);
            Assert.Equal(PluginLoadStage.Initialization, newException.LoadStage);
        }
    }
}
