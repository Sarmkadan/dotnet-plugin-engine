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
public class PluginOperationContext
{
    public required string OperationType { get; set; }
    public required Plugin Plugin { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = [];
    public long StartTimeMs { get; set; }
    public long? EndTimeMs { get; set; }
    public Exception? Exception { get; set; }
    public bool IsSuccessful { get; set; }
}

/// <summary>
/// Delegate representing the next middleware in the pipeline.
/// </summary>
public delegate Task PluginOperationDelegate(PluginOperationContext context);

/// <summary>
/// Pipeline builder for composing multiple middleware components.
/// </summary>
public class PluginMiddlewarePipeline
{
    private readonly List<Func<PluginOperationDelegate, PluginOperationDelegate>> _pipeline = [];

    /// <summary>
    /// Adds middleware to the pipeline.
    /// </summary>
    public PluginMiddlewarePipeline Use(Func<PluginOperationDelegate, PluginOperationDelegate> middleware)
    {
        _pipeline.Add(middleware);
        return this;
    }

    /// <summary>
    /// Builds the complete middleware pipeline.
    /// </summary>
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
