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

## IntegrationTests

The `IntegrationTests` class contains comprehensive integration tests for the plugin engine system, validating end-to-end workflows including dependency resolution, version validation, file system operations, and plugin lifecycle management. It tests critical functionality such as loading plugins with dependencies, validating version constraints, detecting circular dependencies, and managing plugin capabilities.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Implementations;
using PluginEngine.Utils.Helpers;
using PluginEngine.Utils.Validators;
using Microsoft.Extensions.Logging;
using Moq;

public class IntegrationTestsDemo
{
private readonly Mock<ILogger<DependencyResolutionService>> _mockDepLogger;
private readonly Mock<ILogger<PluginValidator>> _mockValLogger;
private readonly Mock<ILogger<VersionHelper>> _mockVerLogger;
private readonly Mock<ILogger<FileSystemHelper>> _mockFsLogger;
private readonly Mock<IPluginLoaderService> _mockLoaderService;

public IntegrationTestsDemo()
{
_mockDepLogger = new Mock<ILogger<DependencyResolutionService>>();
_mockValLogger = new Mock<ILogger<PluginValidator>>();
_mockVerLogger = new Mock<ILogger<VersionHelper>>();
_mockFsLogger = new Mock<ILogger<FileSystemHelper>>();
_mockLoaderService = new Mock<IPluginLoaderService>();
}

public void DemonstrateIntegrationTests()
{
// Test plugin creation and dependency resolution
var pluginId = Guid.NewGuid();
var plugin = new Plugin
{
Id = pluginId,
Name = "MainPlugin",
Version = "1.0.0",
AssemblyPath = $"/plugins/MainPlugin.dll"
};

var dependency = new PluginDependency
{
PluginId = pluginId,
DependencyPluginId = Guid.NewGuid(),
MinimumVersion = "1.0.0"
};
plugin.AddDependency(dependency);

// Test version validation
var versionHelper = new VersionHelper(_mockVerLogger.Object);
bool satisfiesConstraint = versionHelper.SatisfiesConstraint("1.5.0", ">=1.0.0");
Console.WriteLine($"Version constraint satisfied: {satisfiesConstraint}");

// Test file system operations
var fsHelper = new FileSystemHelper(_mockFsLogger.Object);
string testDir = Path.Combine(Path.GetTempPath(), $"plugin-test-{Guid.NewGuid()}");
bool directoryCreated = fsHelper.EnsureDirectoryExists(testDir);
Console.WriteLine($"Directory created: {directoryCreated}");

// Test plugin capabilities
var capability = new PluginCapability
{
PluginId = pluginId,
Name = "DataTransform",
Version = "1.0.0",
InterfaceTypeName = "ITransform",
Tags = "data,transform"
};
plugin.AddCapability(capability);

var transformCapabilities = plugin.Capabilities
.Where(c => c.Tags?.Contains("transform") == true)
.ToList();
Console.WriteLine($"Found {transformCapabilities.Count} transform capabilities");

// Test circular dependency detection
var resolutionService = new DependencyResolutionService(_mockLoaderService.Object);
bool hasCircular = resolutionService.HasCircularDependenciesAsync(plugin).Result;
Console.WriteLine($"Has circular dependencies: {hasCircular}");
}
}
```

## PluginExecutionContextTests

The `PluginExecutionContextTests` class contains unit tests for the `PluginExecutionContext` class, verifying its initialization, state transitions, data storage, and summary generation capabilities. It tests scenarios such as creating new contexts, completing operations successfully or with failures, cancelling operations, and storing arbitrary execution data.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Domain.Entities;
using PluginEngine.Execution;
using System;

public class PluginExecutionContextDemo
{
    public void DemonstratePluginExecutionContext()
    {
        // Create a plugin instance
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "SamplePlugin",
            Version = "1.0.0",
            AssemblyPath = "/plugins/SamplePlugin.dll"
        };

        // Create a new execution context for a plugin operation
        var context = new PluginExecutionContext
        {
            Plugin = plugin,
            OperationType = "Execute"
        };

        Console.WriteLine($"Execution ID: {context.ExecutionId}");
        Console.WriteLine($"State: {context.State}");
        Console.WriteLine($"Started at: {context.StartedAtUtc}");
        Console.WriteLine($"Completed at: {context.CompletedAtUtc}");

        // Store arbitrary data in the context
        context.Data["input"] = new { Param1 = "value1", Param2 = 42 };
        context.Data["timestamp"] = DateTime.UtcNow;

        // Complete the operation successfully
        context.CompleteSuccess(new { Result = "Processed successfully" });

        // Get execution summary
        var summary = context.GetSummary();
        Console.WriteLine($"Operation successful: {summary.IsSuccessful}");
        Console.WriteLine($"Plugin name: {summary.PluginName}");
        Console.WriteLine($"Duration: {context.Duration.TotalMilliseconds}ms");
        Console.WriteLine($"Summary: {summary}");

        // Access metrics
        context.Metrics.CustomMetrics["itemsProcessed"] = 100;
        Console.WriteLine($"Items processed: {context.Metrics.CustomMetrics["itemsProcessed"]}");
    }
}
```

## MarketplaceBrowserTests

The `MarketplaceBrowserTests` class contains unit tests for the `MarketplaceBrowserService` class, which provides functionality for browsing and searching the plugin marketplace. It tests various operations including retrieving categories, getting trending plugins, browsing specific categories, fetching featured plugins, and retrieving home page data with proper caching behavior.

Here's a realistic usage example leveraging its public members:

```csharp
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Marketplace;
using PluginEngine.Results;

public class MarketplaceBrowserTestsDemo
{
    private readonly Mock<IPluginMarketplaceService> _mockMarketplace = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly Mock<ILogger<MarketplaceBrowserService>> _mockLogger = new();

    public async Task DemonstrateMarketplaceBrowserTests()
    {
        // Create the service under test
        var service = new MarketplaceBrowserService(
            _mockMarketplace.Object,
            _cache,
            _mockLogger.Object
        );

        // Test GetCategoriesAsync - retrieves built-in categories
        var categoriesResult = await service.GetCategoriesAsync();
        if (categoriesResult.Success)
        {
            Console.WriteLine($"Found {categoriesResult.Data?.Count} categories");
            var loggingCategory = categoriesResult.Data?.FirstOrDefault(c => c.Id == "logging");
            Console.WriteLine($"Logging category exists: {loggingCategory != null}");
        }

        // Test GetCategoriesAsync caching - second call returns same data without hitting marketplace
        var cachedCategories = await service.GetCategoriesAsync();
        Console.WriteLine($"Categories cached: {ReferenceEquals(categoriesResult.Data, cachedCategories.Data)}");

        // Test GetTrendingAsync - gets trending plugins sorted by downloads
        var trendingResult = await service.GetTrendingAsync(10);
        if (trendingResult.Success)
        {
            Console.WriteLine($"Found {trendingResult.Data?.Count} trending plugins");
        }

        // Test GetTrendingAsync clamping - never exceeds 50 items
        var largeTrendingResult = await service.GetTrendingAsync(100);
        Console.WriteLine($"Trending limit clamped to 50: {largeTrendingResult.Data?.Count <= 50}");

        // Test BrowseCategoryAsync - filters plugins by category tag
        var loggingPluginsResult = await service.BrowseCategoryAsync("logging");
        if (loggingPluginsResult.Success)
        {
            Console.WriteLine($"Found {loggingPluginsResult.Data?.Count} logging plugins");
        }

        // Test BrowseCategoryAsync with empty category - returns failure
        var emptyCategoryResult = await service.BrowseCategoryAsync(string.Empty);
        Console.WriteLine($"Empty category returns failure: {(!emptyCategoryResult.Success && emptyCategoryResult.ErrorCode == 400)}");

        // Test GetFeaturedAsync - gets featured plugins that are verified and rated
        var featuredResult = await service.GetFeaturedAsync();
        if (featuredResult.Success)
        {
            Console.WriteLine($"Found {featuredResult.Data?.Count} featured plugins");
        }

        // Test GetHomePageAsync - returns aggregated home page data
        var homePageResult = await service.GetHomePageAsync();
        if (homePageResult.Success)
        {
            Console.WriteLine($"Home page has {homePageResult.Data?.Categories.Count} categories");
            Console.WriteLine($"Home page generated at: {homePageResult.Data?.GeneratedAtUtc}");
        }
    }
}
```

## IPluginFormatter

The `IPluginFormatter` interface defines the contract for formatting plugin data in various output formats. It provides methods for formatting individual plugins, collections of plugins, detailed reports, and health information. This interface enables consistent output formatting across different serialization formats like JSON, CSV, and XML.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Formatters;
using PluginEngine.Domain.Entities;
using System;
using System.Collections.Generic;

public class PluginFormatterDemo
{
    public void DemonstratePluginFormatter()
    {
        // Create a plugin instance
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "SamplePlugin",
            Status = PluginStatus.Loaded,
            DependencyCount = 3,
            CapabilityCount = 2,
            LoadTimeMs = 150,
            LastAccessedUtc = DateTime.UtcNow,
            IsHealthy = true,
            Issues = new List<string>()
        };

        // Create formatter factory
        var formatterFactory = new FormatterFactory(
            new JsonPluginFormatter(),
            new CsvPluginFormatter(),
            new XmlPluginFormatter()
        );

        // Get a formatter for JSON output
        var jsonFormatter = formatterFactory.GetFormatter("json");
        if (jsonFormatter != null)
        {
            string jsonOutput = jsonFormatter.FormatPluginAsync(plugin).Result;
            Console.WriteLine("JSON Output:");
            Console.WriteLine(jsonOutput);
        }

        // Get a formatter for CSV output
        var csvFormatter = formatterFactory.GetFormatter("csv");
        if (csvFormatter != null)
        {
            string csvOutput = csvFormatter.FormatPluginAsync(plugin).Result;
            Console.WriteLine("\nCSV Output:");
            Console.WriteLine(csvOutput);
        }

        // Get all supported formats
        var supportedFormats = formatterFactory.GetSupportedFormats();
        Console.WriteLine($"\nSupported formats: {string.Join(", ", supportedFormats)}");

        // Format a health report
        var healthInfo = new PluginHealthInfo
        {
            PluginId = plugin.Id,
            PluginName = plugin.Name,
            Status = plugin.Status.ToString(),
            DependencyCount = plugin.DependencyCount,
            CapabilityCount = plugin.CapabilityCount,
            LoadTimeMs = plugin.LoadTimeMs,
            LastAccessedUtc = plugin.LastAccessedUtc,
            IsHealthy = plugin.IsHealthy,
            Issues = plugin.Issues
        };

        string healthReport = formatterFactory.GetFormatter("json")?.FormatHealthReportAsync(healthInfo).Result;
        Console.WriteLine("\nHealth Report:");
        Console.WriteLine(healthReport);
    }
}
```

## MemoryPluginCache

[... existing content ...]
