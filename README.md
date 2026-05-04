# dotnet-plugin-engine

A production-grade, hot-reloadable plugin system for .NET with advanced AssemblyLoadContext isolation, dependency resolution, and versioning support.

## Overview

The dotnet-plugin-engine provides a comprehensive framework for building extensible .NET applications with a plugin architecture. It leverages AssemblyLoadContext for isolation, supports complex dependency graphs, and enables hot-reload scenarios without application restarts.

### Key Features

- **🔌 Plugin Isolation**: Uses AssemblyLoadContext to isolate each plugin in its own loading context
- **🔄 Hot Reload**: Enable plugins to be reloaded without stopping the host application
- **📦 Dependency Management**: Sophisticated dependency resolution with version constraints and circular dependency detection
- **🏗️ Architecture**: Clean separation of concerns with domain models, services, and repository layers
- **🔐 Type Safety**: Fully typed with C# 13 features, nullable reference types enabled
- **⚡ Performance**: In-memory caching, async/await throughout, minimal allocations
- **📊 Diagnostics**: Built-in statistics, health reporting, and event tracking

## Architecture

### Project Structure

```
dotnet-plugin-engine/
├── src/
│   └── PluginEngine/
│       ├── Domain/
│       │   └── Entities/         # Plugin, PluginDependency, PluginCapability, etc.
│       ├── Services/
│       │   ├── Abstractions/     # Service interfaces
│       │   └── Implementations/  # Service implementations
│       ├── Data/
│       │   └── Repositories/     # Data access layer
│       ├── Configuration/        # DI setup and options
│       ├── Exceptions/           # Custom exception types
│       ├── Constants/            # Enums and constants
│       └── PluginEngine.cs       # Main façade class
├── LICENSE
├── README.md
└── .gitignore
```

### Core Components

#### Domain Models
- **Plugin**: Core plugin entity with metadata, dependencies, and capabilities
- **PluginDependency**: Represents a dependency relationship with version constraints
- **PluginCapability**: Represents a feature/capability provided by a plugin
- **PluginMetadata**: Plugin metadata without loading the assembly
- **PluginAssembly**: Assembly-level information and load status
- **AssemblyLoadContextInfo**: Information about plugin isolation contexts
- **VersionInfo**: Semantic versioning with release tracking

#### Services
- **IPluginLoaderService**: Load/unload plugins and manage assembly loading
- **IDependencyResolutionService**: Resolve dependencies and detect circular references
- **IVersioningService**: Version parsing, comparison, and compatibility checking
- **IHotReloadService**: Monitor and perform hot reloads
- **IPluginManagerService**: Orchestrate plugin lifecycle and operations

#### Repository Layer
- **IPluginRepository**: Data access interface for plugin operations
- **PluginRepository**: In-memory implementation (extensible for databases)

#### Exceptions
- **PluginException**: Base exception for all plugin engine errors
- **PluginLoadException**: Exceptions during plugin loading with detailed stage info
- **DependencyResolutionException**: Dependency resolution failures
- **VersionMismatchException**: Version constraint violations

## Usage

### Basic Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;

var services = new ServiceCollection();

// Add plugin engine with custom options
services.AddPluginEngine(options =>
{
    options.PluginDirectory = "plugins";
    options.EnableHotReload = true;
    options.OperationTimeoutMs = 30000;
});

var serviceProvider = services.BuildServiceProvider();
var pluginEngine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();

// Initialize the engine
await pluginEngine.InitializeAsync();

// Load all plugins from directory
var loadedCount = await pluginEngine.LoadAllPluginsAsync();
Console.WriteLine($"Loaded {loadedCount} plugins");

// Get health information
var healthInfo = await pluginEngine.GetHealthInfoAsync();
Console.WriteLine(healthInfo);
```

### Loading a Plugin

```csharp
var pluginLoader = serviceProvider.GetRequiredService<IPluginLoaderService>();

// Load a single plugin
var plugin = await pluginLoader.LoadPluginAsync("path/to/plugin.dll");
Console.WriteLine($"Loaded: {plugin.Name} v{plugin.Version}");

// Load all plugins from a directory
var plugins = await pluginLoader.LoadPluginsFromDirectoryAsync("plugins");
```

### Managing Dependencies

```csharp
var dependencyResolver = serviceProvider.GetRequiredService<IDependencyResolutionService>();

// Resolve all dependencies
var dependencies = await dependencyResolver.ResolveDependenciesAsync(plugin);

// Validate dependencies are satisfied
var valid = await dependencyResolver.ValidateDependenciesAsync(plugin);

// Check for circular dependencies
var hasCircular = await dependencyResolver.HasCircularDependenciesAsync(plugin);

// Get dependency graph
var graph = await dependencyResolver.GetDependencyGraphAsync(plugin.Id);
```

### Hot Reload

```csharp
var hotReloader = serviceProvider.GetRequiredService<IHotReloadService>();

// Start monitoring for changes
await hotReloader.StartHotReloadMonitoringAsync();

// Perform manual reload
var success = await hotReloader.HotReloadPluginAsync(pluginId);

// Register reload callback
hotReloader.RegisterHotReloadCallback(pluginId, async (reloadedPlugin) =>
{
    Console.WriteLine($"Plugin {reloadedPlugin.Name} was reloaded!");
});

// Get statistics
var stats = await hotReloader.GetStatisticsAsync();
Console.WriteLine($"Total reloads: {stats.TotalReloads}");
```

## Configuration Options

The `PluginEngineOptions` class provides comprehensive configuration:

```csharp
options.PluginDirectory = "plugins";                    // Plugin directory
options.EnableHotReload = true;                         // Enable hot reload
options.HotReloadCheckIntervalMs = 5000;               // Check interval
options.EnableDependencyCaching = true;                 // Cache dependencies
options.OperationTimeoutMs = 30000;                    // Operation timeout
options.EnableLogging = true;                           // Enable logging
options.MaxConcurrentPluginLoads = 4;                  // Concurrent loads
options.StrictVersionChecking = true;                   // Strict versions
options.EnableCircularDependencyDetection = true;       // Detect cycles
options.MaxDependencyResolutionAttempts = 10;          // Max resolution attempts
```

## Extension Points

The architecture is designed for extensibility:

### Custom Repository Implementation
Replace `PluginRepository` with a database-backed implementation:

```csharp
services.AddSingleton<IPluginRepository, MyDatabaseRepository>();
```

### Custom Service Implementations
Implement custom versions of any service interface:

```csharp
services.AddSingleton<IPluginLoaderService, MyCustomLoaderService>();
```

### Dependency Injection
Use the standard `IServiceCollection` to add your own services:

```csharp
services.AddPluginEngine(options => { ... });
services.AddScoped<IMyService, MyService>();
```

## Error Handling

The engine provides detailed error information:

```csharp
try
{
    await pluginLoader.LoadPluginAsync(path);
}
catch (PluginLoadException ex)
{
    Console.WriteLine($"Load failed: {ex.ErrorCode}");
    Console.WriteLine($"Stage: {ex.LoadStage}");
    Console.WriteLine($"Message: {ex.Message}");
}
catch (DependencyResolutionException ex)
{
    foreach (var unresolved in ex.UnresolvedDependencies)
    {
        Console.WriteLine($"Unresolved: {unresolved}");
    }
}
```

## Performance Considerations

- **In-Memory Caching**: Dependencies are cached to avoid repeated resolution
- **Async Operations**: All I/O and long-running operations are async
- **Thread Safety**: Services use locks for concurrent access
- **Lazy Loading**: Plugins are only loaded when needed

## Requirements

- **.NET 10** or later
- **C# 13** or later (for language features)
- **Microsoft.Extensions.DependencyInjection** 10.0.0 or later

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Creating Plugins

Plugins are standard .NET DLL assemblies that follow naming conventions and optional interfaces defined by the engine.

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See [LICENSE](LICENSE) for details.

## Support

For issues, feature requests, or contributions, visit the project repository.

---

**Author**: Vladyslav Zaiets | https://sarmkadan.com  
**CTO & Software Architect**
