# ServiceCollectionExtensions

Provides extension methods for `IServiceCollection` that register the core components of the plugin engine, allowing developers to plug in middleware, event handlers, repositories, and pipeline configuration in a fluent manner.

## API

### `AddPluginEngineStack(IServiceCollection services)`

**Purpose**  
Registers the fundamental services required by the plugin engine (e.g., plugin loader, service resolver, and default pipeline builder).

**Parameters**  
- `services`: The service collection to which the plugin engine services are added.

**Return value**  
The same `IServiceCollection` instance, enabling method chaining.

**Exceptions**  
- `ArgumentNullException` if `services` is `null`.  
- `InvalidOperationException` if the plugin engine stack has already been registered.

### `AddPluginMiddleware<TMiddleware>(IServiceCollection services)`

**Purpose**  
Adds a custom middleware component to the plugin execution pipeline.

**Parameters**  
- `services`: The service collection to receive the middleware registration.  
- `TMiddleware`: The middleware type to register; must implement `IPluginMiddleware`.

**Return value**  
The same `IServiceCollection` instance.

**Exceptions**  
- `ArgumentNullException` if `services` is `null`.  
- `ArgumentException` if `TMiddleware` does not satisfy the `IPluginMiddleware` constraint.  
- `InvalidOperationException` if the same middleware type has already been added.

### `ConfigurePluginPipeline(IServiceCollection services, Action<PluginPipelineOptions> configure)`

**Purpose**  
Configures options that control the behavior of the plugin pipeline (e.g., ordering, error handling).

**Parameters**  
- `services`: The service collection to which the options are bound.  
- `configure`: A delegate that receives a `PluginPipelineOptions` instance for configuration.

**Return value**  
The same `IServiceCollection` instance.

**Exceptions**  
- `ArgumentNullException` if `services` or `configure` is `null`.

### `AddPluginEventHandler<TEvent, THandler>(IServiceCollection services)`

**Purpose**  
Registers an event handler for a specific plugin event type.

**Parameters**  
- `services`: The service collection receiving the handler registration.  
- `TEvent`: The event type; must implement `IPluginEvent`.  
- `THandler`: The handler type; must implement `IPluginEventHandler<TEvent>`.

**Return value**  
The same `IServiceCollection` instance.

**Exceptions**  
- `ArgumentNullException` if `services` is `null`.  
- `ArgumentException` if `TEvent` or `THandler` do not meet their respective interface constraints.  
- `InvalidOperationException` if a handler for the same event type is already registered.

### `UsePluginRepository<TRepository>(IServiceCollection services)`

**Purpose**  
Registers a repository implementation used by the plugin engine to persist plugin metadata.

**Parameters**  
- `services`: The service collection to receive the repository registration.  
- `TRepository`: The repository type; must implement `IPluginRepository`.

**Return value**  
The same `IServiceCollection` instance.

**Exceptions**  
- `ArgumentNullException` if `services` is `null`.  
- `ArgumentException` if `TRepository` does not implement `IPluginRepository`.  
- `InvalidOperationException` if a repository has already been registered.

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using DotNetPluginEngine;
using DotNetPluginEngine.Abstractions;

var services = new ServiceCollection();

// Register core plugin engine services
services.AddPluginEngineStack();

// Add a custom middleware that logs plugin execution
services.AddPluginMiddleware<LoggingMiddleware>();

// Configure pipeline options (e.g., stop on first error)
services.ConfigurePluginPipeline(opts => { opts.StopOnFirstError = true; });

// Register an event handler for plugin loaded events
services.AddPluginEventHandler<PluginLoadedEvent, PluginLoadedHandler>();

// Register a repository for persisting plugin metadata
services.UsePluginRepository<SqlPluginRepository>();

var provider = services.BuildServiceProvider();
// The provider can now be used to resolve plugin engine components.
```

```csharp
using Microsoft.Extensions.Hosting;
using DotNetPluginEngine;
using DotNetPluginEngine.Abstractions;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Core registration
        services.AddPluginEngineStack();

        // Middleware from configuration
        services.AddPluginMiddleware<AuthMiddleware>();

        // Pipeline configuration from appsettings
        services.ConfigurePluginPipeline(hostContext.Configuration.GetSection("PluginPipeline"));

        // Event handling
        services.AddPluginEventHandler<PluginErrorEvent, ErrorLoggingHandler>();

        // Repository selection based on environment
        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            services.UsePluginRepository<InMemoryPluginRepository>();
        }
        else
        {
            services.UsePluginRepository<PostgresPluginRepository>();
        }
    })
    .Build()
    .Run();
```

## Notes

- All extension methods are safe to invoke from multiple threads **as long as** the same `IServiceCollection` instance is not being mutated concurrently. Typical usage occurs during application startup, which is single‑threaded.
- Registering the same service type more than once (e.g., calling `AddPluginMiddleware<TMiddleware>` twice with the same `TMiddleware`) will result in an `InvalidOperationException` because the plugin engine expects a single implementation per service.
- Generic type constraints are enforced at compile time; supplying a type that does not implement the required interface will cause a compilation error, preventing accidental mis‑registration.
- The order of registration can affect behavior: middleware is executed in the order it is added, and pipeline options should be configured **after** the middleware registrations if they depend on the middleware list.
- Once `BuildServiceProvider` is called, further calls to these extension methods on the same collection have no effect on the already built provider and may lead to undefined behavior; therefore, all registrations should be completed before building the provider.
