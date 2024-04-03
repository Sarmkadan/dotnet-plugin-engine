#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Middleware;

/// <summary>
/// Middleware for logging plugin operations.
/// Logs operation start, duration, and any exceptions that occur.
/// Provides diagnostic information for troubleshooting plugin issues.
/// </summary>
public sealed class LoggingMiddleware : IPluginMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
        var startTime = DateTime.UtcNow;
        context.StartTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        _logger.LogInformation(
            "Plugin operation started: {OperationType} for {PluginName} v{Version}",
            context.OperationType,
            context.Plugin.Name,
            context.Plugin.Version);

        try
        {
            await next(context);

            var duration = DateTime.UtcNow - startTime;
            context.EndTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            context.IsSuccessful = true;

            _logger.LogInformation(
                "Plugin operation completed: {OperationType} for {PluginName} in {DurationMs}ms",
                context.OperationType,
                context.Plugin.Name,
                duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            context.EndTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            context.Exception = ex;
            context.IsSuccessful = false;

            _logger.LogError(
                ex,
                "Plugin operation failed: {OperationType} for {PluginName} after {DurationMs}ms",
                context.OperationType,
                context.Plugin.Name,
                duration.TotalMilliseconds);

            throw;
        }
    }
}

/// <summary>
/// Extension methods for registering logging middleware.
/// </summary>
public static class LoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds logging middleware to the plugin pipeline.
    /// </summary>
    public static PluginMiddlewarePipeline UseLogging(this PluginMiddlewarePipeline pipeline)
    {
        return pipeline.Use(next => async context =>
        {
            var middleware = new LoggingMiddleware(
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<LoggingMiddleware>());

            await middleware.InvokeAsync(context, next);
        });
    }
}
