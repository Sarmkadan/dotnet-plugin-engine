#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Middleware;

/// <summary>
/// Middleware for handling and normalizing plugin operation errors.
/// Catches exceptions, logs them with context, and optionally provides recovery options.
/// Prevents unhandled exceptions from propagating up the call stack.
/// </summary>
public sealed class ErrorHandlingMiddleware : IPluginMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly bool _continueOnError;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, bool continueOnError = false)
    {
        _logger = logger;
        _continueOnError = continueOnError;
    }

    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (PluginException ex)
        {
            HandlePluginException(ex, context);

            if (!_continueOnError)
                throw;
        }
        catch (Exception ex)
        {
            HandleGeneralException(ex, context);

            if (!_continueOnError)
                throw;
        }
    }

    private void HandlePluginException(PluginException ex, PluginOperationContext context)
    {
        context.Exception = ex;
        context.IsSuccessful = false;

        var details = ex switch
        {
            PluginLoadException ple => $"Load stage: {ple.LoadStage}, Code: {ple.ErrorCode}",
            DependencyResolutionException dre => $"Unresolved: {string.Join(", ", dre.UnresolvedDependencies)}",
            VersionMismatchException vme => $"Expected: {vme.ExpectedVersion}, Got: {vme.ActualVersion}",
            _ => ex.GetType().Name
        };

        _logger.LogError(
            "Plugin error in {Operation}: {PluginName} - {Details}\n{Message}",
            context.OperationType,
            context.Plugin.Name,
            details,
            ex.Message);
    }

    private void HandleGeneralException(Exception ex, PluginOperationContext context)
    {
        context.Exception = ex;
        context.IsSuccessful = false;

        _logger.LogError(
            ex,
            "Unhandled error in {Operation} for plugin {PluginName}",
            context.OperationType,
            context.Plugin.Name);
    }
}

/// <summary>
/// Extension methods for registering error handling middleware.
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds error handling middleware to the plugin pipeline.
    /// </summary>
    public static PluginMiddlewarePipeline UseErrorHandling(
        this PluginMiddlewarePipeline pipeline,
        bool continueOnError = false)
    {
        return pipeline.Use(next => async context =>
        {
            var middleware = new ErrorHandlingMiddleware(
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<ErrorHandlingMiddleware>(),
                continueOnError);

            await middleware.InvokeAsync(context, next);
        });
    }
}
