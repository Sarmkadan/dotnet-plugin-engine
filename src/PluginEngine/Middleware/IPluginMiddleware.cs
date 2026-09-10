#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Middleware;

/// <summary>
/// Defines the contract for plugin operation middleware.
/// Middleware processes plugin operations before and after execution.
/// Supports pipeline composition for logging, caching, error handling, and more.
/// </summary>
public interface IPluginMiddleware
{
    /// <summary>
    /// Executes the middleware and calls the next middleware in the pipeline.
    /// </summary>
    Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next);
}

/// <summary>
/// Represents the context of a plugin operation being processed by middleware.
/// </summary>
public sealed class PluginOperationContext
{
    /// <summary>
    /// Gets or sets the type of the plugin operation.
    /// </summary>
    public required string OperationType { get; set; }

    /// <summary>
    /// Gets or sets the plugin associated with the operation.
    /// </summary>
    public required Plugin Plugin { get; set; }

    /// <summary>
    /// Gets or sets the metadata dictionary for the operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Gets or sets the start time of the operation in milliseconds.
    /// </summary>
    public long StartTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the end time of the operation in milliseconds.
    /// </summary>
    public long? EndTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }
}

/// <summary>
/// Delegate representing the next middleware in the pipeline.
/// </summary>
public delegate Task PluginOperationDelegate(PluginOperationContext context);

/// <summary>
/// Pipeline builder for composing multiple middleware components.
/// </summary>
public sealed class PluginMiddlewarePipeline
{
    private readonly List<Func<PluginOperationDelegate, PluginOperationDelegate>> _pipeline = [];

    /// <summary>
    /// Adds middleware to the pipeline.
    /// </summary>
    /// <param name="middleware">The middleware function to add to the pipeline.</param>
    /// <returns>The pipeline instance for chaining.</returns>
    public PluginMiddlewarePipeline Use(Func<PluginOperationDelegate, PluginOperationDelegate> middleware)
    {
        _pipeline.Add(middleware);
        return this;
    }

    /// <summary>
    /// Builds the complete middleware pipeline.
    /// </summary>
    /// <returns>A delegate representing the composed middleware pipeline.</returns>
    public PluginOperationDelegate Build()
    {
        PluginOperationDelegate pipeline = _ => Task.CompletedTask;

        // Build in reverse order so first middleware added is first to execute
        for (int i = _pipeline.Count - 1; i >= 0; i--)
        {
            var current = _pipeline[i];
            var next = pipeline;
            pipeline = context => current(next)(context);
        }

        return pipeline;
    }
}
