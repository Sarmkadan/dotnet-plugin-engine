# dotnet-plugin-engine

[... existing content ...]

## StringExtensions

The `StringExtensions` class provides a set of extension methods for string operations commonly used in plugin processing. It offers utilities for path normalization, plugin identifier and version validation, filename sanitization, and more. These extensions simplify tasks such as handling plugin paths, validating plugin identifiers and versions, and formatting strings for better readability.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Utils.Extensions;

public class StringExtensionsDemo
{
    public void DemonstrateStringExtensions()
    {
        // Normalize a plugin path
        string normalizedPath = "/path/to/plugin/'.NormalizePluginPath();
        Console.WriteLine($"Normalized path: {normalizedPath}");

        // Validate a plugin identifier
        string pluginId = "123e4567-e89b-12d3-a456-426655440000";
        bool isValidId = pluginId.IsValidPluginId();
        Console.WriteLine($"Is valid plugin ID: {isValidId}");

        // Validate a semantic version
        string version = "1.2.3";
        bool isValidVersion = version.IsValidVersion();
        Console.WriteLine($"Is valid version: {isValidVersion}");

        // Sanitize a filename
        string filename = "My Plugin.dll";
        string sanitizedFilename = filename.SanitizeForFilename();
        Console.WriteLine($"Sanitized filename: {sanitizedFilename}");

        // Get the assembly name from a file path
        string filePath = "/path/to/MyPlugin.dll";
        string assemblyName = filePath.GetAssemblyName();
        Console.WriteLine($"Assembly name: {assemblyName}");

        // Check if a path is an assembly path
        bool isAssemblyPath = filePath.IsAssemblyPath();
        Console.WriteLine($"Is assembly path: {isAssemblyPath}");

        // Truncate a string with an ellipsis
        string longString = "This is a very long string that needs to be truncated.";
        string truncatedString = longString.TruncateWithEllipsis(20);
        Console.WriteLine($"Truncated string: {truncatedString}");

        // Convert a TimeSpan to a readable format
        TimeSpan timeSpan = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromSeconds(45));
        string readableTimeSpan = timeSpan.ToReadableTimeSpan();
        Console.WriteLine($"Readable time span: {readableTimeSpan}");

        // Format bytes
        long bytes = 1024 * 1024 * 2;
        string formattedBytes = bytes.FormatBytes();
        Console.WriteLine($"Formatted bytes: {formattedBytes}");
    }
}

## TypeExtensions

The `TypeExtensions` class provides a set of extension methods for type operations in the plugin system. It offers utilities for discovering interfaces, attributes, and type metadata. These extensions simplify tasks such as checking if a type implements an interface, getting types that implement an interface, and getting type metadata.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Utils.Extensions;
using System;
using System.Reflection;

public class TypeExtensionsDemo
{
    public void DemonstrateTypeExtensions()
    {
        // Check if a type implements an interface
        Type type = typeof(string);
        bool implementsInterface = type.ImplementsInterface<IComparable>();
        Console.WriteLine($"Implements IComparable: {implementsInterface}");

        // Get types that implement an interface
        Assembly assembly = Assembly.GetExecutingAssembly();
        var typesImplementingInterface = assembly.GetTypesImplementing<IComparable>();
        Console.WriteLine($"Types implementing IComparable: {string.Join(", ", typesImplementingInterface)}");

        // Get loadable types from an assembly
        var loadableTypes = assembly.GetLoadableTypes();
        Console.WriteLine($"Loadable types: {string.Join(", ", loadableTypes)}");

        // Check if a type has an attribute
        bool hasAttribute = type.HasAttribute<SerializableAttribute>();
        Console.WriteLine($"Has SerializableAttribute: {hasAttribute}");

        // Get an attribute from a type
        var attribute = type.GetAttribute<SerializableAttribute>();
        Console.WriteLine($"SerializableAttribute: {attribute}");

        // Check if a type is a concrete class
        bool isConcreteClass = type.IsConcreteClass();
        Console.WriteLine($"Is concrete class: {isConcreteClass}");

        // Get full metadata about a type
        string fullMetadata = type.GetFullMetadata();
        Console.WriteLine($"Full metadata: {fullMetadata}");

        // Check if a type is JSON serializable
        bool isJsonSerializable = type.IsJsonSerializable();
        Console.WriteLine($"Is JSON serializable: {isJsonSerializable}");

        // Get public property values from an object
        var propertyValues = new object().GetPropertyValues();
        Console.WriteLine($"Property values: {string.Join(", ", propertyValues)}");
    }
}

## EnumExtensions

The `EnumExtensions` class provides a set of extension methods for enum operations in the plugin system. It offers utilities for converting enums to user-friendly strings, extracting descriptions from attributes, parsing enum values, and checking enum states. These extensions simplify tasks such as displaying plugin statuses, converting states to CSS classes, and working with enum collections.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Utils.Extensions;
using System.ComponentModel;

public class EnumExtensionsDemo
{
    public enum PluginStatus
    {
        [Description("Not Loaded")]
        Unloaded,
        [Description("Loading...")]
        Loading,
        [Description("Loaded")]
        Loaded,
        [Description("Unloading...")]
        Unloading,
        [Description("Error")]
        Failed,
        [Description("Disabled")]
        Inactive
    }

    public enum ExecutionState
    {
        Running,
        Completed,
        Failed,
        Cancelled,
        Timeout
    }

    public void DemonstrateEnumExtensions()
    {
        // Get description from Display attribute
        PluginStatus status = PluginStatus.Loaded;
        string description = status.GetDescription();
        Console.WriteLine($"Status description: {description}");

        // Get display name from Display attribute
        string displayName = status.GetDisplayName();
        Console.WriteLine($"Display name: {displayName}");

        // Convert to user-friendly string
        string friendlyString = status.ToUserFriendlyString();
        Console.WriteLine($"User-friendly string: {friendlyString}");

        // Convert to CSS class
        string cssClass = status.ToCssClass();
        Console.WriteLine($"CSS class: {cssClass}");

        // Check if status is healthy
        bool isHealthy = status.IsHealthy();
        Console.WriteLine($"Is healthy: {isHealthy}");

        // Check if status is transient
        bool isTransient = status.IsTransient();
        Console.WriteLine($"Is transient: {isTransient}");

        // Get all enum values
        var allValues = EnumExtensions.GetAllValues<PluginStatus>();
        Console.WriteLine($"All status values: {string.Join(", ", allValues)}");

        // Try parse enum from string
        PluginStatus? parsedStatus = EnumExtensions.TryParse<PluginStatus>("loaded");
        Console.WriteLine($"Parsed status: {parsedStatus}");

        // Get int value of enum
        int intValue = status.GetIntValue();
        Console.WriteLine($"Int value: {intValue}");

        // Get value descriptions
        var valueDescriptions = EnumExtensions.GetValueDescriptions<ExecutionState>();
        foreach (var (Value, Description) in valueDescriptions)
        {
            Console.WriteLine($"{Value}: {Description}");
        }

        // Convert execution state to color hex
        ExecutionState state = ExecutionState.Completed;
        string colorHex = state.ToColorHex();
        Console.WriteLine($"Color hex: {colorHex}");

        // Check if state is terminal
        bool isTerminal = state.IsTerminal();
        Console.WriteLine($"Is terminal: {isTerminal}");
    }
}
```

## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides extension methods for `IServiceCollection` that simplify the registration of plugin engine services. It offers a fluent API for configuring the complete plugin stack, middleware, caching, event systems, and custom repositories. These extensions make it easy to set up plugin processing pipelines with dependency injection.

Here's a realistic usage example leveraging its public members:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine;
using PluginEngine.Utils.Extensions;

public class PluginEngineDemo
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add the complete plugin engine stack with optional configuration
        services.AddPluginEngineStack(options =>
        {
            options.PluginDirectory = "/var/plugins";
            options.MaxConcurrentPlugins = 10;
            options.PluginTimeout = TimeSpan.FromSeconds(30);
        });

        // Register custom middleware
        services.AddPluginMiddleware<MyCustomMiddleware>();

        // Configure the middleware pipeline
        services.ConfigurePluginPipeline(pipeline =>
        {
            pipeline.AddMiddleware<LoggingMiddleware>();
            pipeline.AddMiddleware<ValidationMiddleware>();
            pipeline.AddMiddleware<ExecutionMiddleware>();
        });

        // Register a custom event handler
        services.AddPluginEventHandler<PluginLoadedEvent, PluginLoadedEventHandler>();
        services.AddPluginEventHandler<PluginFailedEvent, PluginFailedEventHandler>();

        // Register a custom plugin repository
        services.UsePluginRepository<CustomPluginRepository>();
    }
}

// Example custom middleware
public class MyCustomMiddleware : IPluginMiddleware
{
    public Task ProcessAsync(PluginContext context, PluginMiddlewareDelegate next)
    {
        Console.WriteLine($"Processing plugin: {context.Plugin.Name}");
        return next(context);
    }
}

// Example event types
public class PluginLoadedEvent : IPluginEvent
{
    public string PluginName { get; set; }
    public DateTime LoadedAt { get; set; }
}

public class PluginFailedEvent : IPluginEvent
{
    public string PluginName { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime FailedAt { get; set; }
}

// Example event handlers
public class PluginLoadedEventHandler : IPluginEventHandler<PluginLoadedEvent>
{
    public Task HandleAsync(PluginLoadedEvent @event)
    {
        Console.WriteLine($"Plugin loaded: {@event.PluginName} at {@event.LoadedAt}");
        return Task.CompletedTask;
    }
}

public class PluginFailedEventHandler : IPluginEventHandler<PluginFailedEvent>
{
    public Task HandleAsync(PluginFailedEvent @event)
    {
        Console.WriteLine($"Plugin failed: {@event.PluginName} - {@event.ErrorMessage}");
        return Task.CompletedTask;
    }
}

// Example custom repository
public class CustomPluginRepository : IPluginRepository
{
    public Task<IEnumerable<PluginMetadata>> GetAvailablePluginsAsync()
    {
        // Custom implementation
        return Task.FromResult(Enumerable.Empty<PluginMetadata>());
    }

    public Task<PluginMetadata?> GetPluginMetadataAsync(string pluginId)
    {
        // Custom implementation
        return Task.FromResult<PluginMetadata?>(null);
    }
}
```

## MemoryPluginCache

[... existing content ...]
