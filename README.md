# dotnet-plugin-engine

![CI](https://github.com/sarmkadan/dotnet-plugin-engine/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-plugin-engine)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

A production-grade, hot-reloadable plugin system for .NET with advanced AssemblyLoadContext isolation, sophisticated dependency resolution, versioning support, and enterprise-ready features. Build extensible applications that evolve with your needs.

[![Docker](https://img.shields.io/badge/Docker-ready-blue.svg)](https://github.com/Sarmkadan/dotnet-plugin-engine/pkgs/container/dotnet-plugin-engine)

## v2.0.2 Features
* Plugin sandboxing with resource limits and permission scopes

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration Reference](#configuration-reference)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Overview

The **dotnet-plugin-engine** is a comprehensive framework for building extensible .NET applications with a sophisticated plugin architecture. It's designed for scenarios where you need runtime extensibility, zero-downtime updates, and complex dependency management without restarting your application.

### Why dotnet-plugin-engine?

Modern applications require flexibility and extensibility beyond compile-time constraints. Whether you're building:
- **Microservice ecosystems** with pluggable components
- **SaaS platforms** with tenant-specific plugins
- **Enterprise systems** requiring runtime extensibility
- **Workflow engines** with dynamic capability loading
- **Game systems** with hot-swappable modules

This engine provides a battle-tested foundation for plugin architecture in .NET.

### Key Strengths

- **True Isolation**: AssemblyLoadContext provides complete isolation between plugins
- **Zero Downtime**: Hot-reload plugins without stopping your application
- **Dependency Intelligence**: Sophisticated graph analysis and circular dependency detection
- **Version Management**: Semantic versioning with constraint validation
- **Production Ready**: Built-in health checks, diagnostics, and error handling
- **Performance Optimized**: Async throughout, intelligent caching, minimal allocations
- **Type Safe**: Full C# 13 support with nullable reference types

## Key Features

- **Plugin Isolation**: AssemblyLoadContext ensures plugins are loaded in isolated contexts, preventing version conflicts and namespace pollution
- **Hot Reload**: Monitor file changes and reload plugins without restarting the host application
- **Hot-Swap Without Restart**: Replace a running plugin's assembly with a new version atomically — zero host downtime, automatic rollback on failure
- **Dependency Management**: Sophisticated dependency resolution with version constraints, transitive dependency resolution, and circular dependency detection
- **Dependency Resolver**: Advanced topological install ordering, cross-plugin conflict detection, and complete resolution plans with actionable steps
- **Marketplace Browser**: Discover, search, and install plugins from the remote registry — browse by category, trending, featured, and compatibility matrix
- **Clean Architecture**: Domain-driven design with clear separation of concerns (Domain, Services, Repository, Configuration layers)
- **Type Safety**: Full C# 13 language features, nullable reference types, and compile-time safety
- **Performance**: In-memory dependency caching, fully async/await, minimal memory allocations, efficient file monitoring
- **Diagnostics**: Built-in health monitoring, performance statistics, event tracking, and detailed logging
- **Enterprise Ready**: Comprehensive exception handling, operation timeouts, rate limiting middleware, configurable concurrency
- **Extensible**: Plugin interfaces, custom middleware, event publishing/subscribing, multiple output formatters
- **Remote Integration**: HTTP client for remote plugin registries, webhook support for event notifications

## Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│               Host Application                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │  DependencyInjection Container (IServiceProvider)   │
│  │  ┌────────────────────────────────────────────┐ │   │
│  │  │    PluginEngine (Façade & Orchestrator)    │ │   │
│  │  └────────────────────────────────────────────┘ │   │
│  └──────────────────────────────────────────────────┘   │
│                         │                               │
│         ┌───────────────┼───────────────┐               │
│         │               │               │               │
│  ┌──────▼──────┐ ┌──────▼──────┐ ┌──────▼──────┐       │
│  │ Loader      │ │ Dependency  │ │ Hot Reload  │       │
│  │ Service     │ │ Resolution  │ │ Service     │       │
│  │             │ │ Service     │ │             │       │
│  └──────────────┘ └─────────────┘ └─────────────┘       │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │         Plugin Repository (Data Access)          │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
         │                    │                    │
         │                    │                    │
    ┌────▼─────┐         ┌────▼─────┐        ┌────▼─────┐
    │  Plugin   │         │  Plugin   │        │  Plugin   │
    │  ALC #1   │         │  ALC #2   │        │  ALC #N   │
    │           │         │           │        │           │
    │ Assembly  │         │ Assembly  │        │ Assembly  │
    │ A         │         │ B         │        │ C         │
    └───────────┘         └───────────┘        └───────────┘
```

### Project Structure

```
dotnet-plugin-engine/
├── src/
│   └── PluginEngine/
│       ├── Domain/
│       │   └── Entities/
│       │       ├── Plugin.cs                  # Core plugin entity
│       │       ├── PluginMetadata.cs          # Plugin metadata
│       │       ├── PluginDependency.cs        # Dependency relationships
│       │       ├── PluginCapability.cs        # Feature capabilities
│       │       ├── PluginAssembly.cs          # Assembly information
│       │       ├── AssemblyLoadContextInfo.cs # ALC details
│       │       └── VersionInfo.cs             # Version management
│       ├── Services/
│       │   ├── Abstractions/
│       │   │   ├── IPluginLoaderService.cs      # Plugin loading
│       │   │   ├── IDependencyResolutionService.cs
│       │   │   ├── IVersioningService.cs        # Version handling
│       │   │   ├── IHotReloadService.cs         # Hot reload
│       │   │   └── IPluginManagerService.cs     # Orchestration
│       │   └── Implementations/
│       │       ├── PluginLoaderService.cs
│       │       ├── DependencyResolutionService.cs
│       │       ├── VersioningService.cs
│       │       ├── HotReloadService.cs
│       │       └── PluginManagerService.cs
│       ├── Data/
│       │   └── Repositories/
│       │       ├── IPluginRepository.cs
│       │       └── PluginRepository.cs
│       ├── Configuration/
│       │   ├── DependencyInjectionSetup.cs
│       │   ├── LoggingConfiguration.cs
│       │   ├── PluginEngineOptions.cs
│       │   └── WebhookConfiguration.cs
│       ├── Exceptions/
│       │   ├── PluginException.cs
│       │   ├── PluginLoadException.cs
│       │   ├── DependencyResolutionException.cs
│       │   └── VersionMismatchException.cs
│       ├── Events/
│       │   ├── IPluginEvent.cs
│       │   ├── PluginEventPublisher.cs
│       │   └── PluginEventSubscriber.cs
│       ├── Middleware/
│       │   ├── IPluginMiddleware.cs
│       │   ├── CachingMiddleware.cs
│       │   ├── LoggingMiddleware.cs
│       │   ├── ErrorHandlingMiddleware.cs
│       │   └── RateLimitMiddleware.cs
│       ├── Caching/
│       │   ├── IPluginCache.cs
│       │   └── MemoryPluginCache.cs
│       ├── Integration/
│       │   ├── HttpPluginClient.cs
│       │   ├── IIntegrationClient.cs
│       │   ├── RemotePluginRegistry.cs
│       │   └── WebhookHandler.cs
│       ├── Formatters/
│       │   ├── IPluginFormatter.cs
│       │   ├── JsonPluginFormatter.cs
│       │   ├── XmlPluginFormatter.cs
│       │   └── CsvPluginFormatter.cs
│       ├── Utils/
│       │   ├── Extensions/
│       │   ├── Helpers/
│       │   └── Validators/
│       ├── BackgroundServices/
│       │   ├── BackgroundPluginMonitor.cs
│       │   └── PluginHealthCheckService.cs
│       ├── PluginEngine.cs                 # Main façade
│       └── PluginEngine.csproj
├── examples/
│   ├── BasicPluginHost/
│   ├── WebApiWithPlugins/
│   ├── PluginMonitoring/
│   ├── DependencyResolution/
│   ├── HotReloadDemo/
│   └── AdvancedScenarios/
├── docs/
│   ├── getting-started.md
│   ├── architecture.md
│   ├── api-reference.md
│   ├── alc-isolation-guide.md
│   ├── deployment.md
│   └── faq.md
├── .github/
│   └── workflows/
│       └── build.yml
├── .editorconfig
├── Makefile
├── Dockerfile
├── docker-compose.yml
├── CHANGELOG.md
├── LICENSE
├── README.md
└── .gitignore
```

### Core Components Explained

#### Domain Entities
- **Plugin**: The core entity representing a loaded plugin with metadata, version, capabilities, and dependencies
- **PluginDependency**: Represents a dependency relationship with version constraints (e.g., "requires >= 1.0.0, < 2.0.0")
- **PluginCapability**: Represents a feature or capability provided by a plugin
- **PluginMetadata**: Non-loaded plugin metadata extracted from assembly metadata
- **PluginAssembly**: Low-level assembly information including file paths and load status
- **AssemblyLoadContextInfo**: Details about the AssemblyLoadContext isolating a plugin
- **VersionInfo**: Semantic versioning implementation with pre-release and build metadata support

#### Service Layer
- **IPluginLoaderService**: Handles loading/unloading plugins, assembly discovery, and ALC management
- **IDependencyResolutionService**: Sophisticated graph analysis, circular dependency detection, transitive resolution
- **IVersioningService**: Version parsing, comparison, constraint validation, compatibility checking
- **IHotReloadService**: File monitoring, change detection, plugin reload orchestration, callback management
- **IPluginManagerService**: High-level orchestration of plugin lifecycle and operations

#### Additional Components
- **Repository**: Data persistence abstraction (in-memory implementation, extensible to databases)
- **Middleware Pipeline**: Plugin execution middleware for caching, logging, rate limiting, error handling
- **Event System**: Publisher/subscriber pattern for plugin lifecycle events
- **Caching**: Intelligent caching of dependency graphs and plugin metadata
- **Integration**: HTTP client and webhook support for remote plugin registries
- **Formatters**: JSON, XML, CSV output formatters for plugin information
- **Background Services**: Health monitoring and plugin change detection

## Installation

### Package Manager

```bash
dotnet package add DotnetPluginEngine
```

### Manual Build & Install

```bash
git clone https://github.com/Sarmkadan/dotnet-plugin-engine.git
cd dotnet-plugin-engine
dotnet build -c Release
dotnet pack -c Release --output ./nupkg
```

### Source Integration

Add the source directly to your project:

```bash
git submodule add https://github.com/Sarmkadan/dotnet-plugin-engine.git src/PluginEngine
```

Then reference in your `.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="src/PluginEngine/PluginEngine.csproj" />
</ItemGroup>
```

## Quick Start

### 1. Configure Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;

var services = new ServiceCollection();

services.AddPluginEngine(options =>
{
    options.PluginDirectory = "./plugins";
    options.EnableHotReload = true;
    options.HotReloadCheckIntervalMs = 5000;
    options.EnableLogging = true;
    options.OperationTimeoutMs = 30000;
});

var serviceProvider = services.BuildServiceProvider();
```

### 2. Initialize the Engine

```csharp
var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();
await engine.InitializeAsync();
```

### 3. Load Plugins

```csharp
var count = await engine.LoadAllPluginsAsync();
Console.WriteLine($"Loaded {count} plugins");
```

### 4. Use Plugin Services

```csharp
var manager = serviceProvider.GetRequiredService<IPluginManagerService>();
var plugins = await manager.GetAllLoadedPluginsAsync();

foreach (var plugin in plugins)
{
    Console.WriteLine($"{plugin.Name} v{plugin.Version}");
    foreach (var capability in plugin.Capabilities)
    {
        Console.WriteLine($"  - {capability.Name}");
    }
}
```

## Usage Examples

### Example 1: Basic Plugin Loading and Execution

```csharp
var loader = serviceProvider.GetRequiredService<IPluginLoaderService>();

try
{
    // Load a single plugin
    var plugin = await loader.LoadPluginAsync("./plugins/MyPlugin.dll");
    
    Console.WriteLine($"Plugin: {plugin.Name}");
    Console.WriteLine($"Version: {plugin.Version}");
    Console.WriteLine($"Author: {plugin.Metadata?.Author}");
    Console.WriteLine($"Description: {plugin.Metadata?.Description}");
}
catch (PluginLoadException ex)
{
    Console.WriteLine($"Failed to load plugin: {ex.Message}");
}
```

### Example 2: Dependency Resolution and Validation

```csharp
var resolver = serviceProvider.GetRequiredService<IDependencyResolutionService>();
var manager = serviceProvider.GetRequiredService<IPluginManagerService>();

var plugin = await manager.GetPluginAsync("plugin-id");

// Get all dependencies with transitive resolution
var dependencies = await resolver.ResolveDependenciesAsync(plugin);
Console.WriteLine($"Total dependencies: {dependencies.Count}");

// Validate all dependencies are satisfied
var isValid = await resolver.ValidateDependenciesAsync(plugin);
Console.WriteLine($"Dependencies valid: {isValid}");

// Detect circular dependencies
var hasCircular = await resolver.HasCircularDependenciesAsync(plugin);
Console.WriteLine($"Has circular deps: {hasCircular}");

// Get full dependency graph
var graph = await resolver.GetDependencyGraphAsync(plugin.Id);
foreach (var dep in graph.Dependencies)
{
    Console.WriteLine($"  - {dep.Name}: {dep.VersionConstraint}");
}
```

### Example 3: Hot Reload Configuration

```csharp
var hotReloader = serviceProvider.GetRequiredService<IHotReloadService>();

// Start automatic monitoring
await hotReloader.StartHotReloadMonitoringAsync();

// Register reload callbacks
await hotReloader.RegisterHotReloadCallback("plugin-id", async plugin =>
{
    Console.WriteLine($"Plugin {plugin.Name} was reloaded at {DateTime.UtcNow}");
    // Perform any necessary cleanup or re-initialization
});

// Monitor hot reload statistics
var stats = await hotReloader.GetStatisticsAsync();
Console.WriteLine($"Total reloads: {stats.TotalReloads}");
Console.WriteLine($"Average reload time: {stats.AverageReloadTimeMs}ms");
```

### Example 4: Version Constraint Validation

```csharp
var versionService = serviceProvider.GetRequiredService<IVersioningService>();

// Parse versions
var v1 = versionService.ParseVersion("1.2.3");
var v2 = versionService.ParseVersion("1.2.4");

// Compare versions
var comparison = versionService.CompareVersions(v1, v2);
Console.WriteLine($"v1.2.3 < v1.2.4: {comparison < 0}");

// Validate version constraints
var constraint = ">=1.0.0,<2.0.0";
var satisfies = versionService.SatisfiesConstraint(v1, constraint);
Console.WriteLine($"v1.2.3 satisfies {constraint}: {satisfies}");

// Check compatibility
var compatible = versionService.IsCompatibleVersion(v1, v2);
Console.WriteLine($"Versions compatible: {compatible}");
```

### Example 5: Event Publishing and Subscription

```csharp
var publisher = serviceProvider.GetRequiredService<PluginEventPublisher>();
var subscriber = serviceProvider.GetRequiredService<PluginEventSubscriber>();

// Subscribe to plugin loaded events
subscriber.Subscribe<PluginLoadedEvent>(async @event =>
{
    Console.WriteLine($"Plugin loaded: {@event.Plugin.Name}");
    // Perform any initialization logic
});

// Subscribe to plugin unloaded events
subscriber.Subscribe<PluginUnloadedEvent>(async @event =>
{
    Console.WriteLine($"Plugin unloaded: {@event.Plugin.Name}");
    // Perform cleanup
});

// Events are automatically published by the engine during plugin lifecycle
```

### Example 6: Middleware Pipeline

```csharp
var manager = serviceProvider.GetRequiredService<IPluginManagerService>();

// Execute plugin with middleware pipeline
var result = await manager.ExecutePluginAsync(pluginId, new PluginExecutionContext
{
    Parameters = new Dictionary<string, object> 
    {
        { "input", "data" }
    }
});

// Middleware handles caching, logging, rate limiting, and error handling automatically
if (result.IsSuccess)
{
    Console.WriteLine($"Result: {result.Value}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

### Example 7: Remote Plugin Registry Integration

```csharp
var registry = serviceProvider.GetRequiredService<RemotePluginRegistry>();

// Discover plugins from remote registry
var remotePlugins = await registry.DiscoverPluginsAsync("https://registry.example.com");

foreach (var pluginInfo in remotePlugins)
{
    Console.WriteLine($"Available: {pluginInfo.Name} v{pluginInfo.Version}");
    
    // Download and install
    await registry.DownloadAndInstallAsync(pluginInfo, "./plugins");
}
```

### Example 8: Health Monitoring

```csharp
var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();

var health = await engine.GetHealthInfoAsync();
Console.WriteLine($"Loaded plugins: {health.LoadedPluginsCount}");
Console.WriteLine($"Failed plugins: {health.FailedPluginsCount}");
Console.WriteLine($"Average load time: {health.AveragePluginLoadTimeMs}ms");

foreach (var pluginHealth in health.PluginHealthStatus)
{
    Console.WriteLine($"  {pluginHealth.Name}: {pluginHealth.Status}");
    if (!string.IsNullOrEmpty(pluginHealth.ErrorMessage))
    {
        Console.WriteLine($"    Error: {pluginHealth.ErrorMessage}");
    }
}
```

### Example 9: Custom Plugin Discovery

```csharp
var discoveryService = serviceProvider.GetRequiredService<PluginDiscoveryService>();

// Scan directory with custom pattern
var pluginPaths = await discoveryService.DiscoverAsync(
    "./plugins",
    searchPattern: "*.plugin.dll",
    recursive: true
);

var loader = serviceProvider.GetRequiredService<IPluginLoaderService>();

foreach (var path in pluginPaths)
{
    var plugin = await loader.LoadPluginAsync(path);
    Console.WriteLine($"Discovered and loaded: {plugin.Name}");
}
```

### Example 10: Error Handling and Recovery

```csharp
var manager = serviceProvider.GetRequiredService<IPluginManagerService>();

try
{
    await manager.EnablePluginAsync("failing-plugin");
}
catch (DependencyResolutionException ex)
{
    Console.WriteLine("Dependency resolution failed:");
    foreach (var unresolved in ex.UnresolvedDependencies)
    {
        Console.WriteLine($"  - Missing: {unresolved}");
    }
    
    // Attempt to install missing dependencies from registry
    var registry = serviceProvider.GetRequiredService<RemotePluginRegistry>();
    foreach (var missing in ex.UnresolvedDependencies)
    {
        await registry.DownloadAndInstallAsync(missing, "./plugins");
    }
}
catch (VersionMismatchException ex)
{
    Console.WriteLine($"Version conflict: {ex.Message}");
    Console.WriteLine($"Required: {ex.RequiredVersion}");
    Console.WriteLine($"Available: {ex.AvailableVersion}");
}
catch (PluginLoadException ex)
{
    Console.WriteLine($"Plugin load failed at {ex.LoadStage}: {ex.Message}");
}
```

### Example 11: Advanced Dependency Graph Analysis

```csharp
var resolver = serviceProvider.GetRequiredService<IDependencyResolutionService>();

var graph = await resolver.GetDependencyGraphAsync(pluginId);

// Analyze dependency tree
void PrintTree(PluginDependency dep, int depth = 0)
{
    Console.WriteLine(new string(' ', depth * 2) + $"- {dep.Name} {dep.VersionConstraint}");
    foreach (var child in graph.GetDependenciesOf(dep.Id))
    {
        PrintTree(child, depth + 1);
    }
}

foreach (var root in graph.RootDependencies)
{
    PrintTree(root);
}
```

### Example 12: Plugin Output Formatting

```csharp
var manager = serviceProvider.GetRequiredService<IPluginManagerService>();
var plugins = await manager.GetAllLoadedPluginsAsync();

// Use different formatters
var jsonFormatter = serviceProvider.GetRequiredService<JsonPluginFormatter>();
var xmlFormatter = serviceProvider.GetRequiredService<XmlPluginFormatter>();
var csvFormatter = serviceProvider.GetRequiredService<CsvPluginFormatter>();

// Format as JSON
string jsonOutput = await jsonFormatter.FormatAsync(plugins);
Console.WriteLine(jsonOutput);

// Format as XML
string xmlOutput = await xmlFormatter.FormatAsync(plugins);
Console.WriteLine(xmlOutput);

// Format as CSV
string csvOutput = await csvFormatter.FormatAsync(plugins);
Console.WriteLine(csvOutput);
```

### Example 13: Plugin Marketplace Browser

Register the marketplace services in your DI setup:

```csharp
services.AddPluginEngine();
services.AddPluginMarketplace(); // registers IPluginMarketplaceService + IMarketplaceBrowserService
```

Browse and install plugins at runtime:

```csharp
var browser = serviceProvider.GetRequiredService<IMarketplaceBrowserService>();

// Fetch the home page (featured + trending + categories) in one call
var home = await browser.GetHomePageAsync();
Console.WriteLine($"Featured: {home.Data!.Featured.Count} plugins");
Console.WriteLine($"Trending: {home.Data.Trending.Count} plugins");

// Browse a category
var loggingPlugins = await browser.BrowseCategoryAsync("logging");
foreach (var entry in loggingPlugins.Data!)
    Console.WriteLine($"  {entry.Name} v{entry.LatestVersion} — {entry.Downloads:N0} downloads");

// Search by keyword
var marketplace = serviceProvider.GetRequiredService<IPluginMarketplaceService>();
var results = await marketplace.SearchAsync(new MarketplaceSearchFilter
{
    Query     = "authentication",
    SortOrder = MarketplaceSortOrder.Rating,
    PageSize  = 10
});

// Install from the marketplace
await marketplace.InstallAsync(results.Data![0].Id, results.Data[0].LatestVersion, "./plugins");
```

CLI usage:

```bash
# Search for plugins
plugin-engine marketplace --action search --query logging --limit 10

# View plugin details
plugin-engine marketplace --action info --id <guid>

# Install a specific version
plugin-engine marketplace --action install --id <guid> --version 2.1.0 --target ./plugins
```

### Example 14: Hot-Swap Without Restart

`IHotSwapService` replaces a running plugin's assembly atomically. Unlike hot-reload (which reloads the same path), hot-swap can point to an entirely new assembly file — for example, after deploying a new version.

```csharp
var hotSwap = serviceProvider.GetRequiredService<IHotSwapService>();

// Register a callback invoked after every successful swap
hotSwap.RegisterPostSwapCallback(plugin.Id, async updated =>
{
    Console.WriteLine($"Plugin '{updated.Name}' is now running v{updated.Version}");
    // Re-bind any services that depend on the plugin
});

// Swap to a new assembly (host keeps serving traffic throughout)
var result = await hotSwap.SwapPluginAsync(plugin.Id, "./plugins/MyPlugin.v2.dll");

if (result.Success)
{
    Console.WriteLine(result.Message); // "Plugin 'MyPlugin' swapped successfully in 42ms"
}
else
{
    // Rollback was automatically attempted on failure
    Console.WriteLine($"Swap failed: {result.Message}");
}

// Inspect swap history
var history = await hotSwap.GetSwapHistoryAsync(plugin.Id);
foreach (var record in history.Data!)
    Console.WriteLine($"  {record.SwappedAtUtc:u}  {record.PreviousAssemblyPath} → {record.NewAssemblyPath}  success={record.Success}");

// Explicit rollback to the previous assembly
await hotSwap.RollbackSwapAsync(plugin.Id);
```

CLI usage:

```bash
plugin-engine swap --id <guid> --path ./plugins/MyPlugin.v2.dll
```

### Example 15: Plugin Dependency Resolver

`IPluginDependencyResolver` provides set-oriented analysis on top of the basic `IDependencyResolutionService`: topological install ordering, cross-plugin conflict detection, and complete resolution plans.

```csharp
var resolver = serviceProvider.GetRequiredService<IPluginDependencyResolver>();
var loader   = serviceProvider.GetRequiredService<IPluginLoaderService>();

var plugins = (await loader.GetAllLoadedPluginsAsync()).ToList();

// 1. Compute correct installation order (Kahn's topological sort)
var orderResult = await resolver.GetInstallOrderAsync(plugins);
Console.WriteLine("Install order:");
foreach (var p in orderResult.Data!)
    Console.WriteLine($"  {p.Name} v{p.Version}");

// 2. Detect version conflicts across the entire plugin set
var conflictsResult = await resolver.FindConflictsAsync(plugins);
if (conflictsResult.Data!.Count == 0)
{
    Console.WriteLine("No dependency conflicts.");
}
else
{
    foreach (var conflict in conflictsResult.Data)
        Console.WriteLine($"  ⚠ {conflict.Description}");
}

// 3. Build a full resolution plan for a single plugin
var planResult = await resolver.BuildResolutionPlanAsync(myPlugin.Id);
var plan = planResult.Data!;

Console.WriteLine($"Plan executable: {plan.IsExecutable}");
foreach (var step in plan.Steps)
    Console.WriteLine($"  {step.Order}. [{step.Action}] {step.PluginName} v{step.Version}");
```

CLI usage:

```bash
plugin-engine resolve --id <guid>
```



## API Reference

### Main Façade: PluginEngine

```csharp
public class PluginEngine
{
    // Initialization
    Task InitializeAsync();
    
    // Plugin Loading
    Task<int> LoadAllPluginsAsync();
    Task<int> LoadAllPluginsAsync(string directory);
    
    // Plugin Management
    Task<IReadOnlyList<Plugin>> GetLoadedPluginsAsync();
    Task UnloadPluginAsync(string pluginId);
    
    // Health & Diagnostics
    Task<EngineHealthInfo> GetHealthInfoAsync();
    Task<PluginStatistics> GetStatisticsAsync();
    
    // Shutdown
    Task ShutdownAsync();
}
```

### IPluginLoaderService

```csharp
public interface IPluginLoaderService
{
    Task<Plugin> LoadPluginAsync(string assemblyPath);
    Task<IReadOnlyList<Plugin>> LoadPluginsFromDirectoryAsync(string directory);
    Task UnloadPluginAsync(Plugin plugin);
    Task<Plugin> ReloadPluginAsync(string pluginId);
}
```

### IDependencyResolutionService

```csharp
public interface IDependencyResolutionService
{
    Task<IReadOnlyList<PluginDependency>> ResolveDependenciesAsync(Plugin plugin);
    Task<bool> ValidateDependenciesAsync(Plugin plugin);
    Task<bool> HasCircularDependenciesAsync(Plugin plugin);
    Task<DependencyGraph> GetDependencyGraphAsync(string pluginId);
}
```

### IHotReloadService

```csharp
public interface IHotReloadService
{
    Task StartHotReloadMonitoringAsync();
    Task StopHotReloadMonitoringAsync();
    Task<bool> HotReloadPluginAsync(string pluginId);
    Task RegisterHotReloadCallback(string pluginId, Func<Plugin, Task> callback);
    Task<HotReloadStatistics> GetStatisticsAsync();
}
```

### IVersioningService

```csharp
public interface IVersioningService
{
    VersionInfo ParseVersion(string versionString);
    int CompareVersions(VersionInfo v1, VersionInfo v2);
    bool SatisfiesConstraint(VersionInfo version, string constraint);
    bool IsCompatibleVersion(VersionInfo v1, VersionInfo v2);
}
```

### IPluginManagerService

```csharp
public interface IPluginManagerService
{
    Task<IReadOnlyList<Plugin>> GetAllLoadedPluginsAsync();
    Task<Plugin?> GetPluginAsync(string pluginId);
    Task<PluginOperationResult> ExecutePluginAsync(string pluginId, PluginExecutionContext context);
    Task EnablePluginAsync(string pluginId);
    Task DisablePluginAsync(string pluginId);
}
```

## Configuration Reference

### PluginEngineOptions

```csharp
public class PluginEngineOptions
{
    // Paths & Discovery
    public string PluginDirectory { get; set; } = "plugins";
    
    // Hot Reload
    public bool EnableHotReload { get; set; } = true;
    public int HotReloadCheckIntervalMs { get; set; } = 5000;
    
    // Caching
    public bool EnableDependencyCaching { get; set; } = true;
    public int DependencyCacheTTLMs { get; set; } = 300000; // 5 minutes
    
    // Performance
    public int OperationTimeoutMs { get; set; } = 30000;
    public int MaxConcurrentPluginLoads { get; set; } = 4;
    
    // Validation
    public bool StrictVersionChecking { get; set; } = true;
    public bool EnableCircularDependencyDetection { get; set; } = true;
    public int MaxDependencyResolutionAttempts { get; set; } = 10;
    
    // Diagnostics
    public bool EnableLogging { get; set; } = true;
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    
    // Webhooks
    public WebhookConfiguration? WebhookConfig { get; set; }
}
```

### Configuration Example

```csharp
services.AddPluginEngine(options =>
{
    // Discovery
    options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
    
    // Hot Reload Configuration
    options.EnableHotReload = true;
    options.HotReloadCheckIntervalMs = 3000; // Check every 3 seconds
    
    // Caching Strategy
    options.EnableDependencyCaching = true;
    options.DependencyCacheTTLMs = 600000; // 10 minutes
    
    // Performance Tuning
    options.MaxConcurrentPluginLoads = Environment.ProcessorCount;
    options.OperationTimeoutMs = 60000; // 1 minute timeout
    
    // Validation Rules
    options.StrictVersionChecking = true;
    options.EnableCircularDependencyDetection = true;
    options.MaxDependencyResolutionAttempts = 15;
    
    // Logging
    options.EnableLogging = true;
    options.MinimumLogLevel = LogLevel.Debug;
    
    // Webhooks
    options.WebhookConfig = new WebhookConfiguration
    {
        Enabled = true,
        BaseUrl = "https://myapp.example.com/webhooks",
        Events = new[] { "plugin.loaded", "plugin.failed" }
    };
});
```

## Performance

The plugin engine is designed for minimal overhead in production workloads. Representative benchmarks measured on a single-core equivalent workload (.NET 10, x64, Linux):

| Operation | Metric |
|---|---|
| Plugin load (cold, from disk) | ~8 ms per plugin |
| Plugin load (metadata cached) | < 1 ms per plugin |
| Dependency resolution — 10 nodes | ~5 ms |
| Dependency resolution — 100 nodes | < 25 ms |
| Circular dependency detection | O(V + E) — scales linearly with graph size |
| Hot reload detection latency | ~100 ms (file-watcher polling) |
| Event publish throughput | > 50,000 events/sec |
| Concurrent plugin loads | 4× parallel (default `MaxConcurrentPluginLoads`) |

**Key optimisations:**
- Dependency graphs are cached in-memory with a configurable TTL (`DependencyCacheTTLMs`), eliminating repeated resolution for stable plugin sets.
- Each plugin runs in its own `AssemblyLoadContext`, so loading or unloading one plugin has zero impact on the others.
- All service calls are fully `async`/`await` — the host thread is never blocked during I/O or reflection.

> Actual numbers vary with plugin size, dependency depth, and hardware. Use `engine.GetStatisticsAsync()` to capture real metrics in your environment.

## Troubleshooting

### Plugin Fails to Load

**Symptom**: `PluginLoadException` with message "Failed to load assembly"

**Solutions**:
1. Verify the plugin DLL path is correct and file exists
2. Check that the plugin targets .NET 10 or compatible framework
3. Ensure all plugin dependencies are available in the same directory
4. Review the full exception stack trace for dependency loading errors
5. Enable debug logging: `options.MinimumLogLevel = LogLevel.Debug`

```csharp
// Debug approach
try
{
    var plugin = await loader.LoadPluginAsync(path);
}
catch (PluginLoadException ex)
{
    Console.WriteLine($"Stage: {ex.LoadStage}");
    Console.WriteLine($"Inner: {ex.InnerException?.Message}");
}
```

### Circular Dependency Detected

**Symptom**: `DependencyResolutionException` mentioning circular dependencies

**Solutions**:
1. Review plugin dependency declarations for cycles (A→B→C→A)
2. Refactor plugins to break the cycle by creating a shared utility plugin
3. Disable circular detection only if intentional: `options.EnableCircularDependencyDetection = false`
4. Use the dependency graph analyzer to visualize the issue

```csharp
var graph = await resolver.GetDependencyGraphAsync(pluginId);
// Inspect graph for cycles
```

### Version Mismatch Errors

**Symptom**: `VersionMismatchException` "Unsatisfied version constraint"

**Solutions**:
1. Ensure all plugins declare compatible versions
2. Use semantic versioning correctly (major.minor.patch)
3. For development, temporarily disable strict checking: `options.StrictVersionChecking = false`
4. Update constraint formats: `>=1.0.0,<2.0.0` or `~1.2.0`

### Hot Reload Not Working

**Symptom**: Plugin changes not detected, hot reload callbacks not triggered

**Solutions**:
1. Verify hot reload is enabled: `options.EnableHotReload = true`
2. Check file system permissions on plugin directory
3. Verify plugin DLL is in the configured plugin directory
4. Check that plugin has not been locked by another process
5. Review hot reload statistics for errors

```csharp
var stats = await hotReloader.GetStatisticsAsync();
Console.WriteLine($"Last error: {stats.LastErrorMessage}");
```

### Memory Issues with Many Plugins

**Symptom**: High memory usage, GC pressure increasing over time

**Solutions**:
1. Enable dependency caching with appropriate TTL:
   ```csharp
   options.EnableDependencyCaching = true;
   options.DependencyCacheTTLMs = 300000; // 5 minutes
   ```
2. Reduce hot reload check interval if monitoring many plugins
3. Implement plugin unloading for unused plugins
4. Monitor with `GetStatisticsAsync()` to identify resource hogs

### Timeout Errors on Plugin Operations

**Symptom**: Operations timeout with `OperationTimeoutException`

**Solutions**:
1. Increase operation timeout:
   ```csharp
   options.OperationTimeoutMs = 60000; // 1 minute instead of 30 seconds
   ```
2. Check for slow dependency resolution (reduce constraint complexity)
3. Profile plugins to identify performance bottlenecks
4. Increase concurrent load limit if CPU-bound
5. Review logging for what operation is timing out

### Plugin Dependencies Not Resolved

**Symptom**: `DependencyResolutionException` with unresolved dependencies

**Solutions**:
1. Ensure all dependency plugins are in the plugin directory
2. Verify dependency naming matches exactly (case-sensitive)
3. Check dependency version constraints are satisfiable
4. Use remote plugin registry to auto-download missing dependencies
5. Increase max resolution attempts: `options.MaxDependencyResolutionAttempts = 20`

## Testing

```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test class
dotnet test --filter "FullyQualifiedName~VersionHelperTests"

# Run tests in watch mode during development
dotnet watch test --project tests/dotnet-plugin-engine.Tests
```

The test suite uses **xUnit**, **Moq**, and **FluentAssertions**. Unit tests live under `tests/dotnet-plugin-engine.Tests/` and cover core domain entities, version resolution logic, and operation result handling.

## Related Projects

- [dotnet-distributed-lock](https://github.com/sarmkadan/dotnet-distributed-lock) — Distributed locking library for .NET — Redis, SQLite, PostgreSQL backends with fencing tokens and auto-renewal

### Integration Examples

**Coordinating hot reload across multiple application instances**

When the plugin engine runs in a horizontally-scaled deployment, use `dotnet-distributed-lock` to ensure only one instance reloads a plugin at a time:

```csharp
await using var @lock = await distributedLock.AcquireAsync(
    "plugin-reload:analytics-plugin",
    timeout: TimeSpan.FromSeconds(30));

if (@lock.Acquired)
{
    await hotReloader.HotReloadPluginAsync("analytics-plugin");
}
```

**Serialising first-time plugin initialisation**

Guard the expensive plugin discovery and load phase so only one replica races to initialise while the others wait:

```csharp
await using var @lock = await distributedLock.AcquireAsync(
    "plugin-engine:init", timeout: TimeSpan.FromMinutes(2));

await engine.InitializeAsync();
var count = await engine.LoadAllPluginsAsync();
Console.WriteLine($"Loaded {count} plugins");
```

## Contributing

### Setting Up Development Environment

```bash
# Clone repository
git clone https://github.com/Sarmkadan/dotnet-plugin-engine.git
cd dotnet-plugin-engine

# Restore packages
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Pack NuGet package
dotnet pack -c Release
```

### Code Guidelines

1. Follow C# naming conventions (PascalCase for public members)
2. Add XML documentation comments for all public members
3. Use async/await throughout for all I/O operations
4. Include appropriate logging at DEBUG level for diagnostics
5. Add unit tests for new functionality
6. Run `dotnet format` before committing
7. Ensure no compiler warnings

### Commit Message Format

```
<type>: <subject>

<body>

<footer>
```

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

Example:
```
feat: Add webhook support for plugin lifecycle events

Implement webhook publishing for plugin loaded, unloaded, and failed events.
Add WebhookConfiguration and WebhookHandler classes.
Add tests for webhook delivery and retry logic.

Closes #123
```

### Pull Request Process

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make your changes and add tests
4. Ensure all tests pass: `dotnet test`
5. Format code: `dotnet format`
6. Commit with descriptive messages
7. Push to your fork
8. Create a Pull Request with detailed description

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See [LICENSE](LICENSE) for full details.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
