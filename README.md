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

## Contributing
Fork the repository, create a branch for your feature or bug fix, make your changes, and ensure that `dotnet test` passes before submitting a pull request. Follow conventional commits for commit messages.

## License
This project is licensed under the MIT License - see the `src/PluginEngine/PluginEngine.csproj` file for the `PackageLicenseExpression` property.