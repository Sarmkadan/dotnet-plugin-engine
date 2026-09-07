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
