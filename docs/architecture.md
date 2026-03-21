# Architecture Guide

This document explains the architectural design of dotnet-plugin-engine.

## High-Level Architecture

```
┌──────────────────────────────────────────────┐
│          Host Application                    │
│  ┌──────────────────────────────────────┐   │
│  │  PluginEngine (Façade)               │   │
│  │  - Main entry point                  │   │
│  │  - Orchestrates services             │   │
│  │  - Provides unified interface        │   │
│  └──────────────────────────────────────┘   │
│                    │                        │
│     ┌──────────────┼──────────────┐         │
│     │              │              │         │
│     ▼              ▼              ▼         │
│  ┌──────┐     ┌─────────┐  ┌──────────┐   │
│  │Loader│     │Resolver │  │HotReload │   │
│  │      │     │         │  │          │   │
│  └──────┘     └─────────┘  └──────────┘   │
│     │              │              │         │
│     └──────────────┼──────────────┘         │
│                    │                        │
│  ┌────────────────────────────────────┐   │
│  │  Repository (Data Access)          │   │
│  │  - In-memory implementation        │   │
│  │  - Extensible to databases        │   │
│  └────────────────────────────────────┘   │
└──────────────────────────────────────────────┘
         │              │              │
    ┌────▼─┐       ┌────▼─┐      ┌────▼─┐
    │Plugin│       │Plugin│      │Plugin│
    │ ALC  │       │ ALC  │      │ ALC  │
    │  #1  │       │  #2  │      │  #N  │
    └──────┘       └──────┘      └──────┘
```

## Core Design Patterns

### 1. Façade Pattern
The `PluginEngine` class provides a unified interface to complex subsystems:
- Simplifies API for end users
- Orchestrates multiple services
- Handles initialization and shutdown
- Delegates to specialized services

### 2. Service Layer Architecture
Each major responsibility has a dedicated service interface:
- **Plugin Loading**: `IPluginLoaderService`
- **Dependency Management**: `IDependencyResolutionService`
- **Versioning**: `IVersioningService`
- **Hot Reload**: `IHotReloadService`
- **Orchestration**: `IPluginManagerService`

Benefits:
- Single Responsibility Principle
- Easy to mock for testing
- Extensible via DI
- Clear contracts

### 3. Repository Pattern
The `IPluginRepository` abstracts data storage:
- Default in-memory implementation
- Easy to swap for database-backed implementation
- Supports querying and filtering
- Maintains plugin lifecycle state

### 4. Dependency Injection
Uses `Microsoft.Extensions.DependencyInjection`:
- All services registered in container
- Composable configuration
- Standard .NET ecosystem pattern
- Supports middleware pipeline

### 5. Middleware Pipeline
Optional middleware for cross-cutting concerns:
```
Request → Caching → Logging → RateLimit → ErrorHandling → Plugin
Response ← Caching ← Logging ← RateLimit ← ErrorHandling ←
```

### 6. Observer Pattern (Events)
For plugin lifecycle notifications:
- `PluginEventPublisher`: Emits events
- `PluginEventSubscriber`: Subscribes to events
- Decoupled event handling
- Extensible event types

## Detailed Component Analysis

### Domain Entities

#### Plugin
The central entity representing a loaded plugin:
- Unique ID and name
- Version information
- Metadata (author, description)
- Dependencies collection
- Capabilities collection
- Assembly and ALC references
- Load state

```csharp
public class Plugin
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public PluginMetadata? Metadata { get; set; }
    public IReadOnlyList<PluginDependency> Dependencies { get; set; }
    public IReadOnlyList<PluginCapability> Capabilities { get; set; }
    public PluginAssembly Assembly { get; set; }
    public AssemblyLoadContextInfo? ALCInfo { get; set; }
}
```

#### PluginDependency
Represents a dependency relationship with constraints:
- Depends on another plugin by ID
- Version constraint (e.g., ">=1.0.0,<2.0.0")
- Optional: minimum version, maximum version
- Used in dependency graph analysis

#### PluginCapability
A feature or interface provided by plugin:
- Unique name within plugin
- Version
- Optional: metadata, dependencies
- Allows plugins to advertise features

#### VersionInfo
Semantic versioning implementation:
- Major, minor, patch numbers
- Pre-release suffix (alpha, beta, rc)
- Build metadata
- Comparison and constraint satisfaction

### Service Layer Details

#### IPluginLoaderService
**Responsibility**: Load/unload plugins and manage AssemblyLoadContexts

Key operations:
- `LoadPluginAsync()`: Load single plugin from path
- `LoadPluginsFromDirectoryAsync()`: Load all plugins from directory
- `UnloadPluginAsync()`: Unload plugin and clean up ALC
- `ReloadPluginAsync()`: Unload then reload a plugin

Implementation details:
- Creates isolated AssemblyLoadContext for each plugin
- Discovers assemblies in plugin directory
- Handles assembly binding and resolution
- Manages plugin registry updates

#### IDependencyResolutionService
**Responsibility**: Analyze dependencies and detect conflicts

Key operations:
- `ResolveDependenciesAsync()`: Get all dependencies (transitive)
- `ValidateDependenciesAsync()`: Check all deps are satisfied
- `HasCircularDependenciesAsync()`: Detect cycles
- `GetDependencyGraphAsync()`: Build complete graph

Algorithm highlights:
- Breadth-first search for transitive resolution
- Version constraint validation
- Cycle detection using DFS
- Memoization for performance
- Optional caching layer

Complexity: O(V + E) for graph operations where V = plugins, E = dependencies

#### IVersioningService
**Responsibility**: Parse and validate versions

Key operations:
- `ParseVersion()`: Parse version string
- `CompareVersions()`: Compare two versions
- `SatisfiesConstraint()`: Check constraint satisfaction
- `IsCompatibleVersion()`: Semantic version compatibility

Supports:
- Semantic versioning (major.minor.patch)
- Pre-release versions (1.0.0-alpha, 1.0.0-beta.1)
- Build metadata (1.0.0+build.123)
- Version constraints (>=1.0.0, <2.0.0, ~1.2.0)

#### IHotReloadService
**Responsibility**: Monitor and reload plugins on file changes

Key operations:
- `StartHotReloadMonitoringAsync()`: Begin file monitoring
- `StopHotReloadMonitoringAsync()`: Stop monitoring
- `HotReloadPluginAsync()`: Perform reload
- `RegisterHotReloadCallback()`: Register reload handler

Implementation:
- Uses FileSystemWatcher for change detection
- Debounces rapid changes
- Handles locked files gracefully
- Maintains reload statistics

#### IPluginManagerService
**Responsibility**: High-level plugin orchestration

Key operations:
- `GetAllLoadedPluginsAsync()`: Get loaded plugins
- `GetPluginAsync()`: Get specific plugin
- `ExecutePluginAsync()`: Run plugin with middleware
- `EnablePluginAsync()`: Enable plugin
- `DisablePluginAsync()`: Disable plugin

Coordinates:
- Plugin loader for loading/unloading
- Dependency resolver for validation
- Version service for compatibility
- Event publisher for notifications

### Repository Pattern

#### IPluginRepository
Abstracts plugin storage:
- `AddAsync()`: Store plugin
- `GetAsync()`: Retrieve plugin
- `UpdateAsync()`: Update plugin state
- `DeleteAsync()`: Remove plugin
- `GetAllAsync()`: List all plugins
- `QueryAsync()`: Filtered query

Default implementation: In-memory with thread-safe dictionary

Extension points:
- Implement with Entity Framework for database
- Implement with Redis for distributed cache
- Implement with file system for persistence

### Configuration Layer

#### PluginEngineOptions
Central configuration class:
- Discovery settings (plugin directory)
- Performance tuning (timeouts, concurrent loads)
- Caching configuration
- Hot reload settings
- Validation rules
- Logging configuration
- Webhook configuration

#### DependencyInjectionSetup
Extension method `AddPluginEngine()` registers:
- Services (loader, resolver, versioning, etc.)
- Repository
- Caching
- Event system
- Middleware components
- Background services

### Exception Hierarchy

```
PluginException (base)
├── PluginLoadException
│   ├── AssemblyLoadException
│   ├── MetadataParseException
│   └── ALCCreationException
├── DependencyResolutionException
├── VersionMismatchException
└── PluginOperationException
```

Each exception provides:
- Specific error information
- Context (which plugin, which operation)
- Inner exception for debugging
- Helpful error messages

## Data Flow Examples

### Plugin Loading Flow

```
1. User calls: loader.LoadPluginAsync("plugin.dll")
   ↓
2. Service discovers assembly at path
   ↓
3. Create new AssemblyLoadContext with custom resolver
   ↓
4. Load assembly into ALC
   ↓
5. Parse assembly metadata (name, version, author)
   ↓
6. Extract dependency declarations
   ↓
7. Create Plugin entity
   ↓
8. Store in repository
   ↓
9. Publish PluginLoaded event
   ↓
10. Return Plugin entity
```

### Dependency Resolution Flow

```
1. User calls: resolver.ResolveDependenciesAsync(plugin)
   ↓
2. Get plugin's direct dependencies
   ↓
3. For each dependency:
   a. Look in cache (if enabled)
   b. If not cached, load dependent plugin
   c. Recursively resolve its dependencies
   d. Cache result
   ↓
4. Validate all versions satisfy constraints
   ↓
5. Check for circular dependencies
   ↓
6. Return complete dependency list
```

### Hot Reload Flow

```
1. FileSystemWatcher detects plugin.dll change
   ↓
2. Debounce timer fires (waits for rapid changes)
   ↓
3. Verify plugin is loaded
   ↓
4. Call unload on old plugin
   ↓
5. Load new version from disk
   ↓
6. Re-resolve all dependencies
   ↓
7. Execute reload callbacks
   ↓
8. Publish PluginReloaded event
   ↓
9. Update statistics
```

## Performance Considerations

### Caching Strategy
- **Dependency Cache**: TTL-based cache for resolved dependencies
- **Plugin Metadata Cache**: Loaded plugin info cached
- **Version Cache**: Parsed version objects cached
- **Default TTL**: 5 minutes (configurable)

### Concurrency
- Thread-safe repository with ReaderWriterLockSlim
- Async/await throughout for scalability
- Configurable concurrent plugin loads
- No blocking I/O operations

### Memory Management
- Each plugin isolated in separate ALC
- Unloaded plugins' ALCs can be garbage collected
- Minimal string allocations (using string interning)
- Object pooling for common operations

### Complexity Analysis
- Plugin loading: O(1) per plugin
- Dependency resolution: O(V + E) where V=plugins, E=dependencies
- Version comparison: O(1)
- Plugin lookup: O(1) via dictionary
- Hot reload: O(n) for n dependent plugins

## Extensibility Points

### Custom Services
Replace any service interface:
```csharp
services.AddSingleton<IPluginLoaderService, MyCustomLoaderService>();
```

### Custom Repository
Implement database-backed storage:
```csharp
services.AddSingleton<IPluginRepository, MyDatabaseRepository>();
```

### Custom Middleware
Add cross-cutting concerns:
```csharp
services.AddSingleton<IPluginMiddleware, MyCustomMiddleware>();
```

### Custom Formatters
Output plugins in different formats:
```csharp
services.AddSingleton<IPluginFormatter, MyCustomFormatter>();
```

### Event Handling
Subscribe to plugin lifecycle:
```csharp
subscriber.Subscribe<PluginLoadedEvent>(HandlePluginLoaded);
```

## Thread Safety

All components are thread-safe:
- Repository uses locks for concurrent access
- Services are designed for concurrent calls
- No shared mutable state without synchronization
- AssemblyLoadContext operations are thread-safe

## Testing Strategy

The architecture enables comprehensive testing:
- Interface-based services for mocking
- Dependency injection for test setup
- Repository abstraction for data access testing
- Service isolation for unit tests
- Integration test support via DI

Example:
```csharp
var mockRepository = new Mock<IPluginRepository>();
services.AddSingleton(mockRepository.Object);
// Test service with mocked repository
```

## Migration Path

To extend the engine:
1. Understand the relevant service interface
2. Create custom implementation
3. Register in DI container
4. Test with integration tests
5. Use in your host application

No changes to core engine required - fully extensible.
