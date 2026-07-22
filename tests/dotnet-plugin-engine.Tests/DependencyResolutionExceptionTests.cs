using System;
using System.Collections.Generic;
using PluginEngine.Exceptions;
using Xunit;

namespace PluginEngine.Tests
{
    public class DependencyResolutionExceptionTests
    {
        #region Constructors

        [Fact]
        public void Constructor_Parameterless_CreatesExceptionWithDefaultValues()
        {
            // Act
            var exception = new DependencyResolutionException();

            // Assert
            Assert.Equal("DEPENDENCY_RESOLUTION_ERROR", exception.ErrorCode);
            Assert.Null(exception.DependencyPluginId);
            Assert.Empty(exception.VersionConstraint);
            Assert.Equal(DependencyResolutionReason.Unknown, exception.Reason);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        [Fact]
        public void Constructor_WithMessage_SetsMessageAndDefaultValues()
        {
            // Arrange
            var message = "Dependency resolution failed";

            // Act
            var exception = new DependencyResolutionException(message);

            // Assert
            Assert.Equal("DEPENDENCY_RESOLUTION_ERROR", exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.DependencyPluginId);
            Assert.Empty(exception.VersionConstraint);
            Assert.Equal(DependencyResolutionReason.Unknown, exception.Reason);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        [Fact]
        public void Constructor_WithMessageAndReason_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Version mismatch detected";
            var reason = DependencyResolutionReason.VersionMismatch;

            // Act
            var exception = new DependencyResolutionException(message, reason);

            // Assert
            Assert.Equal("DEPENDENCY_RESOLUTION_ERROR", exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Equal(reason, exception.Reason);
            Assert.Null(exception.DependencyPluginId);
            Assert.Empty(exception.VersionConstraint);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        [Fact]
        public void Constructor_WithFullDetails_SetsAllProperties()
        {
            // Arrange
            var message = "Circular dependency detected";
            var pluginId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            var versionConstraint = "[1.0.0, 2.0.0)";
            var reason = DependencyResolutionReason.CircularDependency;

            // Act
            var exception = new DependencyResolutionException(message, pluginId, versionConstraint, reason);

            // Assert
            Assert.Equal("DEPENDENCY_RESOLUTION_ERROR", exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Equal(pluginId, exception.DependencyPluginId);
            Assert.Equal(versionConstraint, exception.VersionConstraint);
            Assert.Equal(reason, exception.Reason);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        [Fact]
        public void Constructor_WithFullDetails_NullPluginId_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Dependency not found";
            Guid? pluginId = null;
            var versionConstraint = "1.0.0";
            var reason = DependencyResolutionReason.DependencyNotFound;

            // Act
            var exception = new DependencyResolutionException(message, pluginId, versionConstraint, reason);

            // Assert
            Assert.Equal("DEPENDENCY_RESOLUTION_ERROR", exception.ErrorCode);
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.DependencyPluginId);
            Assert.Equal(versionConstraint, exception.VersionConstraint);
            Assert.Equal(reason, exception.Reason);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        #endregion

        #region AddUnresolvedDependency

        [Fact]
        public void AddUnresolvedDependency_AddsValidDependencyName()
        {
            // Arrange
            var exception = new DependencyResolutionException("Test message");

            // Act
            var result = exception.AddUnresolvedDependency("Plugin.Core");

            // Assert
            Assert.Same(exception, result); // Should return same instance
            Assert.Single(exception.UnresolvedDependencies);
            Assert.Equal("Plugin.Core", exception.UnresolvedDependencies[0]);
        }

        [Fact]
        public void AddUnresolvedDependency_AddsMultipleDependencies()
        {
            // Arrange
            var exception = new DependencyResolutionException("Test message");

            // Act
            exception.AddUnresolvedDependency("Plugin.Core");
            exception.AddUnresolvedDependency("Plugin.Logging");
            exception.AddUnresolvedDependency("Plugin.Data");

            // Assert
            Assert.Equal(3, exception.UnresolvedDependencies.Count);
            Assert.Equal("Plugin.Core", exception.UnresolvedDependencies[0]);
            Assert.Equal("Plugin.Logging", exception.UnresolvedDependencies[1]);
            Assert.Equal("Plugin.Data", exception.UnresolvedDependencies[2]);
        }

        [Fact]
        public void AddUnresolvedDependency_WithWhitespaceOnly_ReturnsSameInstanceWithoutAdding()
        {
            // Arrange
            var exception = new DependencyResolutionException("Test message");
            var countBefore = exception.UnresolvedDependencies.Count;

            // Act
            var result = exception.AddUnresolvedDependency("   ");

            // Assert
            Assert.Same(exception, result);
            Assert.Equal(countBefore, exception.UnresolvedDependencies.Count);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        [Fact]
        public void AddUnresolvedDependency_WithEmptyString_ReturnsSameInstanceWithoutAdding()
        {
            // Arrange
            var exception = new DependencyResolutionException("Test message");
            var countBefore = exception.UnresolvedDependencies.Count;

            // Act
            var result = exception.AddUnresolvedDependency(string.Empty);

            // Assert
            Assert.Same(exception, result);
            Assert.Equal(countBefore, exception.UnresolvedDependencies.Count);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        [Fact]
        public void AddUnresolvedDependency_WithNull_ReturnsSameInstanceWithoutAdding()
        {
            // Arrange
            var exception = new DependencyResolutionException("Test message");
            var countBefore = exception.UnresolvedDependencies.Count;

            // Act
            var result = exception.AddUnresolvedDependency(null);

            // Assert
            Assert.Same(exception, result);
            Assert.Equal(countBefore, exception.UnresolvedDependencies.Count);
            Assert.Empty(exception.UnresolvedDependencies);
        }

        #endregion

        #region ToString

        [Fact]
        public void ToString_WithNoUnresolvedDependencies_ReturnsBaseToString()
        {
            // Arrange
            var exception = new DependencyResolutionException("Simple error");

            // Act
            var result = exception.ToString();

            // Assert
            Assert.Contains("Simple error", result);
            Assert.DoesNotContain("Unresolved Dependencies", result);
        }

        [Fact]
        public void ToString_WithSingleUnresolvedDependency_ReturnsFormattedOutput()
        {
            // Arrange
            var exception = new DependencyResolutionException("Error with dependencies");
            exception.AddUnresolvedDependency("Plugin.Core");

            // Act
            var result = exception.ToString();

            // Assert
            Assert.Contains("Error with dependencies", result);
            Assert.Contains("Unresolved Dependencies (1):", result);
            Assert.Contains("- Plugin.Core", result);
        }

        [Fact]
        public void ToString_WithMultipleUnresolvedDependencies_ReturnsAllDependencies()
        {
            // Arrange
            var exception = new DependencyResolutionException("Multiple dependencies missing");
            exception.AddUnresolvedDependency("Plugin.Core");
            exception.AddUnresolvedDependency("Plugin.Logging");
            exception.AddUnresolvedDependency("Plugin.Data");

            // Act
            var result = exception.ToString();

            // Assert
            Assert.Contains("Multiple dependencies missing", result);
            Assert.Contains("Unresolved Dependencies (3):", result);
            Assert.Contains("- Plugin.Core", result);
            Assert.Contains("- Plugin.Logging", result);
            Assert.Contains("- Plugin.Data", result);
        }

        [Fact]
        public void ToString_WithFullDetails_ContainsAllProperties()
        {
            // Arrange
            var pluginId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            var exception = new DependencyResolutionException(
                "Circular dependency detected",
                pluginId,
                "[1.0.0, 2.0.0)",
                DependencyResolutionReason.CircularDependency
            );
            exception.AddUnresolvedDependency("Plugin.A");
            exception.AddUnresolvedDependency("Plugin.B");

            // Act
            var result = exception.ToString();

            // Assert
            Assert.Contains("Circular dependency detected", result);
            Assert.Contains("DEPENDENCY_RESOLUTION_ERROR", result);
            Assert.Contains("Unresolved Dependencies (2):", result);
            Assert.Contains("- Plugin.A", result);
            Assert.Contains("- Plugin.B", result);
        }

        #endregion

        #region PropertyAccessors

        [Fact]
        public void DependencyPluginId_CanBeSetAndGet()
        {
            // Arrange
            var exception = new DependencyResolutionException();
            var pluginId = Guid.Parse("87654321-4321-4321-4321-cba987654321");

            // Act
            exception.DependencyPluginId = pluginId;

            // Assert
            Assert.Equal(pluginId, exception.DependencyPluginId);
        }

        [Fact]
        public void VersionConstraint_CanBeSetAndGet()
        {
            // Arrange
            var exception = new DependencyResolutionException();
            var version = "1.2.3";

            // Act
            exception.VersionConstraint = version;

            // Assert
            Assert.Equal(version, exception.VersionConstraint);
        }

        [Fact]
        public void Reason_CanBeSetAndGet()
        {
            // Arrange
            var exception = new DependencyResolutionException();
            var reason = DependencyResolutionReason.OptionalDependencyFailed;

            // Act
            exception.Reason = reason;

            // Assert
            Assert.Equal(reason, exception.Reason);
        }

        [Fact]
        public void UnresolvedDependencies_ListIsInitializedAndCanBeModified()
        {
            // Arrange
            var exception = new DependencyResolutionException();

            // Act
            exception.UnresolvedDependencies.Add("TestPlugin");

            // Assert
            Assert.Single(exception.UnresolvedDependencies);
            Assert.Equal("TestPlugin", exception.UnresolvedDependencies[0]);
        }

        #endregion

        #region EdgeCases

        [Fact]
        public void Constructor_WithEmptyMessage_SetsEmptyMessage()
        {
            // Act
            var exception = new DependencyResolutionException(string.Empty);

            // Assert
            Assert.Empty(exception.Message);
            Assert.Equal("DEPENDENCY_RESOLUTION_ERROR", exception.ErrorCode);
        }

        [Fact]
        public void Constructor_WithNullMessage_SetsNullMessage()
        {
            // Act
            var exception = new DependencyResolutionException(null);

            // Assert
            Assert.Null(exception.Message);
            Assert.Equal("DEPENDENCY_RESOLUTION_ERROR", exception.ErrorCode);
        }

        [Fact]
        public void Constructor_WithWhitespaceMessage_SetsWhitespaceMessage()
        {
            // Act
            var exception = new DependencyResolutionException("   ");

            // Assert
            Assert.Equal("   ", exception.Message);
        }

        [Fact]
        public void Constructor_WithAllEnumValues_SetsCorrectReason()
        {
            // Test each enum value
            var exception1 = new DependencyResolutionException("Test", DependencyResolutionReason.Unknown);
            Assert.Equal(DependencyResolutionReason.Unknown, exception1.Reason);

            var exception2 = new DependencyResolutionException("Test", DependencyResolutionReason.DependencyNotFound);
            Assert.Equal(DependencyResolutionReason.DependencyNotFound, exception2.Reason);

            var exception3 = new DependencyResolutionException("Test", DependencyResolutionReason.VersionMismatch);
            Assert.Equal(DependencyResolutionReason.VersionMismatch, exception3.Reason);

            var exception4 = new DependencyResolutionException("Test", DependencyResolutionReason.CircularDependency);
            Assert.Equal(DependencyResolutionReason.CircularDependency, exception4.Reason);

            var exception5 = new DependencyResolutionException("Test", DependencyResolutionReason.DependencyNotLoaded);
            Assert.Equal(DependencyResolutionReason.DependencyNotLoaded, exception5.Reason);

            var exception6 = new DependencyResolutionException("Test", DependencyResolutionReason.OptionalDependencyFailed);
            Assert.Equal(DependencyResolutionReason.OptionalDependencyFailed, exception6.Reason);
        }

        #endregion
    }
}