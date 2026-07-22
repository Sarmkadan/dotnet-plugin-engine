using System;
using PluginEngine.Exceptions;
using Xunit;

namespace PluginEngine.Tests
{
    public class PluginIncompatibleExceptionExtensionsTests
    {
        [Fact]
        public void IsVersionIncompatible_ReturnsTrue_WhenHostEngineVersionIsNull()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", "[1.0.0]", null);

            // Act
            var result = exception.IsVersionIncompatible();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsVersionIncompatible_ReturnsTrue_WhenHostEngineVersionIsEmpty()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", "[1.0.0]", string.Empty);

            // Act
            var result = exception.IsVersionIncompatible();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsVersionIncompatible_ReturnsTrue_WhenDeclaredConstraintIsNull()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", null, "1.0.0");

            // Act
            var result = exception.IsVersionIncompatible();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsVersionIncompatible_ReturnsTrue_WhenDeclaredConstraintIsEmpty()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", string.Empty, "1.0.0");

            // Act
            var result = exception.IsVersionIncompatible();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsVersionIncompatible_ReturnsFalse_WhenBothVersionsArePresent()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", "[1.0.0]", "2.0.0");

            // Act
            var result = exception.IsVersionIncompatible();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVersionIncompatible_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            PluginIncompatibleException exception = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.IsVersionIncompatible());
        }

        [Fact]
        public void GetIncompatibilityReason_ReturnsFormattedString_WithAllProperties()
        {
            // Arrange
            var exception = new PluginIncompatibleException("AwesomePlugin", "[1.0.0,2.0.0]", "3.5.2");

            // Act
            var result = exception.GetIncompatibilityReason();

            // Assert
            Assert.Equal("Plugin [1.0.0,2.0.0] is incompatible with host engine version 3.5.2", result);
        }

        [Fact]
        public void GetIncompatibilityReason_ReturnsFormattedString_WithEmptyConstraint()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", string.Empty, "1.0.0");

            // Act
            var result = exception.GetIncompatibilityReason();

            // Assert
            Assert.Equal("Plugin  is incompatible with host engine version 1.0.0", result);
        }

        [Fact]
        public void GetIncompatibilityReason_ReturnsFormattedString_WithEmptyHostVersion()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", "[1.0.0]", string.Empty);

            // Act
            var result = exception.GetIncompatibilityReason();

            // Assert
            Assert.Equal("Plugin [1.0.0] is incompatible with host engine version ", result);
        }

        [Fact]
        public void GetIncompatibilityReason_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            PluginIncompatibleException exception = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.GetIncompatibilityReason());
        }

        [Fact]
        public void HasDeclaredConstraint_ReturnsTrue_WhenConstraintIsPresent()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", "[1.0.0]", "2.0.0");

            // Act
            var result = exception.HasDeclaredConstraint();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasDeclaredConstraint_ReturnsFalse_WhenConstraintIsNull()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", null, "1.0.0");

            // Act
            var result = exception.HasDeclaredConstraint();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasDeclaredConstraint_ReturnsFalse_WhenConstraintIsEmpty()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", string.Empty, "1.0.0");

            // Act
            var result = exception.HasDeclaredConstraint();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasDeclaredConstraint_ReturnsFalse_WhenConstraintIsWhitespace()
        {
            // Arrange
            var exception = new PluginIncompatibleException("TestPlugin", "   ", "1.0.0");

            // Act
            var result = exception.HasDeclaredConstraint();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasDeclaredConstraint_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            PluginIncompatibleException exception = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.HasDeclaredConstraint());
        }
    }
}
