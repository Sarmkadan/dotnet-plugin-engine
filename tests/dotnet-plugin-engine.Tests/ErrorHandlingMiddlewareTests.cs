using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PluginEngine.Middleware;
using PluginEngine.Exceptions;
using PluginEngine.Domain.Entities;
using Xunit;

namespace PluginEngine.Tests
{
    /// <summary>
    /// Simple logger that records the formatted log messages so that tests can assert that logging occurred.
    /// </summary>
    internal sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> LoggedMessages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            // Record the formatted message; formatter may be null in pathological cases.
            if (formatter != null)
            {
                LoggedMessages.Add(formatter(state, exception));
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    public class ErrorHandlingMiddlewareTests
    {
        private static PluginOperationContext CreateContext()
        {
            // The real PluginOperationContext expects a Plugin with a Name property.
            // PluginInfo exists in the domain layer and satisfies that contract.
            return new PluginOperationContext
            {
                Plugin = new PluginInfo { Name = "TestPlugin" },
                OperationType = "Load"
            };
        }

        [Fact]
        public async Task InvokeAsync_NoException_NextDelegateIsCalled()
        {
            // Arrange
            var logger = new RecordingLogger<ErrorHandlingMiddleware>();
            var middleware = new ErrorHandlingMiddleware(logger);
            var context = CreateContext();

            var nextCalled = false;
            PluginOperationDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            // Act
            await middleware.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled, "The next delegate should have been executed.");
            Assert.Null(context.Exception);
            // IsSuccessful is only set on error paths; it should remain its default value (true or false depending on implementation).
        }

        [Fact]
        public async Task InvokeAsync_PluginException_ContinuesWhenContinueOnErrorTrue()
        {
            // Arrange
            var logger = new RecordingLogger<ErrorHandlingMiddleware>();
            var middleware = new ErrorHandlingMiddleware(logger, continueOnError: true);
            var context = CreateContext();

            var pluginEx = new PluginException("simulated plugin error");
            PluginOperationDelegate next = ctx => throw pluginEx;

            // Act
            await middleware.InvokeAsync(context, next);

            // Assert
            Assert.Same(pluginEx, context.Exception);
            Assert.False(context.IsSuccessful);
            Assert.NotEmpty(logger.LoggedMessages);
        }

        [Fact]
        public async Task InvokeAsync_PluginException_RethrowsWhenContinueOnErrorFalse()
        {
            // Arrange
            var logger = new RecordingLogger<ErrorHandlingMiddleware>();
            var middleware = new ErrorHandlingMiddleware(logger, continueOnError: false);
            var context = CreateContext();

            var pluginEx = new PluginException("simulated plugin error");
            PluginOperationDelegate next = ctx => throw pluginEx;

            // Act & Assert
            var thrown = await Assert.ThrowsAsync<PluginException>(() => middleware.InvokeAsync(context, next));
            Assert.Same(pluginEx, thrown);
            Assert.Same(pluginEx, context.Exception);
            Assert.False(context.IsSuccessful);
            Assert.NotEmpty(logger.LoggedMessages);
        }

        [Fact]
        public async Task InvokeAsync_GeneralException_ContinuesWhenContinueOnErrorTrue()
        {
            // Arrange
            var logger = new RecordingLogger<ErrorHandlingMiddleware>();
            var middleware = new ErrorHandlingMiddleware(logger, continueOnError: true);
            var context = CreateContext();

            var generalEx = new InvalidOperationException("unexpected");
            PluginOperationDelegate next = ctx => throw generalEx;

            // Act
            await middleware.InvokeAsync(context, next);

            // Assert
            Assert.Same(generalEx, context.Exception);
            Assert.False(context.IsSuccessful);
            Assert.NotEmpty(logger.LoggedMessages);
        }

        [Fact]
        public void UseErrorHandling_ReturnsPipelineInstance()
        {
            // Arrange
            var pipeline = new PluginMiddlewarePipeline();

            // Act
            var result = pipeline.UseErrorHandling();

            // Assert
            Assert.NotNull(result);
            // The returned pipeline should be the same instance (fluent API) – this is the contract of Use().
            Assert.Same(pipeline, result);
        }
    }
}
