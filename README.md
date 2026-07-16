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

## PluginEngineCoreTests

The `PluginEngineCoreTests` class contains unit tests for the core `PluginEngine` functionality. It tests constructor validation with all required services, initialization and shutdown sequences, plugin loading and unloading operations, status retrieval, and health reporting. The tests validate that the plugin engine properly delegates to its dependencies and handles error conditions appropriately.

Here's a realistic usage example leveraging its public members:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PluginEngine;
using PluginEngine.Configuration;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

public class PluginEngineCoreTestsDemo : IDisposable
{
    private readonly Mock<IPluginManagerService> _pluginManagerMock = new();
    private readonly Mock<IPluginLoaderService> _pluginLoaderMock = new();
    private readonly Mock<IDependencyResolutionService> _dependencyResolverMock = new();
    private readonly Mock<IVersioningService> _versioningMock = new();
    private readonly Mock<IHotReloadService> _hotReloadMock = new();
    private readonly PluginEngineOptions _options;
    private readonly string _testDirectory;

    public PluginEngineCoreTestsDemo()
    {
        // Setup test environment
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _options = new PluginEngineOptions { PluginDirectory = _testDirectory };

        // Configure mocks
        _pluginManagerMock
            .Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _pluginManagerMock
            .Setup(x => x.ShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _pluginManagerMock
            .Setup(x => x.GetStatusAsync())
            .ReturnsAsync(new PluginManagerStatus
            {
                IsInitialized = true,
                TotalPlugins = 5,
                LoadedPlugins = 3,
                ActivePlugins = 2,
                FailedPlugins = 1
            });

        _pluginManagerMock
            .Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(new PluginManagerStatistics
            {
                TotalPlugins = 5,
                LoadedPlugins = 3,
                ActivePlugins = 2,
                TotalMemoryUsageBytes = 1024 * 1024,
                AverageLoadTimeMs = 150.5
            });

        var plugins = new List<Plugin> {
            new Plugin { Id = Guid.NewGuid(), Name = "TestPlugin1", Status = PluginStatus.Loaded },
            new Plugin { Id = Guid.NewGuid(), Name = "TestPlugin2", Status = PluginStatus.Loaded }
        };

        _pluginLoaderMock
            .Setup(x => x.LoadPluginsFromDirectoryAsync(_options.PluginDirectory, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins);

        _pluginLoaderMock
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugins);

        _pluginLoaderMock
            .Setup(x => x.UnloadPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    public async Task DemonstratePluginEngineCoreTests()
    {
        // Create the plugin engine with mocked dependencies
        var engine = new PluginEngine.PluginEngine(
            _pluginManagerMock.Object,
            _pluginLoaderMock.Object,
            _dependencyResolverMock.Object,
            _versioningMock.Object,
            _hotReloadMock.Object,
            _options
        );

        // Access the configured services through properties
        var pluginManager = engine.PluginManager;
        var pluginLoader = engine.PluginLoader;
        var dependencyResolver = engine.DependencyResolver;
        var versioningService = engine.VersioningService;
        var hotReloader = engine.HotReloader;
        var options = engine.Options;

        Console.WriteLine("Plugin Engine services configured successfully");
        Console.WriteLine($"Plugin Manager: {pluginManager.GetType().Name}");
        Console.WriteLine($"Plugin Loader: {pluginLoader.GetType().Name}");

        // Test initialization
        await engine.InitializeAsync();
        Console.WriteLine("Plugin engine initialized");

        // Test status retrieval
        var status = await engine.GetStatusAsync();
        Console.WriteLine($"Status - Initialized: {status.IsInitialized}, Total Plugins: {status.TotalPlugins}");

        // Test loading plugins
        var loadedCount = await engine.LoadAllPluginsAsync();
        Console.WriteLine($"Loaded {loadedCount} plugins from directory");

        // Test getting health info
        var healthInfo = await engine.GetHealthInfoAsync();
        Console.WriteLine("Health report generated");

        // Test unloading all plugins
        await engine.UnloadAllPluginsAsync();
        Console.WriteLine("All plugins unloaded");

        // Test shutdown
        await engine.ShutdownAsync();
        Console.WriteLine("Plugin engine shut down");
    }
}
```

## HotSwapServiceTests

The `HotSwapServiceTests` class contains unit tests for the `HotSwapService` class, which provides functionality for hot-swapping plugin assemblies at runtime. It tests various operations including checking if a plugin can be swapped, performing plugin swaps with validation, rolling back swaps, managing swap history, and handling post-swap callbacks. The tests validate that the hot swap functionality properly handles different plugin states, validates inputs, and maintains swap history.

Here's a realistic usage example leveraging its public members:

```csharp
using FluentAssertions;

## FileSystemHelperTests

The `FileSystemHelperTests` class contains unit tests for the `FileSystemHelper` class, which provides utility methods for file system operations commonly used in plugin processing. It tests directory creation, file discovery, file operations, directory size calculations, and recursive directory deletion. These tests validate that file system operations handle various edge cases including invalid paths, non-existent files, and permission scenarios.

Here's a realistic usage example leveraging its public members:

```csharp
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Utils.Helpers;
using System;
using System.IO;

public class FileSystemHelperTestsDemo : IDisposable
{
    private readonly Mock<ILogger<FileSystemHelper>> _mockLogger = new();
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly string _testDirectory;
    private readonly string _testFile;

    public FileSystemHelperTestsDemo()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testFile = Path.Combine(_testDirectory, "test-plugin.dll");
        
        _fileSystemHelper = new FileSystemHelper(_mockLogger.Object);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    public void DemonstrateFileSystemHelperTests()
    {
        // Test ensuring directory exists
        bool directoryCreated = _fileSystemHelper.EnsureDirectoryExists(_testDirectory);
        Console.WriteLine($"Directory created: {directoryCreated}");
        
        // Test discovering plugins in directory
        var plugins = _fileSystemHelper.DiscoverPlugins(_testDirectory);
        Console.WriteLine($"Plugins discovered: {plugins.Count}");
        
        // Create a test file
        Directory.CreateDirectory(_testDirectory);
        File.WriteAllText(_testFile, "test content");
        
        // Test getting file info
        var fileInfo = _fileSystemHelper.GetFileInfo(_testFile);
        if (fileInfo != null)
        {
            Console.WriteLine($"File size: {fileInfo.Size} bytes");
            Console.WriteLine($"Modified time: {fileInfo.ModifiedTime}");
        }
        
        // Test safe file copy
        string destinationFile = Path.Combine(_testDirectory, "copied-plugin.dll");
        bool copied = _fileSystemHelper.SafeCopyFile(_testFile, destinationFile, overwrite: true);
        Console.WriteLine($"File copied successfully: {copied}");
        
        // Test directory size calculation
        long directorySize = _fileSystemHelper.GetDirectorySize(_testDirectory);
        Console.WriteLine($"Directory size: {directorySize} bytes");
        
        // Test recursive directory deletion
        bool deleted = _fileSystemHelper.DeleteDirectoryRecursive(_testDirectory);
        Console.WriteLine($"Directory deleted: {deleted}");
    }
}
```

## HotSwapServiceTests

The `HotSwapServiceTests` class contains unit tests for the `HotSwapService` class, which provides functionality for hot-swapping plugin assemblies at runtime. It tests various operations including checking if a plugin can be swapped, performing plugin swaps with validation, rolling back swaps, managing swap history, and handling post-swap callbacks. The tests validate that the hot swap functionality properly handles different plugin states, validates inputs, and maintains swap history.

Here's a realistic usage example leveraging its public members:

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;
using Xunit;

public class HotSwapServiceTestsDemo : IDisposable
{
    private readonly Mock<IPluginLoaderService> _mockLoader = new();
    private readonly Mock<ILogger<HotSwapService>> _mockLogger = new();
    private readonly string _tempDir;
    private readonly HotSwapService _service;

    public HotSwapServiceTestsDemo()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        
        _service = new HotSwapService(_mockLoader.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string CreateTempDll(string name = "plugin.dll")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllBytes(path, new byte[] { 0x4D, 0x5A }); // minimal PE header stub
        return path;
    }

    private Plugin MakePlugin(PluginStatus status = PluginStatus.Active, string? assemblyPath = null)
    {
        return new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "TestPlugin",
            Version = "1.0.0",
            AssemblyPath = assemblyPath ?? CreateTempDll("current.dll"),
            Status = status
        };
    }

    [Fact]
    public void CanSwap_ActiveOrLoadedPlugin_ReturnsTrue()
    {
        // Arrange
        var plugin = MakePlugin(PluginStatus.Active);
        
        // Act
        var result = _service.CanSwap(plugin);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SwapPluginAsync_Success_RecordsSwapHistory()
    {
        // Arrange
        var plugin = MakePlugin();
        var newDll = CreateTempDll("new.dll");
        var newPlugin = MakePlugin(assemblyPath: newDll);
        
        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(newDll, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlugin);

        // Act
        var swapResult = await _service.SwapPluginAsync(plugin.Id, newDll);
        var historyResult = await _service.GetSwapHistoryAsync(plugin.Id);
        
        // Assert
        swapResult.Success.Should().BeTrue();
        historyResult.Data.Should().HaveCount(1);
        historyResult.Data![0].NewAssemblyPath.Should().Be(newDll);
        historyResult.Data[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task RollbackSwapAsync_AfterSuccessfulSwap_ReloadsOldAssembly()
    {
        // Arrange
        var oldDll = CreateTempDll("old.dll");
        var newDll = CreateTempDll("new.dll");
        var plugin = MakePlugin(assemblyPath: oldDll);
        var newPlugin = MakePlugin(assemblyPath: newDll);
        
        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.UnloadPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(newDll, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlugin);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(oldDll, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);

        // Act
        await _service.SwapPluginAsync(plugin.Id, newDll);
        var rollbackResult = await _service.RollbackSwapAsync(plugin.Id);
        
        // Assert
        rollbackResult.Success.Should().BeTrue();
    }

    [Fact]
    public void UnregisterPostSwapCallback_RemovesCallback()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        
        // Act
        _service.RegisterPostSwapCallback(pluginId, _ => Task.CompletedTask);
        _service.UnregisterPostSwapCallback(pluginId);
        
        // Assert - no exception thrown
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

## JsonPluginFormatterTests

The `JsonPluginFormatterTests` class contains unit tests for the `JsonPluginFormatter` class, which provides JSON serialization functionality for plugin data. It tests various formatting scenarios including individual plugin formatting, collections of plugins, detailed reports with metadata, and health reports. The tests validate that the JSON output is properly structured, contains expected fields, and maintains data integrity.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Formatters;
using PluginEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class JsonPluginFormatterDemo
{
    public async Task DemonstrateJsonPluginFormatter()
    {
        // Create a plugin instance
        var pluginId = Guid.NewGuid();
        var plugin = new Plugin
        {
            Id = pluginId,
            Name = "SamplePlugin",
            Version = "1.2.3",
            Status = PluginStatus.Loaded,
            DependencyCount = 2,
            CapabilityCount = 3,
            LoadTimeMs = 125,
            LastAccessedUtc = DateTime.UtcNow.AddMinutes(-5),
            IsHealthy = true,
            Issues = new List<string> { "Minor issue detected" }
        };

        // Create dependencies
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = Guid.NewGuid(),
            MinimumVersion = "1.0.0"
        });
        
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = Guid.NewGuid(),
            MinimumVersion = "2.0.0"
        });

        // Create capabilities
        plugin.AddCapability(new PluginCapability
        {
            PluginId = pluginId,
            Name = "DataTransform",
            Version = "1.0.0",
            InterfaceTypeName = "ITransform",
            Tags = "data,transform"
        });

        plugin.AddCapability(new PluginCapability
        {
            PluginId = pluginId,
            Name = "DataValidation",
            Version = "1.1.0",
            InterfaceTypeName = "IValidator",
            Tags = "validation"
        });

        // Create the JSON formatter
        var jsonFormatter = new JsonPluginFormatter();

        // Test FormatPluginAsync - formats a single plugin as JSON
        string pluginJson = await jsonFormatter.FormatPluginAsync(plugin);
        Console.WriteLine("Single Plugin JSON:");
        Console.WriteLine(pluginJson);

        // Test FormatPluginsAsync - formats a collection of plugins
        var plugins = new List<Plugin> { plugin };
        string pluginsJson = await jsonFormatter.FormatPluginsAsync(plugins);
        Console.WriteLine("\nMultiple Plugins JSON:");
        Console.WriteLine(pluginsJson);

        // Test FormatDetailedReportAsync - formats a detailed report with metadata
        var metadata = new Dictionary<string, object>
        {
            { "author", "Sample Author" },
            { "description", "A sample plugin for demonstration purposes" },
            { "license", "MIT" },
            { "repository", "https://github.com/sample/sample-plugin" }
        };
        
        string detailedReportJson = await jsonFormatter.FormatDetailedReportAsync(plugins, metadata);
        Console.WriteLine("\nDetailed Report JSON:");
        Console.WriteLine(detailedReportJson);

        // Test FormatHealthReportAsync - formats health information
        var healthInfo = new PluginHealthInfo
        {
            PluginId = pluginId,
            PluginName = plugin.Name,
            Status = plugin.Status.ToString(),
            DependencyCount = plugin.DependencyCount,
            CapabilityCount = plugin.CapabilityCount,
            LoadTimeMs = plugin.LoadTimeMs,
            LastAccessedUtc = plugin.LastAccessedUtc,
            IsHealthy = plugin.IsHealthy,
            Issues = plugin.Issues
        };
        
        string healthReportJson = await jsonFormatter.FormatHealthReportAsync(healthInfo);
        Console.WriteLine("\nHealth Report JSON:");
        Console.WriteLine(healthReportJson);

        // Verify the JSON is valid by parsing it back
        var parsedPlugin = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(pluginJson);
        Console.WriteLine($"\nPlugin name in JSON: {parsedPlugin?["name"]}");
        Console.WriteLine($"Plugin version in JSON: {parsedPlugin?["version"]}");
        Console.WriteLine($"Plugin has {parsedPlugin?.Count} top-level properties");
    }
}
```

## MemoryPluginCache

[... existing content ...]
