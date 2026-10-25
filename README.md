# dotnet-plugin-engine

The `IPluginEventPublisher` interface provides a set of extension methods for event publishing, subscription management, and diagnostics.

## PluginIncompatibleExceptionExtensionsTests

The `PluginIncompatibleExceptionExtensionsTests` class contains unit tests for the extension methods that operate on `PluginIncompatibleException`. These tests verify that the extension methods correctly determine version incompatibility, format incompatibility reasons, and handle null or empty values.

## PluginExceptionTests

The `PluginExceptionTests` class contains unit tests for the `PluginException` class, verifying its constructors, property getters, fluent modification methods, and string representation. It ensures that error codes, entity IDs, and contextual metadata are correctly stored and that appropriate exceptions are thrown for invalid inputs.

## PluginLoadExceptionExtensionsTests

The `PluginLoadExceptionExtensionsTests` class contains unit tests for the extension methods that operate on `PluginLoadException`. These tests verify that the extension methods correctly check the load stage, generate a formatted failure summary, and create a new exception with an updated load stage while preserving other properties.

Example usage:
```csharp
var ex = new PluginLoadException("Failed to load", "MyPlugin", "path/to/plugin.dll", PluginLoadStage.AssemblyResolution);
bool isAtResolution = ex.IsLoadStage(PluginLoadStage.AssemblyResolution); // returns true
string summary = ex.GetLoadFailureSummary(); // returns "Failed to load plugin 'MyPlugin' from 'path/to/plugin.dll' at stage AssemblyResolution."
var updatedEx = ex.WithLoadStage(PluginLoadStage.TypeLoading); // returns a new exception with LoadStage set to TypeLoading
```

## DependencyResolutionExceptionValidationTests

The `DependencyResolutionExceptionValidationTests` class verifies the validation logic for `DependencyResolutionException` objects. It ensures that the exception contains valid version constraints, reasons, and a list of unresolved dependencies, and that the validation methods correctly report problems or confirm validity.

Example usage:
```csharp
using PluginEngine.Exceptions;
using PluginEngine.Validation;

var ex = new DependencyResolutionException(
    "Missing dependency",
    "MyPlugin",
    new[] { "DependencyA", "DependencyB" });

var problems = DependencyResolutionExceptionValidator.Validate(ex);
if (problems.Any())
{
    foreach (var p in problems)
    {
        Console.WriteLine(p);
    }
}
else
{
    Console.WriteLine("Exception is valid.");
}
```

## VersionMismatchExceptionTests

The `VersionMismatchExceptionTests` class contains unit tests for the `VersionMismatchException` class, verifying its constructors and the `ToString` method under various conditions.

Example usage:
```csharp
using PluginEngine.Exceptions;

// Create an exception with version details
var ex = new VersionMismatchException(
    "Version mismatch detected",
    "1.0.0",
    "2.0.0",
    "Plugin",
    "MyPlugin");

// The ToString method includes the version and component information
string errorMessage = ex.ToString();
```

## VersionMismatchExceptionExtensionsTests

The `VersionMismatchExceptionExtensionsTests` class contains unit tests for the extension methods that operate on `VersionMismatchException`. These tests verify that the extension methods correctly format error messages, determine critical version mismatches based on version numbers, add contextual information, and produce simplified messages.

Example usage:
```csharp
using PluginEngine.Exceptions;

var ex = new VersionMismatchException(
    "Version mismatch detected",
    "1.0.0",
    "2.0.0",
    "Plugin",
    "MyPlugin");

bool isCritical = ex.IsCriticalVersionMismatch();
string formattedMessage = ex.GetFormattedErrorMessage();
string simplifiedMessage = ex.GetSimplifiedMessage();
var exWithContext = ex.WithContext("operation", "plugin loading");
```

## Middleware Pipeline

Create a `PluginMiddlewarePipeline` and register caching and rate limiting before building the operation delegate:

```csharp
using PluginEngine.Middleware;

PluginOperationDelegate middleware = new PluginMiddlewarePipeline()
    .UseCaching(cacheDuration: TimeSpan.FromMinutes(10))
    .UseRateLimit(maxTokensPerSecond: 50, windowSizeSeconds: 1)
    .Build();

await middleware(context);
```

## Caching

The engine has two caching layers. `IPluginCache` (namespace `PluginEngine.Caching`, `src/PluginEngine/Caching/IPluginCache.cs`) is a general key-value cache that application code can use directly. `CachingMiddleware` (namespace `PluginEngine.Middleware`, `src/PluginEngine/Middleware/CachingMiddleware.cs`) caches the results of selected plugin operations inside the pipeline. The two do not share storage: the middleware keeps its own `IMemoryCache`.

### IPluginCache

`IPluginCache` has five async operations:

| Method | Description |
|--------|-------------|
| `Task<T?> GetAsync<T>(string key)` | Returns the cached value, or `default` on a miss. Each call counts as a hit or a miss. |
| `Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)` | Stores a value. The sliding expiration is always 15 minutes. `expiration` sets the absolute lifetime; when omitted it is 1 hour. |
| `Task RemoveAsync(string key)` | Removes one entry. |
| `Task ClearAsync()` | Removes every entry that the cache instance stored and resets the hit and miss counters. |
| `Task<CacheStatistics> GetStatisticsAsync()` | Returns a snapshot of the current statistics. |

`MemoryPluginCache` is the built-in implementation. It wraps `IMemoryCache`, and it is registered as the singleton `IPluginCache` by `AddPluginEngineStack`. Keys must not be null or whitespace. Errors from the backing cache are logged and not thrown: a failed `GetAsync` returns `default`, and a failed `SetAsync` or `RemoveAsync` does nothing.

```csharp
using PluginEngine.Caching;

// Resolved from DI (registered by AddPluginEngineStack)
IPluginCache cache = serviceProvider.GetRequiredService<IPluginCache>();

// Uses the default 15-minute sliding expiration
await cache.SetAsync("plugins:names", new[] { "alpha", "beta" });

// Adds a 2-minute absolute lifetime on top of the sliding expiration
await cache.SetAsync("plugins:short", "value", TimeSpan.FromMinutes(2));

var names = await cache.GetAsync<string[]>("plugins:names");
await cache.RemoveAsync("plugins:short");
await cache.ClearAsync();
```

### CacheStatistics

`CacheStatistics` (defined in `IPluginCache.cs`) has these properties:

| Property | Description |
|----------|-------------|
| `TotalHits` | Number of `GetAsync` calls that found a value. |
| `TotalMisses` | Number of `GetAsync` calls that did not find a value. |
| `CurrentEntries` | Number of keys currently tracked by the cache. |
| `TotalMemoryBytes` | Process-wide managed memory from `GC.GetTotalMemory(false)`. It is not the size of the cache alone. |
| `HitRate` | `TotalHits / (TotalHits + TotalMisses)`, or `0` when there have been no requests. |

`CacheStatisticsExtensions` (`src/PluginEngine/Caching/CacheStatisticsExtensions.cs`) adds helpers:

- `IsEffective(minHitRate = 0.5)` returns `true` when `HitRate` meets the threshold.
- `GetTotalRequests()` returns `TotalHits + TotalMisses`.
- `ToSummaryString()` returns a one-line text summary.

```csharp
var stats = await cache.GetStatisticsAsync();

Console.WriteLine(stats.ToSummaryString());
Console.WriteLine($"Hit rate: {stats.HitRate:P1}, effective: {stats.IsEffective()}");
```

### CachingMiddleware

`CachingMiddleware` caches successful results of three operations: `GetMetadata`, `ResolveDependencies`, and `ValidateVersion`. Other operations always run. Each entry uses a key built from the plugin ID, the operation type, and the metadata keys. The default lifetime is 5 minutes absolute and 1 minute sliding. A cache hit sets `context.Metadata["cached"] = true` and marks the context successful without calling the next middleware. A failed operation is not cached.

Use `UseCaching` to add it to a pipeline. The `cacheDuration` argument sets the absolute lifetime.

```csharp
using PluginEngine.Middleware;

PluginOperationDelegate middleware = new PluginMiddlewarePipeline()
    .UseCaching(cacheDuration: TimeSpan.FromMinutes(10))
    .Build();
```

To drop the cached results for one plugin, for example after it is reloaded, call `InvalidatePluginCache(pluginId)` on a `CachingMiddleware` instance. `UseCaching` creates its own instance internally and does not expose it, so keep a reference to the instance you construct if you need to invalidate.

## Hot Swapping Plugins

`IHotSwapService` replaces a running plugin assembly while keeping the host application available. A plugin must be loaded or active and its assembly file must be accessible for `CanSwap` to return `true`.

- `SwapPluginAsync` unloads the current plugin, loads the replacement assembly in a new assembly load context, and invokes the registered post-swap callback. If the swap fails, the service makes a best-effort attempt to reload the previous assembly.
- `RollbackSwapAsync` restores the assembly used immediately before the most recent successful swap.
- `GetSwapHistoryAsync` returns the plugin's complete swap history in oldest-first order.
- `RegisterPostSwapCallback` registers one callback for a plugin. Registering another callback for the same plugin replaces the existing callback.
- `CanSwap` checks whether a plugin is currently eligible for hot swapping.

Example usage:

```csharp
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

async Task UpdatePluginAsync(
    IHotSwapService hotSwapService,
    Plugin plugin,
    string replacementAssemblyPath,
    CancellationToken cancellationToken)
{
    if (!hotSwapService.CanSwap(plugin))
    {
        throw new InvalidOperationException("The plugin cannot be hot-swapped.");
    }

    hotSwapService.RegisterPostSwapCallback(
        plugin.Id,
        swappedPlugin =>
        {
            Console.WriteLine($"Swapped {swappedPlugin.Name}");
            return Task.CompletedTask;
        });

    var swapResult = await hotSwapService.SwapPluginAsync(
        plugin.Id,
        replacementAssemblyPath,
        cancellationToken);

    if (!swapResult.Success)
    {
        return;
    }

    var historyResult = await hotSwapService.GetSwapHistoryAsync(
        plugin.Id,
        cancellationToken);

    if (historyResult.Success && historyResult.Data is not null)
    {
        foreach (var record in historyResult.Data)
        {
            Console.WriteLine($"{record.SwappedAtUtc:u}: {record.PreviousAssemblyPath} -> {record.NewAssemblyPath}");
        }
    }

    // Restore the assembly that was active before the successful swap if needed.
    var rollbackResult = await hotSwapService.RollbackSwapAsync(
        plugin.Id,
        cancellationToken);

    Console.WriteLine(rollbackResult.Message);
}
```

## Hot Reload

`IHotReloadService` reloads a loaded plugin on request and keeps a record of each attempt. Unlike Hot Swapping, which replaces a plugin with a specific assembly file, hot reload reloads the plugin through `IPluginLoaderService` and reports the outcome through callbacks, statistics, and per-plugin status.

- `StartHotReloadMonitoringAsync` starts monitoring the application base directory for `*.dll` changes. Calling it while monitoring is already active has no effect. `StopHotReloadMonitoringAsync` stops monitoring and releases the file watcher. The file-change handler does not reload plugins by itself, so call `HotReloadPluginAsync` to perform a reload.
- `CanHotReload` returns `true` only when the plugin has `SupportsHotReload` set and its status is `Loaded`. It returns `false` for a `null` plugin.
- `HotReloadPluginAsync` reloads the plugin with the given identifier and returns `true` on success. It returns `false` when the plugin is not loaded, cannot be hot reloaded, or the reload throws. Failures are recorded as events and do not propagate as exceptions.
- `RegisterHotReloadCallback` registers one callback per plugin identifier. The callback receives the reloaded `Plugin` and runs only after a successful reload. Registering another callback for the same plugin replaces the existing one. `UnregisterHotReloadCallback` removes it, and `RemoveCallbacksForContext` removes every callback whose delegate was defined in the given `AssemblyLoadContext`.
- `GetStatisticsAsync` returns a `HotReloadStatistics` snapshot. It covers the most recent 100 reload attempts. `RecentEvents` holds the last 10 of them.
- `GetHotReloadStatusAsync` returns the `HotReloadStatus` for a plugin, or `null` if no reload has been attempted for it.

### Types

- `HotReloadStatistics` contains `TotalReloads`, `SuccessfulReloads`, `FailedReloads`, `LastReloadTime`, `AverageReloadTime`, and `RecentEvents`.
- `HotReloadEvent` describes one attempt: `PluginId`, `Timestamp` (UTC), `Success`, `ErrorMessage` (set on failure), and `Duration`.
- `HotReloadStatus` describes a plugin's reload state: `PluginId`, `SupportsHotReload`, `LastReloadTime`, `ReloadCount`, and `LastError`.

Example usage:

```csharp
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

async Task ReloadPluginAsync(
    IHotReloadService hotReloadService,
    Plugin plugin,
    CancellationToken cancellationToken)
{
    hotReloadService.RegisterHotReloadCallback(
        plugin.Id,
        reloadedPlugin =>
        {
            Console.WriteLine($"Reloaded {reloadedPlugin.Name}");
            return Task.CompletedTask;
        });

    await hotReloadService.StartHotReloadMonitoringAsync(cancellationToken);

    if (!hotReloadService.CanHotReload(plugin))
    {
        return;
    }

    var reloaded = await hotReloadService.HotReloadPluginAsync(plugin.Id, cancellationToken);
    if (!reloaded)
    {
        var status = await hotReloadService.GetHotReloadStatusAsync(plugin.Id, cancellationToken);
        Console.WriteLine($"Reload failed: {status?.LastError ?? "plugin was not reloaded"}");
    }

    var statistics = await hotReloadService.GetStatisticsAsync();
    Console.WriteLine(statistics);
}
```

## PluginOperationResult

`PluginOperationResult` represents an operation without return data, while `PluginOperationResult<T>` adds a nullable `Data` value. Both expose `Success`, `Message`, `ErrorCode`, `ErrorDetails`, `DurationMs`, and `TimestampUtc`.

- `CreateSuccess` creates a successful result and clears error information. The generic overload accepts the returned data.
- `CreateFailure` requires a non-empty message and uses error code `500` by default. The optional `details` and `durationMs` arguments populate `ErrorDetails` and `DurationMs`.
- `FromException` uses the exception message, the inner exception message as error details, and maps `PluginLoadException` to `1001`, `DependencyResolutionException` to `1002`, `VersionMismatchException` to `1003`, and all other exceptions to `500`.

`PluginBatchOperationResult` collects per-plugin results through `AddResult`, updates `SuccessCount` and `FailureCount`, and exposes `TotalCount`, `TotalDurationMs`, and `GetSummary()`. `IsSuccessful` is `true` when there are no failures or when successes outnumber failures.

```csharp
using PluginEngine.Results;

var loaded = PluginOperationResult<string>.CreateSuccess(
    "MyPlugin",
    "Plugin loaded",
    durationMs: 18);

var failed = PluginOperationResult.CreateFailure(
    "Plugin configuration is invalid",
    errorCode: 400,
    details: "The entry point is missing.");

var batch = new PluginBatchOperationResult();
batch.AddResult(Guid.NewGuid(), "MyPlugin", loaded);
batch.AddResult(Guid.NewGuid(), "OtherPlugin", failed);

Console.WriteLine(batch.GetSummary());
```

## Getting Started

Register the plugin engine with an `IServiceCollection`, configure it through `PluginEngineOptions`, and resolve the `PluginEngine` facade from the service provider:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;
using PluginEngineFacade = PluginEngine.PluginEngine;

var services = new ServiceCollection();

DependencyInjectionSetup.AddPluginEngine(
    services,
    (PluginEngineOptions options) =>
    {
        options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
        options.EnableHotReload = true;
        options.MaxConcurrentPluginLoads = 4;
    });

using var serviceProvider = services.BuildServiceProvider();
var pluginEngine = serviceProvider.GetRequiredService<PluginEngineFacade>();

try
{
    await pluginEngine.InitializeAsync();

    var loadedPluginCount = await pluginEngine.LoadAllPluginsAsync();
    Console.WriteLine($"Loaded {loadedPluginCount} plugin(s).");

    var healthInfo = await pluginEngine.GetHealthInfoAsync();
    Console.WriteLine(healthInfo);
}
finally
{
    await pluginEngine.ShutdownAsync();
}
```

## Configuration

`PluginEngineOptions` (`src/PluginEngine/Configuration/PluginEngineOptions.cs`) holds every engine setting. Pass it through the `configureOptions` callback of `AddPluginEngine`, or set the properties directly. `IsValid()` and `GetValidationErrors()` check the options, and `AddPluginEngine` throws `InvalidOperationException` when a value is invalid.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PluginDirectory` | `string` | `"plugins"` | Plugin directory path. Must not be empty. |
| `EnableHotReload` | `bool` | `true` | Enables hot reload. When `false`, the background plugin monitor does not run. |
| `HotReloadCheckIntervalMs` | `int` | `5000` | Hot reload check interval in milliseconds. Must be greater than 0. |
| `EnableDependencyCaching` | `bool` | `true` | Enables dependency caching. |
| `OperationTimeoutMs` | `int` | `30000` | Operation timeout in milliseconds. Must be greater than 0. |
| `EnableLogging` | `bool` | `true` | Enables logging. |
| `LogLevel` | `LogLevel` | `Information` | Log level: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, or `None`. |
| `MaxConcurrentPluginLoads` | `int` | `4` | Maximum concurrent plugin loads. Must be greater than 0. |
| `DependencyCacheTtlMinutes` | `int` | `60` | Dependency cache TTL in minutes. Must be greater than 0. |
| `TargetFramework` | `string` | `"net10.0"` | Target framework. |
| `StrictVersionChecking` | `bool` | `true` | Enables strict version checking. |
| `EnableCircularDependencyDetection` | `bool` | `true` | Enables circular dependency detection. |
| `MaxDependencyResolutionAttempts` | `int` | `10` | Maximum dependency resolution attempts. Must be greater than 0. |

### appsettings.json

The keys below come from `ConfigurationKeys` in `src/PluginEngine/Constants/PluginEngineConstants.cs`. They are nested under the `ConfigurationKeys.SectionName` section, which is `PluginEngine`:

```json
{
  "PluginEngine": {
    "PluginDirectory": "plugins",
    "EnableHotReload": true,
    "HotReloadCheckInterval": 5000,
    "EnableDependencyCaching": true,
    "OperationTimeout": 30000,
    "EnableLogging": true,
    "LogLevel": "Information",
    "MaxConcurrentPluginLoads": 4
  }
}
```

The engine does not read `appsettings.json` on its own. The host has to copy the values into `PluginEngineOptions`. Two keys use different names from their properties: `HotReloadCheckInterval` maps to `HotReloadCheckIntervalMs`, and `OperationTimeout` maps to `OperationTimeoutMs`. The other properties in the table have no `ConfigurationKeys` constant, so they keep their defaults unless the host sets them in code.

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;
using PluginEngine.Constants;

var section = configuration.GetSection(ConfigurationKeys.SectionName);

DependencyInjectionSetup.AddPluginEngine(services, options =>
{
    options.PluginDirectory = section.GetValue(ConfigurationKeys.PluginDirectory, options.PluginDirectory)!;
    options.EnableHotReload = section.GetValue(ConfigurationKeys.EnableHotReload, options.EnableHotReload);
    options.HotReloadCheckIntervalMs = section.GetValue(ConfigurationKeys.HotReloadCheckInterval, options.HotReloadCheckIntervalMs);
    options.EnableDependencyCaching = section.GetValue(ConfigurationKeys.EnableDependencyCaching, options.EnableDependencyCaching);
    options.OperationTimeoutMs = section.GetValue(ConfigurationKeys.OperationTimeout, options.OperationTimeoutMs);
    options.EnableLogging = section.GetValue(ConfigurationKeys.EnableLogging, options.EnableLogging);
    options.LogLevel = Enum.TryParse<LogLevel>(section[ConfigurationKeys.LogLevel], out var logLevel) ? logLevel : options.LogLevel;
    options.MaxConcurrentPluginLoads = section.GetValue(ConfigurationKeys.MaxConcurrentPluginLoads, options.MaxConcurrentPluginLoads);
});
```

`GetValue` comes from `Microsoft.Extensions.Configuration.Binder`. The host adds that package, and the engine does not depend on it.

## Output Formatters

Plugin data can be rendered as text through the `IPluginFormatter` contract in `src/PluginEngine/Formatters/`. Every formatter returns a `Task<string>` from four methods:

- `FormatPluginAsync` formats a single `Plugin`.
- `FormatPluginsAsync` formats a collection of plugins.
- `FormatDetailedReportAsync` formats a plugin with its metadata, dependencies, and capabilities.
- `FormatHealthReportAsync` formats a `PluginHealthInfo` snapshot, including health state and issues.

The `FormatType` property identifies the format. Three implementations ship with the library:

- `JsonPluginFormatter` (`"json"`) writes indented JSON for API responses and file storage.
- `CsvPluginFormatter` (`"csv"`) writes comma-separated rows for spreadsheets and data analysis.
- `XmlPluginFormatter` (`"xml"`) writes indented XML for configuration files and system integration.

`FormatterFactory` maps a format name to a formatter. `GetFormatter` matches the name case-insensitively and returns `null` for unknown formats. `GetSupportedFormats` lists the registered names. The factory is constructed with one instance of each built-in formatter.

`PluginHealthInfoExtensions` adds `HasIssues`, `GetIssueSummary`, and `IsStale` to `PluginHealthInfo`.

Example usage:

```csharp
using PluginEngine.Formatters;

var factory = new FormatterFactory(
    new JsonPluginFormatter(),
    new CsvPluginFormatter(),
    new XmlPluginFormatter());

IPluginFormatter? formatter = factory.GetFormatter("JSON");
if (formatter is null)
{
    throw new NotSupportedException("Unknown output format.");
}

string pluginJson = await formatter.FormatPluginAsync(plugin);

string healthXml = await factory
    .GetFormatter("xml")!
    .FormatHealthReportAsync(healthInfo);

string pluginsCsv = await factory
    .GetFormatter("csv")!
    .FormatPluginsAsync(plugins);
```

For the full format reference, see [docs/formatters-guide.md](docs/formatters-guide.md).

## Plugin Events

Lifecycle changes are published in-process through `PluginEventPublisher` and consumed through `PluginEventSubscriber`. The publisher keeps handlers in a dictionary keyed by event type, so a subscriber only receives the event types it registered for. Delivery is in-process and not retried. If a handler throws, the other handlers still run and the failures are reported as an `AggregateException` from `PublishAsync`. This is a notification mechanism, not a message bus. Use the webhook integration for anything that must leave the process.

The event types are defined in `src/PluginEngine/Events/IPluginEvent.cs`. They all derive from `PluginEventBase`, which supplies `EventId`, `OccurredAtUtc`, and the required `PluginId`:

| Event | `EventType` | Event-specific properties |
| --- | --- | --- |
| `PluginLoadedEvent` | `PluginLoaded` | `PluginName`, `Version`, `LoadTimeMs` |
| `PluginUnloadedEvent` | `PluginUnloaded` | `PluginName`, `Reason` |
| `PluginUpdatedEvent` | `PluginUpdated` | `PreviousVersion`, `NewVersion`, `ChangesSummary` |
| `PluginErrorEvent` | `PluginError` | `ErrorMessage`, `ErrorDetails`, `ErrorCode` |
| `DependenciesResolvedEvent` | `DependenciesResolved` | `ResolvedDependencies`, `ResolutionTimeMs` |

Subscribe with a `Func<TEvent, Task>` handler, then publish an event:

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using PluginEngine.Events;

var publisher = new PluginEventPublisher(NullLogger<PluginEventPublisher>.Instance);
var subscriber = new PluginEventSubscriber(publisher, NullLogger<PluginEventSubscriber>.Instance);

subscriber.OnPluginLoaded(async e =>
{
    Console.WriteLine($"{e.PluginName} {e.Version} loaded in {e.LoadTimeMs} ms");
    await Task.CompletedTask;
});

subscriber.Subscribe<PluginErrorEvent>(async e =>
{
    Console.WriteLine($"Error {e.ErrorCode}: {e.ErrorMessage}");
    await Task.CompletedTask;
});

await publisher.PublishAsync(new PluginLoadedEvent
{
    PluginId = Guid.NewGuid(),
    PluginName = "ReportExporter",
    Version = "1.2.0",
    LoadTimeMs = 42
});
```

`PluginEventPublisherExtensions` adds helpers to `PluginEventPublisher`:

- `SubscribeOnce<T>` registers a handler that unsubscribes itself after the first event. It returns an `IDisposable`. Disposing it earlier removes the handler without waiting for an event. Overloads accept `Func<T, Task>` or `Action<T>`.
- `PublishBatchAsync<T>` publishes a sequence of events concurrently and awaits them all. Publisher re-entrancy protection works per event type and stays active until every handler for the in-flight event has finished. A batch with several events of the same type can therefore skip the later ones while the first is still being handled. The skipped events are logged as a warning and not delivered. Use `PublishBatchAsync` for mixed event types, or publish same-type events one at a time with `await`.

For the full event reference, see [docs/events-guide.md](docs/events-guide.md).

## Dependency Injection

Extension methods register the engine with `Microsoft.Extensions.DependencyInjection`. Every method returns the same `IServiceCollection`, so calls can be chained. All engine services are registered as singletons.

- `DependencyInjectionSetup` (namespace `PluginEngine.Configuration`, `src/PluginEngine/Configuration/DependencyInjectionSetup.cs`) registers the core engine.
- `ServiceCollectionExtensions` (namespace `PluginEngine.Utils.Extensions`, `src/PluginEngine/Utils/Extensions/ServiceCollectionExtensions.cs`) adds optional middleware, event, and repository registrations.

| Method | Description |
|--------|-------------|
| `DependencyInjectionSetup.AddPluginEngine(services, configureOptions)` | Registers the core engine services and the `PluginEngine` facade. `configureOptions` is optional. Throws `InvalidOperationException` when the options are invalid. |
| `DependencyInjectionSetup.AddPluginEngine(services)` | Same as above with default `PluginEngineOptions`. |
| `AddPluginEngineStack(services, configureOptions)` | Calls `AddPluginEngine`, then adds the middleware pipeline, memory cache, event publisher and subscriber, HTTP clients, utilities, and formatters. |
| `AddPluginMiddleware<TMiddleware>(services)` | Registers `TMiddleware` as a singleton `IPluginMiddleware`. Call it several times to register several middleware. |
| `ConfigurePluginPipeline(services, configure)` | Registers a `PluginMiddlewarePipeline` built by running `configure` once. It replaces the pipeline from `AddPluginEngineStack`, so call it after that method. |
| `AddPluginEventHandler<TEvent, THandler>(services)` | Registers `THandler` as a singleton `IPluginEventHandler<TEvent>`. |
| `UsePluginRepository<TRepository>(services)` | Registers `TRepository` as the singleton `IPluginRepository`. It replaces the default in-memory `PluginRepository`, so call it after `AddPluginEngine` or `AddPluginEngineStack`. |

Registration example:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;
using PluginEngine.Middleware;
using PluginEngine.Utils.Extensions;
using PluginEngineFacade = PluginEngine.PluginEngine;

var services = new ServiceCollection();

services
    .AddPluginEngineStack(options =>
    {
        options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
        options.EnableHotReload = false;
    })
    .AddPluginMiddleware<AuditMiddleware>()                           // your IPluginMiddleware
    .ConfigurePluginPipeline(pipeline => pipeline.UseLogging())       // replaces the stack's pipeline
    .AddPluginEventHandler<PluginLoadedEvent, PluginLoadedAuditHandler>() // your IPluginEventHandler<PluginLoadedEvent>
    .UsePluginRepository<SqlPluginRepository>();                      // your IPluginRepository

using var serviceProvider = services.BuildServiceProvider();
var pluginEngine = serviceProvider.GetRequiredService<PluginEngineFacade>();
```

`AuditMiddleware`, `PluginLoadedEvent`, `PluginLoadedAuditHandler`, and `SqlPluginRepository` are placeholders for your own types. The example is not part of the build.

Plain core registration needs only `DependencyInjectionSetup.AddPluginEngine` (see [Getting Started](#getting-started)).

## Dependency Resolution

Dependency resolution has three parts. The two services are registered as singletons by `DependencyInjectionSetup.AddPluginEngine`. `DependencyGraphAnalyzer` is not registered, so create it yourself.

| Type | Namespace | Role |
|------|-----------|------|
| `IDependencyResolutionService` | `PluginEngine.Services.Abstractions` | Per-plugin resolution. Implemented by `DependencyResolutionService`. |
| `IPluginDependencyResolver` | `PluginEngine.Services.Abstractions` | Set-based install order, conflict detection, and resolution plans. Implemented by `PluginDependencyResolver`. |
| `DependencyGraphAnalyzer` | `PluginEngine.Utils.Helpers` | Text graph visualization and complexity analysis, built on `IDependencyResolutionService`. |

### IDependencyResolutionService

Works on one plugin at a time. Its methods are `ResolveDependenciesAsync`, `ValidateDependenciesAsync`, `HasCircularDependenciesAsync`, `GetDependencyGraphAsync(Guid)`, `ResolveSingleDependencyAsync`, `GetDependentsAsync(Guid)`, and `ClearDependencyCacheAsync`. The graph it returns is a `DependencyGraph` with `Nodes` (`DependencyNode`) and `Edges` (`DependencyEdge`).

### IPluginDependencyResolver

Works on a group of plugins. All three methods return `PluginOperationResult<T>`. Check `Success` before you read `Data`.

| Method | Returns | Description |
|--------|---------|-------------|
| `GetInstallOrderAsync(IEnumerable<Plugin>, CancellationToken)` | `PluginOperationResult<List<Plugin>>` | Topological order. Each plugin comes after its dependencies. |
| `FindConflictsAsync(IEnumerable<Plugin>, CancellationToken)` | `PluginOperationResult<List<DependencyConflict>>` | Finds plugins that declare incompatible version constraints on the same dependency. Each conflict lists `ConflictingRequirements`. |
| `BuildResolutionPlanAsync(Guid, CancellationToken)` | `PluginOperationResult<DependencyResolutionPlan>` | Plan for one root plugin and its transitive dependencies. |

A `DependencyResolutionPlan` has `Steps` (`ResolutionStep` items with `Order`, `PluginName`, `Version`, `IsOptional`, and `Action`), `Conflicts`, and `IsExecutable`. `IsExecutable` is `true` only when `Conflicts` is empty. `ResolutionAction` has four values: `Install`, `AlreadySatisfied`, `Upgrade`, and `ManualResolutionRequired`.

`GetInstallOrderAsync` and `BuildResolutionPlanAsync` report failures in the result, for example a circular dependency. A `null` plugin collection passed to `GetInstallOrderAsync` or `FindConflictsAsync` throws `ArgumentNullException`.

```csharp
var resolver = serviceProvider.GetRequiredService<IPluginDependencyResolver>();

var plan = await resolver.BuildResolutionPlanAsync(rootPluginId);
if (plan.Success && plan.Data is { IsExecutable: true } resolved)
{
    foreach (var step in resolved.Steps)
    {
        Console.WriteLine(step); // e.g. "1. Install Logging v2.1.0"
    }
}
```

### DependencyGraphAnalyzer

Constructor: `DependencyGraphAnalyzer(IDependencyResolutionService, ILogger<DependencyGraphAnalyzer>)`.

- `AnalyzeAsync(Plugin)` returns a `DependencyAnalysisReport`. It contains `DirectDependencies`, `TotalDependencies`, `HasCircularDependencies`, `ComplexityScore`, `AnalyzedAtUtc`, and `Issues`. `GetComplexityLevel()` maps the score to `Simple` (below 20), `Moderate` (below 50), `Complex` (below 75), or `Very Complex`. `Issues` flags more than 20 direct dependencies, circular dependencies, and more than 50% optional dependencies. On error, the method logs the exception and returns a report with a score of 0 and no issues.
- `GenerateGraphVisualizationAsync(Plugin)` returns an indented text tree of the plugin and its direct `Requires:` entries. It returns an empty string on error.

The complexity score is calculated from the plugin's direct and total dependencies:

- 10 points per direct dependency.
- 2 points per resolved dependency (direct and transitive), up to 100 points.
- 5 points per optional dependency.
- The total is capped at 100.

Circular dependencies do not add to the score. The `ComplexityScore` value is available on the report, but the formula is internal to the analyzer.

```csharp
var analyzer = new DependencyGraphAnalyzer(
    serviceProvider.GetRequiredService<IDependencyResolutionService>(),
    serviceProvider.GetRequiredService<ILogger<DependencyGraphAnalyzer>>());

var report = await analyzer.AnalyzeAsync(plugin);
Console.WriteLine(report);                  // includes Complexity Score and level
Console.WriteLine(report.GetComplexityLevel());

Console.WriteLine(await analyzer.GenerateGraphVisualizationAsync(plugin));
```

For the full reference, see [docs/dependency-resolution-guide.md](docs/dependency-resolution-guide.md).

## Marketplace and Remote Registry

The marketplace has three layers. Each layer calls only the one below it:

| Layer | Types | Namespace | Role |
|-------|-------|-----------|------|
| Browsing | `IMarketplaceBrowserService`, `MarketplaceBrowserService` | `PluginEngine.Marketplace` | Categories, trending, featured plugins, category browsing, home page snapshot |
| Marketplace | `IPluginMarketplaceService`, `PluginMarketplaceService` | `PluginEngine.Marketplace` | Search, listing details, compatibility matrix, install |
| Registry | `IRemotePluginRegistry`, `RemotePluginRegistry`, `IHttpPluginClient`, `HttpPluginClient` | `PluginEngine.Integration` | HTTP calls to the registry back-end, with in-memory caching |

Marketplace methods return `PluginOperationResult<T>`. Check `Success` first, then read `Data`, or `ErrorCode` and `Message` on failure. For how to package and publish a plugin, see the [Plugin Marketplace Publishing Guide](docs/marketplace-publishing-guide.md).

### Registration and Configuration

`AddPluginMarketplace` (`PluginEngine.Marketplace.MarketplaceExtensions`) registers `IPluginMarketplaceService` and `IMarketplaceBrowserService` as singletons, and adds the memory cache. It requires `IRemotePluginRegistry`, which `AddPluginEngineStack` registers. `AddPluginEngine` alone does not register it.

`AddPluginEngineStack` does not register `IHttpPluginClient`, so add it yourself or `IRemotePluginRegistry` cannot be resolved:

```csharp
services.AddPluginEngineStack()
        .AddPluginMarketplace();

// Required: RemotePluginRegistry depends on IHttpPluginClient
services.AddHttpClient<IHttpPluginClient, HttpPluginClient>();
```

`HttpPluginClient` reads the registry address from the `PluginRegistry:BaseUrl` configuration key and appends routes such as `/plugins/search`. Use a base URL without a trailing slash. When the key is missing, the client makes no requests and returns empty results.

```json
{
  "PluginRegistry": {
    "BaseUrl": "https://registry.example.com"
  }
}
```

### Searching with MarketplaceSearchFilter

`MarketplaceSearchFilter` carries the search criteria. Only `Query` and `PageSize` take effect today (see [Current Limitations](#current-limitations)):

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Marketplace;

var marketplace = serviceProvider.GetRequiredService<IPluginMarketplaceService>();

var filter = new MarketplaceSearchFilter
{
    Query = "logging",
    PageSize = 10
};

var result = await marketplace.SearchAsync(filter);

if (result.Success)
{
    foreach (var entry in result.Data!)
    {
        Console.WriteLine(entry); // "{Name} v{LatestVersion} by {Author} (... downloads, ... rating)"
    }
}
else
{
    Console.WriteLine($"Search failed ({result.ErrorCode}): {result.Message}");
}
```

### Browsing with IMarketplaceBrowserService

| Method | Returns | Behavior |
|--------|---------|----------|
| `GetCategoriesAsync()` | `List<MarketplaceCategory>` | Returns a built-in list of eight categories: analytics, authentication, caching, database, logging, messaging, security, and ui. The list is not fetched from the registry, and `PluginCount` is not populated. Cached for 6 hours. |
| `GetTrendingAsync(limit = 10)` | `List<MarketplaceEntry>` | Intended to return plugins ordered by downloads. `limit` is clamped to 1–50. Cached for 15 minutes. |
| `GetFeaturedAsync()` | `List<MarketplaceEntry>` | Intended to return up to six verified plugins ordered by rating. Cached for 1 hour. |
| `BrowseCategoryAsync(categoryId, filter = null)` | `List<MarketplaceEntry>` | Runs a search with the given filter. The category slug is added to `Tags`, which is not sent to the registry yet. Set `filter.Query`. |
| `GetHomePageAsync()` | `MarketplaceHomePage` | Combines categories, up to five trending plugins, and up to six featured plugins. Cached for 10 minutes. |

### Plugin Listings, Compatibility, and Install

`IPluginMarketplaceService` also provides:

- `GetEntryAsync(pluginId)` returns the listing with `AvailableVersions` populated. It fails with code `404` when the plugin is not found.
- `GetCompatibilityMatrixAsync(pluginId)` returns a `VersionCompatibilityMatrix` for engine versions 8.0, 9.0, and 10.0. It is cached for 30 minutes. Each stable version is rated as follows:
  - Released within 365 days: `Compatible`.
  - Released within 730 days: `Compatible` on engine 9.0 or later, otherwise `Deprecated`.
  - Older: `Deprecated` on engine 10.0 or later, otherwise `Incompatible`.
  - Prereleases are `Compatible` on engine 10.0 or later. Unstable versions are `Unknown`.
- `CheckCompatibilityAsync(pluginId, pluginVersion, engineVersion)` reads one pair from that matrix. It returns `Unknown` for pairs that were not recorded.
- `InstallAsync(pluginId, version, targetDirectory)` creates the directory if needed and downloads the package into it. It does not load the plugin into the engine.

### Remote Registry and HTTP Client

`IRemotePluginRegistry` wraps `IHttpPluginClient` and caches its results in `IMemoryCache`:

| Method | Behavior |
|--------|----------|
| `SearchAsync(query, limit = 20)` | Searches the registry. Results are cached for 1 hour. Rejects a null or whitespace query. |
| `GetPluginAsync(pluginId)` | Returns plugin metadata, or `null` when it is not found. Cached for 1 hour. |
| `GetVersionsAsync(pluginId)` | Returns a list of `PluginVersionInfo`. |
| `DownloadPluginAsync(pluginId, version, downloadPath)` | Downloads the package and returns its path, or `null`. |
| `PublishPluginAsync(filePath, metadata)` | Publishes a package with a `PluginPublishMetadata` record. `PluginName`, `Version`, `Description`, and `Author` are required. |
| `InvalidateCache(pluginId)` | Removes the cached plugin and version entries. Cached search results stay until they expire. |

`IHttpPluginClient` is the raw HTTP layer. It provides `SearchPluginsAsync`, `GetPluginInfoAsync`, `GetPluginVersionsAsync`, `UploadPluginAsync`, and `GetAsync`. The concrete `HttpPluginClient` also exposes `IsAvailableAsync` (health check), `SendNotificationAsync`, and `CheckForUpdatesAsync`.

### Current Limitations

- `SearchAsync` sends only `Query` and `PageSize` to the registry. `Tags`, `MinVersion`, `TargetFramework`, `OnlyVerified`, `Page`, and `SortOrder` have no effect yet.
- `GetTrendingAsync` and `GetFeaturedAsync` search with an empty query, which the registry rejects. They return a failed result. `GetHomePageAsync` still succeeds, with empty `Trending` and `Featured` lists, and caches that page for 10 minutes.
- `BrowseCategoryAsync` returns a failed result unless `filter.Query` is set.

## Plugin Discovery

`PluginDiscoveryService` (namespace `PluginEngine.Utils.Helpers`, `src/PluginEngine/Utils/Helpers/PluginDiscoveryService.cs`) scans a folder for plugin assemblies and reads their metadata without registering them with the engine. It is not registered by `DependencyInjectionSetup`, so create it yourself from `FileSystemHelper` and `VersionHelper`. `AddPluginEngineStack` registers both helpers.

```csharp
var discovery = new PluginDiscoveryService(fileSystemHelper, versionHelper, logger);
```

| Method | Description |
|--------|-------------|
| `DiscoverPluginsAsync(string directory)` | Returns `Task<List<PluginCandidateInfo>>`. Scans the directory's top level for `*.dll` files, skipping names that start with `_`, and inspects each one. A missing directory or no matches returns an empty list. Errors are logged and return an empty list. |
| `InspectPluginAsync(string filePath)` | Returns `Task<PluginCandidateInfo?>` for one file. Returns `null` when the file does not exist, its file info cannot be read, or an unexpected error occurs. If `Assembly.LoadFrom` fails, the candidate is still returned with `IsValid = false` and no metadata. |
| `FilterPlugins(candidates, filter)` | Returns a new `List<PluginCandidateInfo>` that matches `PluginDiscoveryFilter`. `filter` is optional, and a `null` value uses the default filter. |
| `GetStatistics(candidates)` | Returns a `DiscoveryStatistics` with the counts and total size for the given candidates. |

`DiscoverPluginsAsync` and `InspectPluginAsync` load each assembly with `Assembly.LoadFrom` into the default load context. This is not an isolated `AssemblyLoadContext`, so the files stay loaded for the life of the process. Use `PluginLoaderService` for plugins you plan to run.

### PluginCandidateInfo

| Field | Type | Description |
|-------|------|-------------|
| `FilePath` | `string` | Full path to the file. Required. |
| `FileName` | `string` | File name with the extension. Required. |
| `AssemblyName` | `string` | File name without the extension. Required. |
| `FileSize` | `long` | File size in bytes. Required. |
| `ModifiedAtUtc` | `DateTime` | Last write time in UTC. Required. |
| `IsValid` | `bool` | `true` when the assembly loaded. Required. |
| `DiscoveredAtUtc` | `DateTime` | Time the candidate was created, in UTC. Required. |
| `Version` | `string?` | `AssemblyVersionAttribute` value. |
| `ProductName` | `string?` | `AssemblyProductAttribute` value. |
| `Company` | `string?` | `AssemblyCompanyAttribute` value. |
| `Description` | `string?` | `AssemblyDescriptionAttribute` value. |
| `CustomAttributes` | `List<string>` | Type names of every assembly-level attribute. Read-only property, the list is mutable. |

The fields marked as required are set by `InspectPluginAsync`. The metadata fields are filled only when the assembly loads.

### PluginDiscoveryFilter

| Property | Type | Default | Effect |
|----------|------|---------|--------|
| `ValidOnly` | `bool` | `true` | Keeps only candidates with `IsValid == true`. |
| `MinimumVersionInfo` | `string?` | `null` | Keeps candidates whose `Version` is not `null` and is at least this version, using `VersionHelper.CompareVersions`. |
| `NamePattern` | `string?` | `null` | Regular expression matched against `AssemblyName`. |
| `MaxFileSizeBytes` | `long?` | `null` | Keeps candidates whose `FileSize` is at most this value. |

Because `ValidOnly` defaults to `true`, invalid candidates are removed unless you set it to `false`.

### DiscoveryStatistics

| Property | Type | Description |
|----------|------|-------------|
| `TotalCandidates` | `int` | Number of candidates passed to `GetStatistics`. |
| `ValidPlugins` | `int` | Number with `IsValid == true`. |
| `InvalidPlugins` | `int` | Number with `IsValid == false`. |
| `TotalSizeBytes` | `long` | Sum of `FileSize`. |
| `DiscoveredAtUtc` | `DateTime` | Time the statistics were created, in UTC. |
| `ValidPercentage` | `double` | Read-only. Valid plugins as a percentage of `TotalCandidates`, or `0` when there are none. |

Example:

```csharp
var discovery = new PluginDiscoveryService(fileSystemHelper, versionHelper, logger);

var candidates = await discovery.DiscoverPluginsAsync(Path.Combine(AppContext.BaseDirectory, "plugins"));

var usable = discovery.FilterPlugins(candidates, new PluginDiscoveryFilter
{
    MinimumVersionInfo = "1.2.0",
    NamePattern = "^Acme\\.",
    MaxFileSizeBytes = 5 * 1024 * 1024
});

var stats = discovery.GetStatistics(candidates);
Console.WriteLine($"{stats.ValidPlugins}/{stats.TotalCandidates} valid ({stats.ValidPercentage:F0}%)");
```

## Validation and Versioning

Three types handle plugin validation and version strings. `PluginValidator` checks a whole `Plugin` entity. `VersionHelper` parses and matches individual version strings, including the constraint syntax used by plugin dependencies. `IVersioningService` is the service-layer API for the same kinds of checks.

| Type | Namespace | Role |
|------|-----------|------|
| `PluginValidator` | `PluginEngine.Utils.Validators` | Validates a `Plugin` and its metadata and dependencies. Returns a `PluginValidationResult`. |
| `VersionHelper` | `PluginEngine.Utils.Helpers` | Parses, compares, and matches version strings. Supports constraint syntax. |
| `IVersioningService` | `PluginEngine.Services.Abstractions` | Version validation, comparison, and increment. Implemented by `VersioningService`. |

### PluginValidator

`PluginValidator` takes an `ILogger<PluginValidator>` and a `VersionHelper`. `ServiceCollectionExtensions.AddPluginEngineStack` registers it, along with `VersionHelper`, as a singleton. `Validate(Plugin)` collects every error instead of stopping at the first one. It applies these rules:

| Field | Rule |
|-------|------|
| Name | Required. At most 100 characters. Must not start with `System.` or `Microsoft.`. Must start with a letter or digit. Other characters must be letters, digits, `-`, `_`, or `.`. |
| Version | Required. Must be a valid version (`VersionHelper.IsValidSemanticVersion`). Must not be `0.0.0`. |
| Metadata | Skipped when `Metadata` is `null`. Otherwise `Description` and `Author` are required, and `Author` must be at most 100 characters. |
| Dependencies | At most 50 dependencies. Each `MinimumVersion` is required and must be a valid version. `MaximumVersion` is optional, but when set it must be a valid version that is not less than `MinimumVersion`. Each `DependencyPluginId` may appear only once. |

`ValidateDependencyRelationship(dependent, dependency, depSpec)` is a separate check. It returns `false` when a plugin depends on itself, or when `dependency.Version` does not satisfy `depSpec.MinimumVersion` under the constraint rules below.

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using PluginEngine.Utils.Helpers;
using PluginEngine.Utils.Validators;

var versionHelper = new VersionHelper(NullLogger<VersionHelper>.Instance);
var validator = new PluginValidator(NullLogger<PluginValidator>.Instance, versionHelper);

PluginValidationResult result = validator.Validate(plugin);
if (!result.IsValid)
{
    Console.WriteLine(result);                  // "Invalid: N errors, 0 warnings"
    Console.WriteLine(result.GetErrorSummary()); // one error per line
}
```

### VersionHelper Constraint Syntax

`SatisfiesConstraint(version, constraint)` returns `true` when `version` satisfies `constraint`. Write constraints without spaces between the operator and the version:

| Constraint | Matches when |
|------------|--------------|
| `1.2.0` (no operator) | The version equals `1.2.0` exactly. |
| `>=1.2.0` | The version is `1.2.0` or later. |
| `>1.2.0` | The version is later than `1.2.0`. |
| `<=1.2.0` | The version is `1.2.0` or earlier. |
| `<1.2.0` | The version is earlier than `1.2.0`. |
| `^1.2.0` | The major version is `1`, and the version is `1.2.0` or later. |
| `~1.2.0` | The major and minor versions are `1.2`, and the version is `1.2.0` or later. |

- A leading `v` is ignored. Prerelease and build suffixes (`-beta`, `+build`) are dropped before comparison, so `2.0.0-beta` is compared as `2.0.0`.
- `==` is not an operator here. `SatisfiesConstraint` returns `false` for it. Use a bare version for an exact match.
- Unsupported operators, unparseable versions, and unparseable constraint targets return `false`.
- An empty constraint throws `IndexOutOfRangeException`. Pass a non-empty string.

```csharp
var versionHelper = new VersionHelper(NullLogger<VersionHelper>.Instance);

versionHelper.SatisfiesConstraint("1.4.2", "^1.2.0");         // true
versionHelper.SatisfiesConstraint("1.3.0", "~1.2.0");         // false
versionHelper.SatisfiesConstraint("2.0.0-beta", ">=1.5.0");   // true
```

### VersioningService

`VersioningService` implements `IVersioningService`. `DependencyInjectionSetup.AddPluginEngine` registers it as a singleton. It uses `System.Version` directly, not `VersionHelper`, so its rules differ from the constraint syntax above:

- `ValidateVersion` accepts any string that `Version.TryParse` accepts. Prerelease strings such as `1.0.0-beta` are rejected.
- `IsSatisfiedBy(constraint, version)` handles only `>=` and `==`. A constraint without `>=`, `<=`, or `==` is a prefix match on the version's string form, so `1.2` also matches `1.20.0`. Constraints such as `>1.0`, `<=1.0`, `^1.0`, and `~1.0` always return `false`.
- `CompareVersions` throws `ArgumentException` when either input is not a valid version. `VersionHelper.CompareVersions` returns `-1` in that case.
- `ParseVersion` returns a `SemanticVersion` with `Major`, `Minor`, `Patch`, `Prerelease`, and `Metadata`. It throws `ArgumentException` for an empty string.
- `IncrementVersion(version, VersionPart)` returns the next version as a string. `Major`, `Minor`, and `Patch` increment their part and reset the lower parts to `0`.
- `AreCompatible` returns `true` when both versions have the same major version.
- `GetVersionHistoryAsync` and `GetLatestVersionAsync` are placeholders. They return an empty sequence and `null`, because they do not query a repository yet.

```csharp
using PluginEngine.Services.Abstractions;

IVersioningService versioning = serviceProvider.GetRequiredService<IVersioningService>();

string next = versioning.IncrementVersion("1.4.2", VersionPart.Minor); // "1.5.0"
bool compatible = versioning.AreCompatible("1.4.2", "1.9.0");          // true
bool satisfied = versioning.IsSatisfiedBy(">=1.4.0", "1.4.2");         // true
```

## Exceptions and Error Codes

Every engine exception derives from `PluginException` (namespace `PluginEngine.Exceptions`). It adds three members:

- `ErrorCode` (`string`, default `"PLUGIN_ERROR"`): a machine-readable code.
- `EntityId` (`Guid?`): the plugin or entity the error relates to. Set it with `WithEntityId(Guid)`.
- `Context` (`Dictionary<string, object>`): extra details. Add entries with `WithContext(key, value)`.

`WithContext` and `WithEntityId` return the exception, so you can chain them.

### Exception hierarchy

```
Exception
└── PluginException
    ├── PluginLoadException            (sealed)
    ├── DependencyResolutionException  (sealed)
    ├── VersionMismatchException       (sealed)
    └── PluginIncompatibleException    (sealed)
```

| Exception | Thrown when | Type-specific members | Default `ErrorCode` |
|-----------|-------------|-----------------------|---------------------|
| `PluginLoadException` | A plugin fails to load. | `PluginName`, `AssemblyPath`, `LoadStage` (`PluginLoadStage`) | `PLUGIN_LOAD_ERROR` |
| `DependencyResolutionException` | Dependency resolution fails. | `DependencyPluginId` (`Guid?`), `VersionConstraint`, `Reason` (`DependencyResolutionReason`), `UnresolvedDependencies` (`List<string>`) | `DEPENDENCY_RESOLUTION_ERROR` |
| `VersionMismatchException` | A version constraint is not satisfied. | `ExpectedVersion`, `ActualVersion`, `ComponentType`, `ComponentName` | `VERSION_MISMATCH_ERROR` |
| `PluginIncompatibleException` | A plugin's declared constraint does not match the host engine version. | `DeclaredConstraint`, `HostEngineVersion` | `PLUGIN_INCOMPATIBLE` |

### PluginLoadException and PluginLoadStage

`PluginLoadStage` records the step where loading failed:

| Value | Meaning |
|-------|---------|
| `Unknown` (0) | The stage could not be determined. This is the default. |
| `MetadataValidation` (1) | Validating the plugin metadata. |
| `AssemblyResolution` (2) | Resolving the plugin assembly on disk. |
| `TypeLoading` (3) | Loading types from the plugin assembly. |
| `InterfaceValidation` (4) | Validating the plugin interface implementation. |
| `Instantiation` (5) | Creating the plugin instance. |
| `Initialization` (6) | Initializing the plugin. |
| `DependencyResolution` (7) | Resolving plugin dependencies. |

### DependencyResolutionException and DependencyResolutionReason

`DependencyResolutionReason` records why resolution failed:

| Value | Meaning |
|-------|---------|
| `Unknown` (0) | The reason could not be determined. This is the default. |
| `DependencyNotFound` (1) | A required dependency is not registered. |
| `VersionMismatch` (2) | An installed dependency version does not satisfy the constraint. |
| `CircularDependency` (3) | The dependency graph contains a cycle. |
| `DependencyNotLoaded` (4) | A dependency exists but has not been loaded. |
| `OptionalDependencyFailed` (5) | An optional dependency failed to resolve. |

```csharp
try
{
    // ... resolve the plugin's dependencies
}
catch (DependencyResolutionException ex) when (ex.Reason == DependencyResolutionReason.DependencyNotFound)
{
    foreach (var name in ex.UnresolvedDependencies)
    {
        Console.WriteLine($"{ex.ErrorCode}: missing {name}");
    }
}
```

### Error codes

`PluginEngine.Constants.ErrorCodes` declares these string constants:

| Constant | Value |
|----------|-------|
| `GenericError` | `PLUGIN_ERROR` |
| `PluginNotFound` | `PLUGIN_NOT_FOUND` |
| `PluginAlreadyLoaded` | `PLUGIN_ALREADY_LOADED` |
| `PluginLoadFailed` | `PLUGIN_LOAD_FAILED` |
| `DependencyResolutionFailed` | `DEPENDENCY_RESOLUTION_FAILED` |
| `VersionMismatch` | `VERSION_MISMATCH` |
| `InvalidConfiguration` | `INVALID_CONFIGURATION` |
| `HotReloadFailed` | `HOT_RELOAD_FAILED` |
| `CircularDependency` | `CIRCULAR_DEPENDENCY` |
| `OperationTimeout` | `OPERATION_TIMEOUT` |

These constants are not used by the library. The exception types set their own `ErrorCode` values, listed in the table above, and those values differ from the constants. For example, `PluginLoadException` sets `PLUGIN_LOAD_ERROR`, not `ErrorCodes.PluginLoadFailed` (`PLUGIN_LOAD_FAILED`). When you handle exceptions, match on the exception type or on `ErrorCode` as set by the exception, not on `ErrorCodes`.

For the numeric codes on `PluginOperationResult.ErrorCode` and how exceptions map to them, see [docs/error-codes.md](docs/error-codes.md).

## Project Structure

```
dotnet-plugin-engine/
├── src/PluginEngine/                      The library (the only buildable source)
│   ├── BackgroundServices/                Hosted services: background plugin monitor and health checks (excluded from build)
│   ├── Caching/                           Plugin cache abstraction (IPluginCache), in-memory implementation, cache statistics
│   ├── Cli/                               Command-line host and command parser (excluded from build)
│   ├── Configuration/                     PluginEngineOptions, DI registration (AddPluginEngine), logging and webhook settings
│   ├── Constants/                         Library-wide defaults: plugin directory and config file names, length limits, timeouts
│   ├── Data/                              Repositories, including the in-memory IPluginRepository
│   ├── Domain/                            Domain entities: Plugin, PluginMetadata, PluginDependency, VersionInfo, and related types
│   ├── Events/                            In-process pub/sub: IPluginEvent, event publisher and subscriber
│   ├── Exceptions/                        PluginException hierarchy and the *Extensions and *Validation helpers
│   ├── Execution/                         Plugin lifecycle contract (IPluginLifecycle) and execution context
│   ├── Formatters/                        Output formatters for plugin data: JSON, XML, CSV
│   ├── Integration/                       HTTP plugin client, remote plugin registry, and webhook handler
│   ├── Marketplace/                       Marketplace browsing and publishing services and their models
│   ├── Middleware/                        ASP.NET-style pipeline: caching, error handling, logging, rate limiting
│   ├── Results/                           PluginOperationResult and its extensions
│   ├── Services/                          Abstractions (I*Service interfaces) and implementations (loader, manager, versioning, hot reload/swap)
│   └── Utils/                             Extensions, helpers and validators shared across the library
├── tests/dotnet-plugin-engine.Tests/      xUnit tests (one file per type, FluentAssertions and Moq)
├── benchmarks/dotnet-plugin-engine.Benchmarks/   BenchmarkDotNet suites for resolution, loading, discovery and events
└── docs/                                  ARCHITECTURE.md, guides, and one reference page per public type
```

## Contributing
Fork the repository, create a branch for your feature or bug fix, make your changes, and ensure that `dotnet test` passes before submitting a pull request. Follow conventional commits for commit messages.

## License
This project is licensed under the MIT License - see the `src/PluginEngine/PluginEngine.csproj` file for the `PackageLicenseExpression` property.