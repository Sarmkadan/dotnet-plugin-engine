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
    public class CachingMiddlewareTests
    {
        [Fact]
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
        public async Task EdgeCase_NullContext()
        {
            // Arrange
            var next = new Mock<PluginOperationDelegate>(context => Task.CompletedTask);
            var cachingMiddleware = new CachingMiddleware(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(5));

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => cachingMiddleware.InvokeAsync(null, next));
        }

        [Fact]
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