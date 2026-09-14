using PluginEngine.Middleware;
using PluginEngine.Tests;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PluginEngine.Tests
{
    /// <summary>
    /// Tests for the CachingMiddleware class.
    /// </summary>
    public class CachingMiddlewareTests
    {
        [Fact]
        /// <summary>Verifies that the middleware works correctly for the GetMetadata operation.</summary>
        public async Task HappyPath_GetMetadata()
        {
            // Arrange
            var context = new PluginOperationContext
            {
                Plugin = new Plugin
                {
                    Id = Guid.NewGuid(),
                    Name = "TestPlugin"
                },
                OperationType = "GetMetadata",
                Metadata = new Dictionary<string, object>()
            };
            var next = new Mock<PluginOperationDelegate>(context => Task.CompletedTask);
            var cachingMiddleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(5));

            // Act
            await cachingMiddleware.InvokeAsync(context, next);

            // Assert
            Assert.True(context.IsSuccessful);
            Assert.Null(context.Exception);
            Assert.True(context.Metadata.ContainsKey("cached"));
        }

        [Fact]
        /// <summary>Verifies that the middleware works correctly for the ResolveDependencies operation.</summary>
        public async Task HappyPath_ResolveDependencies()
        {
            // Arrange
            var context = new PluginOperationContext
            {
                Plugin = new Plugin
                {
                    Id = Guid.NewGuid(),
                    Name = "TestPlugin"
                },
                OperationType = "ResolveDependencies",
                Metadata = new Dictionary<string, object>()
            };
            var next = new Mock<PluginOperationDelegate>(context => Task.CompletedTask);
            var cachingMiddleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(5));

            // Act
            await cachingMiddleware.InvokeAsync(context, next);

            // Assert
            Assert.True(context.IsSuccessful);
            Assert.Null(context.Exception);
            Assert.True(context.Metadata.ContainsKey("cached"));
        }

        [Fact]
        /// <summary>Verifies that the middleware works correctly for the ValidateVersion operation.</summary>
        public async Task HappyPath_ValidateVersion()
        {
            // Arrange
            var context = new PluginOperationContext
            {
                Plugin = new Plugin
                {
                    Id = Guid.NewGuid(),
                    Name = "TestPlugin"
                },
                OperationType = "ValidateVersion",
                Metadata = new Dictionary<string, object>()
            };
            var next = new Mock<PluginOperationDelegate>(context => Task.CompletedTask);
            var cachingMiddleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(5));

            // Act
            await cachingMiddleware.InvokeAsync(context, next);

            // Assert
            Assert.True(context.IsSuccessful);
            Assert.Null(context.Exception);
            Assert.True(context.Metadata.ContainsKey("cached"));
        }

        [Fact]
        /// <summary>Verifies that the middleware throws an ArgumentNullException when the context is null.</summary>
        public async Task EdgeCase_NullContext()
        {
            // Arrange
            var next = new Mock<PluginOperationDelegate>(context => Task.CompletedTask);
            var cachingMiddleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(5));

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => cachingMiddleware.InvokeAsync(null, next));
        }

        [Fact]
        /// <summary>Verifies that the middleware throws an ArgumentNullException when the next delegate is null.</summary>
        public async Task EdgeCase_NullNext()
        {
            // Arrange
            var context = new PluginOperationContext
            {
                Plugin = new Plugin
                {
                    Id = Guid.NewGuid(),
                    Name = "TestPlugin"
                },
                OperationType = "GetMetadata"
            };
            var cachingMiddleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(5));

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => cachingMiddleware.InvokeAsync(context, null));
        }

        [Fact]
        /// <summary>Verifies that the middleware marks the context as unsuccessful when an invalid operation type is provided.</summary>
        public async Task ErrorPath_InvalidOperationType()
        {
            // Arrange
            var context = new PluginOperationContext
            {
                Plugin = new Plugin
                {
                    Id = Guid.NewGuid(),
                    Name = "TestPlugin"
                },
                OperationType = "InvalidOperation"
            };
            var next = new Mock<PluginOperationDelegate>(context => Task.CompletedTask);
            var cachingMiddleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(5));

            // Act
            await cachingMiddleware.InvokeAsync(context, next);

            // Assert
            Assert.False(context.IsSuccessful);
            Assert.NotNull(context.Exception);
        }
    }
}