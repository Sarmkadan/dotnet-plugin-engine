# DependencyInjectionSetup

`DependencyInjectionSetup` provides extension methods for registering the plugin engine with a Microsoft dependency injection `IServiceCollection`.

## Signatures

| Method | Description |
| --- | --- |
| `IServiceCollection AddPluginEngine(this IServiceCollection services, Action<PluginEngineOptions>? configureOptions = null)` | Creates a `PluginEngineOptions` instance, applies the optional configuration delegate, validates the options, and registers the plugin engine services. |
| `IServiceCollection AddPluginEngine(this IServiceCollection services)` | Registers the plugin engine with the default `PluginEngineOptions` values by delegating to the configurable overload with `null`. |

Both overloads return the same `IServiceCollection` instance so registration calls can be chained.

## Registered services

All registrations use the singleton lifetime.

| Service type | Implementation or registered instance | Lifetime |
| --- | --- | --- |
| `PluginEngineOptions` | The options instance created and optionally configured by `AddPluginEngine` | Singleton |
| `IPluginRepository` | `PluginRepository` | Singleton |
| `IPluginLoaderService` | `PluginLoaderService` | Singleton |
| `IDependencyResolutionService` | `DependencyResolutionService` | Singleton |
| `IVersioningService` | `VersioningService` | Singleton |
| `IHotReloadService` | `HotReloadService` | Singleton |
| `IHotSwapService` | `HotSwapService` | Singleton |
| `IPluginDependencyResolver` | `PluginDependencyResolver` | Singleton |
| `IPluginManagerService` | `PluginManagerService` | Singleton |
| `PluginEngine` | `PluginEngine` | Singleton |

## Configuration and validation

The configurable overload constructs `PluginEngineOptions`, invokes the supplied delegate when it is not `null`, and then calls `PluginEngineOptions.IsValid()`. If validation fails, it throws `InvalidOperationException` with the messages returned by `GetValidationErrors()`. No services are registered before this validation succeeds.

`IsValid()` checks that:

- `PluginDirectory` is not null, empty, or whitespace.
- `HotReloadCheckIntervalMs` is greater than zero.
- `OperationTimeoutMs` is greater than zero.
- `MaxConcurrentPluginLoads` is greater than zero.

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;

var services = new ServiceCollection();

services.AddPluginEngine(options =>
{
    options.PluginDirectory = "extensions";
    options.EnableHotReload = true;
    options.HotReloadCheckIntervalMs = 2_000;
    options.EnableDependencyCaching = true;
    options.OperationTimeoutMs = 15_000;
    options.MaxConcurrentPluginLoads = 8;
    options.TargetFramework = "net10.0";
    options.StrictVersionChecking = true;
});

using ServiceProvider provider = services.BuildServiceProvider();
var pluginEngine = provider.GetRequiredService<PluginEngine.PluginEngine>();
```

To use every default option value, call the parameterless overload:

```csharp
services.AddPluginEngine();
```
