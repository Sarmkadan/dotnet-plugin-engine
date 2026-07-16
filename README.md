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



## IDependencyResolutionService

The `IDependencyResolutionService` interface provides functionality for resolving, validating, and analyzing plugin dependencies. It enables discovering all dependencies for a plugin, checking for circular dependencies, building dependency graphs for visualization, and managing the dependency resolution cache. This service is essential for plugin engines that need to ensure proper plugin loading order and detect dependency conflicts.

Here's a realistic usage example that builds a dependency graph and validates dependencies:

```csharp
using PluginEngine.Services.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DependencyResolutionDemo
{
    private readonly IDependencyResolutionService _dependencyService;

    public DependencyResolutionDemo(IDependencyResolutionService dependencyService)
    {
        _dependencyService = dependencyService;
    }

    public async Task RunAsync()
    {
        // Get the dependency graph for a plugin
        var graph = await _dependencyService.GetDependencyGraphAsync(
            Guid.Parse("00000000-0000-0000-0000-000000000000"));
        
        Console.WriteLine($"Root Plugin: {graph.RootPluginId}");
        Console.WriteLine($"Total Nodes: {graph.Nodes.Count}");
        Console.WriteLine($"Total Edges: {graph.Edges.Count}");
        
        // Display dependency tree
        foreach (var node in graph.Nodes.OrderBy(n => n.Level))
        {
            Console.WriteLine($"  Level {node.Level}: {node.PluginName} ({node.PluginId}) v{node.Version}");
        }
        
        // Check for circular dependencies
        bool hasCircular = await _dependencyService.HasCircularDependenciesAsync(
            new Plugin { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") });
        Console.WriteLine($"Circular dependencies detected: {hasCircular}");
        
        // Validate dependencies
        bool isValid = await _dependencyService.ValidateDependenciesAsync(
            new Plugin { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") });
        Console.WriteLine($"Dependencies valid: {isValid}");
        
        // Get all plugins that depend on a specific plugin
        var dependents = await _dependencyService.GetDependentsAsync(
            Guid.Parse("00000000-0000-0000-0000-000000000000"));
        Console.WriteLine($"Plugins depending on this: {dependents.Count()}");
        
        // Resolve all dependencies for a plugin
        var dependencies = await _dependencyService.ResolveDependenciesAsync(
            new Plugin { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") });
        Console.WriteLine($"Total dependencies resolved: {dependencies.Count()}");
        
        // Clear dependency cache when needed
        await _dependencyService.ClearDependencyCacheAsync();
    }
}
```

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

## IVersioningService

The `IVersioningService` interface and its associated `SemanticVersion` class provide functionality for managing semantic versioning in the plugin system. The `SemanticVersion` class represents a semantic version with `Major`, `Minor`, `Patch`, `Prerelease`, and `Metadata` components, enabling version parsing, comparison, and formatting operations.


Here's a realistic usage example that demonstrates version parsing and formatting:

```csharp
using PluginEngine.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

public class VersioningDemo
{
    private readonly IVersioningService _versioningService;

    public VersioningDemo(IVersioningService versioningService)
    {
        _versioningService = versioningService;
    }

    public void Run()
    {
        // Parse a version string into SemanticVersion
        var versionString = "2.5.1-beta.1+20240716";
        var semanticVersion = _versioningService.ParseVersion(versionString);

        if (semanticVersion != null)
        {
            Console.WriteLine($"Parsed version: {semanticVersion}");
            Console.WriteLine($"Major: {semanticVersion.Major}");
            Console.WriteLine($"Minor: {semanticVersion.Minor}");
            Console.WriteLine($"Patch: {semanticVersion.Patch}");
            Console.WriteLine($"Prerelease: {semanticVersion.Prerelease}");
            Console.WriteLine($"Metadata: {semanticVersion.Metadata}");
        }

        // Create a SemanticVersion directly
        var customVersion = new SemanticVersion
        {
            Major = 3,
            Minor = 0,
            Patch = 0,
            Prerelease = "rc.1",
            Metadata = "build.12345"
        };

        Console.WriteLine($"Custom version: {customVersion}");

        // Validate a version string
        bool isValid = _versioningService.ValidateVersion("1.2.3");
        Console.WriteLine($"Is '1.2.3' valid: {isValid}");

        // Compare two versions
        int comparison = _versioningService.CompareVersions("2.0.0", "1.9.9");
        Console.WriteLine($"2.0.0 compared to 1.9.9: {comparison} (2.0.0 > 1.9.9)");

        // Check version compatibility
        bool isCompatible = _versioningService.AreCompatible("1.5.0", "1.5.2");
        Console.WriteLine($"1.5.0 and 1.5.2 compatible: {isCompatible}");

        // Increment a version
        string incrementedVersion = _versioningService.IncrementVersion("1.2.3", VersionPart.Major);
        Console.WriteLine($"Incremented major version: {incrementedVersion}");
    }
}

// Usage in DI setup
var services = new ServiceCollection();
services.AddPluginEngine();

var serviceProvider = services.BuildServiceProvider();
var versioningService = serviceProvider.GetRequiredService<IVersioningService>();

var demo = new VersioningDemo(versioningService);
demo.Run();
```

