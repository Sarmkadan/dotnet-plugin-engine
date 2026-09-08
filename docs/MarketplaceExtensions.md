# MarketplaceExtensions

`MarketplaceExtensions` provides dependency-injection registration for the plugin marketplace services. It is defined in the `PluginEngine.Marketplace` namespace.

## Signature

| Method | Parameter | Return value | Null handling |
| --- | --- | --- | --- |
| `AddPluginMarketplace(this IServiceCollection services)` | The service collection to add registrations to | The same `IServiceCollection` instance, enabling fluent registration | Throws `ArgumentNullException` when `services` is `null` |

## Registered services

Calling `AddPluginMarketplace` makes the following registrations:

| Service | Implementation | Lifetime |
| --- | --- | --- |
| `IMemoryCache` | Added through `AddMemoryCache()` | Singleton |
| `IPluginMarketplaceService` | `PluginMarketplaceService` | Singleton |
| `IMarketplaceBrowserService` | `MarketplaceBrowserService` | Singleton |

The memory cache is used by both marketplace implementations. `PluginMarketplaceService` caches compatibility matrices, while `MarketplaceBrowserService` caches discovery data such as categories, trending plugins, featured plugins, and the marketplace home page.

## Prerequisites

`AddPluginMarketplace` calls `AddMemoryCache()`, so callers do not need to register `IMemoryCache` separately.

The marketplace service also requires an `IRemotePluginRegistry`. `AddPluginMarketplace` does not register that dependency and does not call `AddHttpClient()`. `AddPluginEngineStack()` visibly adds HTTP client services and maps `IRemotePluginRegistry` to `RemotePluginRegistry`; however, `RemotePluginRegistry` also requires an `HttpPluginClient`, which the stack method does not register.

`AddPluginEngine()` by itself registers the core plugin engine services, but it does not register HTTP client services or `IRemotePluginRegistry`.

The registered marketplace implementations also request `ILogger<PluginMarketplaceService>` and `ILogger<MarketplaceBrowserService>` from dependency injection. The logging services therefore need to be available when the singleton services are resolved.

## Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Integration;
using PluginEngine.Marketplace;
using PluginEngine.Utils.Extensions;

var services = new ServiceCollection();

services.AddLogging();
services.AddPluginEngineStack();
services.AddTransient<HttpPluginClient>();
services.AddPluginMarketplace();

using var provider = services.BuildServiceProvider();

var marketplace = provider.GetRequiredService<IPluginMarketplaceService>();
var browser = provider.GetRequiredService<IMarketplaceBrowserService>();
```

`AddPluginMarketplace` returns `services`, so the registrations can also be chained:

```csharp
services
    .AddPluginEngineStack()
    .AddPluginMarketplace();
```
