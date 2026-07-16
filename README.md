# dotnet-plugin-engine

[... existing content ...]  

## IPluginMiddleware

The `IPluginMiddleware` interface defines the contract for plugin operation middleware components. Middleware processes plugin operations before and after execution, enabling cross-cutting concerns like logging, error handling, performance tracking, and validation. Middleware components are composed into a pipeline using `PluginMiddlewarePipeline`, allowing flexible middleware composition.

Here's a realistic usage example implementing a custom logging middleware:

```csharp
using PluginEngine.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

// Custom middleware that logs plugin operation timing and exceptions
public class LoggingMiddleware : IPluginMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    
    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(PluginOperationContext context, PluginOperationDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        context.StartTimeMs = stopwatch.ElapsedMilliseconds;
        
        try
        {
            await next(context);
            context.IsSuccessful = true;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.IsSuccessful = false;
            _logger.LogError(ex, "Plugin operation failed: {OperationType}", context.OperationType);
            throw;
        }
        finally
        {
            context.EndTimeMs = stopwatch.ElapsedMilliseconds;
            
            var durationMs = context.EndTimeMs - context.StartTimeMs;
            _logger.LogInformation(
                "Plugin operation {OperationType} for {PluginName} completed in {DurationMs}ms. Status: {(context.IsSuccessful ? "Success" : "Failed")}",
                context.OperationType,
                context.Plugin.Name,
                durationMs,
                context.IsSuccessful ? "Success" : "Failed"
            );
        }
    }
}

// Usage in a plugin engine pipeline
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPluginEngine();

var serviceProvider = services.BuildServiceProvider();
var pipeline = serviceProvider.GetRequiredService<PluginMiddlewarePipeline>();

// Add your custom logging middleware
pipeline.Use(async (next) => 
    new LoggingMiddleware(serviceProvider.GetRequiredService<ILogger<LoggingMiddleware>>())(context, next)
);

// Or use the middleware directly with the pipeline's Use method
pipeline.Use<LoggingMiddleware>();

// The middleware pipeline can now process plugin operations with logging
```

## IHotReloadService

The `IHotReloadService` interface exposes operations for monitoring, triggering, and querying hot reloads of plugins at runtime. It allows starting and stopping a file‑watcher that automatically reloads changed assemblies, registering callbacks that run after a successful reload, and retrieving statistics and status information about reload activity.

```csharp
using PluginEngine.Services.Abstractions;
using System;
using System.Threading.Tasks;

public class HotReloadDemo
{
    private readonly IHotReloadService _hotReloadService;

    public HotReloadDemo(IHotReloadService hotReloadService)
    {
        _hotReloadService = hotReloadService;
    }

    public async Task RunAsync()
    {
        // Start monitoring for changes
        await _hotReloadService.StartHotReloadMonitoringAsync();

        // Register a callback for a specific plugin
        var pluginId = Guid.Parse("00000000-0000-0000-0000-000000000000"); // replace with a real plugin id
        _hotReloadService.RegisterHotReloadCallback(pluginId, async plugin =>
        {
            Console.WriteLine($"Plugin {plugin.Id} reloaded at {DateTime.UtcNow}");
            await Task.CompletedTask;
        });

        // Perform a hot reload manually
        bool success = await _hotReloadService.HotReloadPluginAsync(pluginId);
        Console.WriteLine($"Manual reload {(success ? "succeeded" : "failed")}");

        // Get statistics
        var stats = await _hotReloadService.GetStatisticsAsync();
        Console.WriteLine($"Total reloads: {stats.TotalReloads}");
        Console.WriteLine($"Successful reloads: {stats.SuccessfulReloads}");
        Console.WriteLine($"Failed reloads: {stats.FailedReloads}");
        Console.WriteLine($"Last reload time: {stats.LastReloadTime}");
        Console.WriteLine($"Average reload time: {stats.AverageReloadTime}");

        // Get status of a plugin
        var status = await _hotReloadService.GetHotReloadStatusAsync(pluginId);
        if (status != null)
        {
            Console.WriteLine($"Plugin supports hot reload: {status.SupportsHotReload}");
            Console.WriteLine($"Reload count: {status.ReloadCount}");
        }

        // Stop monitoring
        await _hotReloadService.StopHotReloadMonitoringAsync();
    }
}
```

