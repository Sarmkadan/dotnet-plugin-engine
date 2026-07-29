using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using PluginEngine.Domain.Entities;

namespace PluginEngine.Tests
{
    public class AssemblyLoadContextInfoExtensionsTests
    {
        [Fact]
        public void HasMemoryExceeded_HappyPath_ReturnsTrue()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { MemoryUsageBytes = 1024 * 1024 * 1024 + 1 };
            // Act
            var result = context.HasMemoryExceeded(1024 * 1024 * 1024);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetAgeInMinutes_HappyPath_ReturnsAge()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
            // Act
            var result = context.GetAgeInMinutes();
            // Assert
            Assert.True(result >= 10);
        }

        [Fact]
        public void GetInactivityMinutes_HappyPath_ReturnsInactivity()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { LastActivityAt = DateTime.UtcNow.AddMinutes(-10) };
            // Act
            var result = context.GetInactivityMinutes();
            // Assert
            Assert.True(result >= 10);
        }

        [Fact]
        public void IsStale_HappyPath_ReturnsTrue()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { LastActivityAt = DateTime.UtcNow.AddMinutes(-61) };
            // Act
            var result = context.IsStale(60);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetDetailedStatusReport_HappyPath_ReturnsReport()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { ContextId = Guid.NewGuid(), Name = "Test", PluginId = Guid.NewGuid() };
            // Act
            var result = context.GetDetailedStatusReport();
            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void IsHealthy_HappyPath_ReturnsTrue()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { IsActive = true, IsValid = true, MemoryUsageBytes = 1024 * 1024 * 1024 - 1, LastActivityAt = DateTime.UtcNow };
            // Act
            var result = context.IsHealthy();
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void FindAssembliesByPattern_HappyPath_ReturnsAssemblies()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { LoadedAssemblies = new List<string> { "Assembly1", "Assembly2" } };
            // Act
            var result = context.FindAssembliesByPattern("Assembly");
            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetMemoryUsageString_HappyPath_ReturnsString()
        {
            // Arrange
            var context = new AssemblyLoadContextInfo { MemoryUsageBytes = 1024 * 1024 * 1024 };
            // Act
            var result = context.GetMemoryUsageString();
            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HasMemoryExceeded_NullContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((AssemblyLoadContextInfo)null).HasMemoryExceeded(1024));
        }

        [Fact]
        public void GetAgeInMinutes_NullContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((AssemblyLoadContextInfo)null).GetAgeInMinutes());
        }
    }
}
