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

## IHotSwapService

The `IHotSwapService` interface provides zero-downtime plugin hot-swapping capabilities, enabling replacement of a running plugin's assembly with a new version while maintaining host-application availability. It supports automatic rollback to the previous assembly if the new version fails to load, and maintains a complete history of all swap operations for audit and recovery purposes.

Here's a realistic usage example that performs a hot swap with automatic rollback on failure:

```csharp
using PluginEngine.Services.Abstractions;
using System;
using System.Threading.Tasks;

public class HotSwapDemo
{
    private readonly IHotSwapService _hotSwapService;

    public HotSwapDemo(IHotSwapService hotSwapService)
    {
        _hotSwapService = hotSwapService;
    }

    public async Task RunAsync()
    {
        var pluginId = Guid.Parse("00000000-0000-0000-0000-000000000000"); // replace with real plugin id
        var newAssemblyPath = @"./plugins/MyPlugin/v2.0.0/MyPlugin.dll";

        // Register a callback to run after successful swap
        _hotSwapService.RegisterPostSwapCallback(pluginId, async plugin =>
        {
            Console.WriteLine($"Plugin {plugin.Id} successfully swapped at {DateTime.UtcNow}");
            await Task.CompletedTask;
        });

        // Check if the plugin can be swapped
        var plugin = new Plugin { Id = pluginId, State = PluginState.Active };
        if (_hotSwapService.CanSwap(plugin))
        {
            Console.WriteLine("Plugin is ready for hot-swapping");

            // Perform the hot swap
            var result = await _hotSwapService.SwapPluginAsync(pluginId, newAssemblyPath);
            
            if (result.Success)
            {
                Console.WriteLine($"Swap completed successfully in {result.Duration.TotalMilliseconds}ms");
                Console.WriteLine($"Swapped at: {result.SwappedAtUtc}");
                
                // Get the swap record
                var swapRecordResult = await _hotSwapService.GetLastSwapRecordAsync(pluginId);
                if (swapRecordResult.Success && swapRecordResult.Data != null)
                {
                    var record = swapRecordResult.Data;
                    Console.WriteLine($"Previous assembly: {record.PreviousAssemblyPath}");
                    Console.WriteLine($"New assembly: {record.NewAssemblyPath}");
                    Console.WriteLine($"Rolled back: {record.RolledBack}");
                }
            }
            else
            {
                Console.WriteLine($"Swap failed: {result.ErrorMessage}");
                
                // Attempt rollback
                var rollbackResult = await _hotSwapService.RollbackSwapAsync(pluginId);
                if (rollbackResult.Success)
                {
                    Console.WriteLine("Successfully rolled back to previous assembly");
                }
                else
                {
                    Console.WriteLine($"Rollback failed: {rollbackResult.ErrorMessage}");
                }
            }
        }
        else
        {
            Console.WriteLine("Plugin cannot be swapped - check its state and assembly path");
        }

        // Get swap history
        var historyResult = await _hotSwapService.GetSwapHistoryAsync(pluginId);
        if (historyResult.Success)
        {
            Console.WriteLine($"Total swaps: {historyResult.Data?.Count ?? 0}");
            foreach (var record in historyResult.Data ?? new List<SwapRecord>())
            {
                Console.WriteLine($"  {record.SwappedAtUtc:yyyy-MM-dd HH:mm:ss} - {(record.Success ? "Success" : "Failed")} - Duration: {record.Duration.TotalMilliseconds}ms");
            }
        }

        // Unregister the callback when done
        _hotSwapService.UnregisterPostSwapCallback(pluginId);
    }
}
```

## IHotReloadService

The `IHotReloadService` interface exposes operations for monitoring, triggering, and querying hot reloads of plugins at runtime. It allows starting and stopping a file‑watcher that automatically reloads changed assemblies, registering callbacks that run after a successful reload, and retrieving statistics and status information about reload activity.



## IPluginManagerService

The `IPluginManagerService` interface serves as the central service for managing the entire plugin lifecycle within the plugin engine. It provides comprehensive functionality for discovering, loading, initializing, activating, deactivating, and monitoring plugins. The service maintains the overall system state through its status tracking capabilities and offers detailed insights into plugin operations through statistics and detailed plugin information retrieval.

Here's a realistic usage example that demonstrates the complete plugin management workflow:

```csharp
using PluginEngine.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

public class PluginManagerDemo
{
    private readonly IPluginManagerService _pluginManager;
    private readonly ILogger<PluginManagerDemo> _logger;

    public PluginManagerDemo(IPluginManagerService pluginManager, ILogger<PluginManagerDemo> logger)
    {
        _pluginManager = pluginManager;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        try
        {
            // Initialize the plugin manager - discovers and loads all plugins from configured directories
            _logger.LogInformation("Initializing plugin manager...");
            await _pluginManager.InitializeAsync();
            _logger.LogInformation("Plugin manager initialized successfully");

            // Get current status
            var status = await _pluginManager.GetStatusAsync();
            _logger.LogInformation("Plugin Manager Status:");
            _logger.LogInformation($"  IsInitialized: {status.IsInitialized}");
            _logger.LogInformation($"  InitializedAt: {status.InitializedAt}");
            _logger.LogInformation($"  TotalPlugins: {status.TotalPlugins}");
            _logger.LogInformation($"  LoadedPlugins: {status.LoadedPlugins}");
            _logger.LogInformation($"  ActivePlugins: {status.ActivePlugins}");
            _logger.LogInformation($"  FailedPlugins: {status.FailedPlugins}");
            _logger.LogInformation($"  LastError: {status.LastError ?? "None"}");

            // Get all plugins
            _logger.LogInformation($"\nDiscovered {status.TotalPlugins} plugins:");
            var allPlugins = await _pluginManager.GetAllPluginsAsync();
            foreach (var plugin in allPlugins.Take(5)) // Show first 5
            {
                _logger.LogInformation($"  - {plugin.Name} v{plugin.Version} ({plugin.State})");
            }
            if (allPlugins.Count() > 5)
            {
                _logger.LogInformation($"  ... and {allPlugins.Count() - 5} more");
            }

            // Search for specific plugins
            var searchCriteria = new PluginSearchCriteria
            {
                Name = "Logging",
                Status = PluginStatus.Active,
                PageSize = 10
            };
            
            var matchingPlugins = await _pluginManager.SearchPluginsAsync(searchCriteria);
            _logger.LogInformation($"\nFound {matchingPlugins.Count()} active plugins matching 'Logging':");
            foreach (var plugin in matchingPlugins)
            {
                _logger.LogInformation($"  - {plugin.Name} v{plugin.Version}");
            }

            // Get detailed information about a specific plugin
            var firstPlugin = allPlugins.FirstOrDefault();
            if (firstPlugin != null)
            {
                var details = await _pluginManager.GetPluginDetailsAsync(firstPlugin.Id);
                if (details != null)
                {
                    _logger.LogInformation($"\nDetails for plugin: {details.Plugin.Name}");
                    _logger.LogInformation($"  Author: {details.Metadata?.Author ?? "Unknown"}");
                    _logger.LogInformation($"  Description: {details.Metadata?.Description ?? "N/A"}");
                    _logger.LogInformation($"  Assemblies: {details.Assemblies.Count()}");
                    _logger.LogInformation($"  Dependencies: {details.Dependencies.Count()}");
                    _logger.LogInformation($"  Capabilities: {details.Capabilities.Count()}");
                }
            }

            // Get aggregate statistics
            var stats = await _pluginManager.GetStatisticsAsync();
            _logger.LogInformation($"\nPlugin Manager Statistics:");
            _logger.LogInformation($"  TotalPlugins: {stats.TotalPlugins}");
            _logger.LogInformation($"  LoadedPlugins: {stats.LoadedPlugins}");
            _logger.LogInformation($"  ActivePlugins: {stats.ActivePlugins}");
            _logger.LogInformation($"  FailedPlugins: {stats.FailedPlugins}");
            _logger.LogInformation($"  TotalMemoryUsage: {stats.TotalMemoryUsageBytes:N0} bytes");
            _logger.LogInformation($"  TotalLoadContexts: {stats.TotalLoadContexts}");
            _logger.LogInformation($"  AverageLoadTime: {stats.AverageLoadTimeMs:F2} ms");

            // Activate a plugin if it's inactive
            var inactivePlugin = allPlugins.FirstOrDefault(p => p.State == PluginState.Inactive);
            if (inactivePlugin != null)
            {
                _logger.LogInformation($"\nAttempting to activate plugin: {inactivePlugin.Name}");
                bool activated = await _pluginManager.ActivatePluginAsync(inactivePlugin.Id);
                _logger.LogInformation($"Activation {(activated ? "succeeded" : "failed")}");
            }

            // Deactivate a plugin if it's active
            var activePlugin = allPlugins.FirstOrDefault(p => p.State == PluginState.Active);
            if (activePlugin != null)
            {
                _logger.LogInformation($"\nAttempting to deactivate plugin: {activePlugin.Name}");
                bool deactivated = await _pluginManager.DeactivatePluginAsync(activePlugin.Id);
                _logger.LogInformation($"Deactivation {(deactivated ? "succeeded" : "failed")}");
            }

            // Get plugins by status
            var failedPlugins = await _pluginManager.GetPluginsByStatusAsync(PluginStatus.Failed);
            _logger.LogInformation($"\nFailed plugins count: {failedPlugins.Count()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin manager operations");
            throw;
        }
    }
}

// Usage in DI setup
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPluginEngine();

var serviceProvider = services.BuildServiceProvider();
var pluginManager = serviceProvider.GetRequiredService<IPluginManagerService>();

var demo = new PluginManagerDemo(pluginManager, 
    serviceProvider.GetRequiredService<ILogger<PluginManagerDemo>>());
await demo.RunAsync();
```

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

