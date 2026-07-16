# Architecture

This document describes how the plugin engine is actually put together - the moving parts, the data flow, and why some decisions were made the way they were.

## Overview

The solution is a single-library plugin runtime for .NET (`src/PluginEngine`) plus tests, benchmarks and usage examples:

```
src/PluginEngine/            the engine itself (class library)
tests/                       xUnit test project
benchmarks/                  BenchmarkDotNet project
examples/                    standalone usage samples
```

The engine loads plugin assemblies from a directory into isolated, collectible `AssemblyLoadContext`s, resolves inter-plugin dependencies, checks version compatibility, and supports hot reload / hot swap of plugins at runtime. Everything is exposed through DI and a thin facade class.

## Component breakdown

### Facade: `PluginEngine`

`PluginEngine.cs` is a deliberate facade, not a god object. It holds references to the five core services (`IPluginManagerService`, `IPluginLoaderService`, `IDependencyResolutionService`, `IVersioningService`, `IHotReloadService`) plus `PluginEngineOptions`, and forwards to them. It adds only trivial orchestration (`LoadAllPluginsAsync`, `UnloadAllPluginsAsync`, `GetHealthInfoAsync`). All real logic lives in the services, so consumers can either use the facade for convenience or inject individual services when they need finer control.

### Services (`Services/Abstractions` + `Services/Implementations`)

Interface-per-service, implementations registered as singletons:

- **`PluginLoaderService`** - the heart of the engine. Contains a private nested `PluginAssemblyLoadContext : AssemblyLoadContext` created with `isCollectible: true` and an `AssemblyDependencyResolver` per plugin path. Each plugin gets its own load context, which is what makes unloading and hot reload possible. Loaded plugins are tracked in a dictionary of `(Plugin, AssemblyLoadContext, List<IPluginLifecycle>)` guarded by a lock.
- **`PluginManagerService`** - lifecycle coordinator: initialize/shutdown the engine, aggregate status and statistics.
- **`DependencyResolutionService`** / **`PluginDependencyResolver`** - build and validate the dependency graph between plugins (topological ordering, circular dependency detection - see `EnableCircularDependencyDetection` in options).
- **`VersioningService`** - semantic version parsing and compatibility checks (`StrictVersionChecking` option).
- **`HotReloadService`** - watches the plugin directory and reloads changed plugins (`EnableHotReload`, `HotReloadCheckIntervalMs`).
- **`HotSwapService`** - replaces a running plugin version with another without a full engine restart.

### Domain (`Domain/Entities`)

Plain entities, no persistence framework: `Plugin`, `PluginMetadata`, `PluginDependency`, `PluginCapability`, `PluginAssembly`, `VersionInfo`, `AssemblyLoadContextInfo`. They carry their own validation (`IsValid()`-style methods) and JSON/extension helpers live in sibling `*Extensions.cs` / `*JsonExtensions.cs` files. Keeping serialization out of the entity classes themselves was intentional - entities stay small and the JSON shape can change without touching domain code.

### Data (`Data/Repositories`)

`IPluginRepository` / `PluginRepository` - in-memory registry of known plugins. It is an interface specifically so hosts can swap in a persistent store; `ServiceCollectionExtensions.UsePluginRepository<T>()` exists for exactly that.

### Middleware (`Middleware/`)

A small pipeline modeled on ASP.NET Core middleware: `IPluginMiddleware.InvokeAsync(PluginOperationContext, PluginOperationDelegate next)` composed by `PluginMiddlewarePipeline`. Shipped middleware: `LoggingMiddleware`, `CachingMiddleware`, `ErrorHandlingMiddleware`, `RateLimitMiddleware`. The context object carries operation type, the plugin, timing, exception and a metadata bag. Trade-off: it is a separate mini-pipeline rather than reusing ASP.NET Core's, because the engine must work in non-web hosts (CLI, workers).

### Events (`Events/`)

`PluginEventPublisher` / `PluginEventSubscriber` - an in-process pub/sub keyed by event type (`Dictionary<Type, List<Delegate>>` under a lock). Events implement `IPluginEvent`. This is deliberately not a message bus: subscribers run in-process and failures are logged, not retried. Good enough for lifecycle notifications (plugin loaded/failed/health degraded); anything cross-process should be bridged via `Integration/WebhookHandler`.

### Caching (`Caching/`)

`IPluginCache` with a `MemoryPluginCache` implementation on top of `IMemoryCache`, plus its own key tracking (`ConcurrentDictionary<string, byte>`) and hit/miss counters - `IMemoryCache` does not expose key enumeration, hence the extra bookkeeping.

### Integration (`Integration/`)

`HttpPluginClient`, `IRemotePluginRegistry`/`RemotePluginRegistry`, `WebhookHandler` - the outbound edge: talking to remote plugin registries over HTTP and pushing webhook notifications.

### Marketplace (`Marketplace/`)

`PluginMarketplaceService` and `MarketplaceBrowserService` sit on top of `IRemotePluginRegistry`: search, listing retrieval, and a version-compatibility matrix cached for 30 minutes. Registered separately via `AddPluginMarketplace()` because most hosts do not need it.

### Hosting extras

- `BackgroundServices/` - `PluginHealthCheckService` and `BackgroundPluginMonitor`, standard `BackgroundService` implementations that periodically inspect loaded plugins and publish events.
- `Cli/` - `PluginEngineCliHost` + `CommandParser`, an interactive command-line front end over the same services.
- `Controllers/HealthController` - a minimal health endpoint for web hosts.
- `Formatters/` - `JsonPluginFormatter`, `CsvPluginFormatter`, `XmlPluginFormatter` behind `IPluginFormatter` with a `FormatterFactory`.
- `Results/PluginOperationResult` - result-object pattern (`PluginOperationResult<T>`) used at service boundaries instead of throwing for expected failures; exceptions (`Exceptions/`) are reserved for genuinely exceptional states (load failure, version mismatch, circular dependency).

## Dependency injection

Two entry points, layered:

- `AddPluginEngine(options)` (`Configuration/DependencyInjectionSetup.cs`) - the core: options, repository, the six core services, and the `PluginEngine` facade. Options are validated eagerly at registration time (`options.IsValid()`), so a misconfigured host fails at startup, not on first plugin load.
- `AddPluginEngineStack(options)` (`Utils/Extensions/ServiceCollectionExtensions.cs`) - core plus middleware pipeline, memory cache, event publisher/subscriber, HTTP client + remote registry, validators/helpers, formatters.
- `AddPluginMarketplace()` - marketplace services; requires the stack (needs `IRemotePluginRegistry`).

Everything is a singleton by design: the engine owns process-wide state (load contexts, plugin registry), so scoped/transient lifetimes would just create split-brain state.

## Data flow

Typical load path:

```
Host calls PluginEngine.LoadAllPluginsAsync()
  -> PluginLoaderService.LoadPluginsFromDirectoryAsync(options.PluginDirectory)
       -> discovery: enumerate candidate assemblies
       -> per plugin: new PluginAssemblyLoadContext (collectible) + AssemblyDependencyResolver
       -> reflection: find IPluginLifecycle implementations, run initialization
       -> DependencyResolutionService: order/validate dependencies, detect cycles
       -> VersioningService: compatibility check against host/engine version
       -> PluginRepository: record the Plugin entity
       -> PluginEventPublisher: publish loaded/failed events
```

Hot reload is the same path triggered by the file watcher in `HotReloadService`: unload (dispose lifecycles, `AssemblyLoadContext.Unload()`), then reload from disk. Unloading is cooperative - a plugin that pins its assemblies (static caches, threads) will keep the context alive until the GC can collect it; this is a known CLR constraint, not something the engine can force.

## Key design decisions

1. **One collectible `AssemblyLoadContext` per plugin.** Gives true isolation (two plugins can carry conflicting dependency versions) and unloadability. Cost: cross-plugin type sharing must go through contracts loaded in the default context, and unload is only best-effort (see above).
2. **`AssemblyDependencyResolver` instead of custom probing.** Uses each plugin's `.deps.json`, which handles native dependencies and RID-specific assets correctly. Cost: plugins must be published with their deps file.
3. **Facade + interface-per-service.** Slightly more ceremony, but every service is mockable and replaceable through DI, and the test project exercises them in isolation.
4. **Result objects at the API surface, exceptions internally.** Callers of marketplace/manager APIs get `PluginOperationResult<T>` and don't need try/catch for expected failures.
5. **In-process events, not a bus.** Zero infrastructure requirements; webhook integration exists for the cases that need to leave the process.

## Extension points

- `IPluginLifecycle` - what a plugin implements to hook init/shutdown.
- `IPluginMiddleware` + `AddPluginMiddleware<T>()` / `ConfigurePluginPipeline(...)` - intercept plugin operations.
- `IPluginRepository` + `UsePluginRepository<T>()` - persistent plugin registry.
- `IPluginEventHandler<TEvent>` + `AddPluginEventHandler<TEvent, THandler>()` - react to engine events.
- `IPluginFormatter` - additional metadata formats.
- `IRemotePluginRegistry` - alternative marketplace backends.

## Known limitations

- Unloading depends on the plugin not leaking references; a misbehaving plugin can keep its load context alive indefinitely.
- `PluginRepository` is in-memory - engine state does not survive a restart unless the host swaps in its own repository.
- The event system is fire-and-forget in-process; no ordering or delivery guarantees across handlers.
- Rate limiting and caching middleware operate per-process; nothing is coordinated across multiple host instances.
- `PluginLoaderService` serializes bookkeeping through a single lock, so very large plugin sets will contend on it. `PluginEngineOptions.MaxConcurrentPluginLoads` is validated but not yet enforced by the loader - it is a declared option waiting for its implementation.
