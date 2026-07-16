# dotnet-plugin-engine

[... existing content ...]

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

## PluginEntityTests

The `PluginEntityTests` class contains unit tests for the core plugin entities (`Plugin`, `PluginDependency`, and `PluginCapability`) that form the foundation of the plugin system. It tests various operations including dependency management, capability handling, validation logic, and version constraint validation. These tests validate that plugin entities properly handle their required fields, maintain data integrity, and provide appropriate error messages when validation fails.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Domain.Entities;
using System;

public class PluginEntityTestsDemo
{
    public void DemonstratePluginEntityTests()
    {
        // Create a valid plugin instance with required fields
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "DataProcessingPlugin",
            Version = "2.1.0",
            AssemblyPath = "/plugins/DataProcessingPlugin.dll"
        };

        Console.WriteLine($"Created plugin: {plugin.Name} v{plugin.Version}");
        Console.WriteLine($"Plugin is valid: {plugin.IsValid()}");

        // Add a dependency to another plugin
        var dependencyPluginId = Guid.NewGuid();
        plugin.AddDependency(new PluginDependency
        {
            PluginId = plugin.Id,
            DependencyPluginId = dependencyPluginId,
            MinimumVersion = "1.5.0",
            MaximumVersion = "3.0.0"
        });

        Console.WriteLine($"Added dependency: {dependencyPluginId}");
        Console.WriteLine($"Dependency count: {plugin.Dependencies.Count}");

        // Add capabilities to the plugin
        plugin.AddCapability(new PluginCapability
        {
            PluginId = plugin.Id,
            Name = "DataTransform",
            Version = "2.1.0",
            InterfaceTypeName = "IDataTransformer",
            Tags = "data,transform,csv"
        });

        plugin.AddCapability(new PluginCapability
        {
            PluginId = plugin.Id,
            Name = "DataValidation",
            Version = "1.2.0",
            InterfaceTypeName = "IDataValidator",
            Tags = "validation,data"
        });

        Console.WriteLine($"Added {plugin.Capabilities.Count} capabilities");

        // Check if plugin has specific capabilities
        var hasTransform = plugin.Capabilities.Any(c => c.HasTag("transform"));
        Console.WriteLine($"Has transform capability: {hasTransform}");

        var hasEncryption = plugin.Capabilities.Any(c => c.HasTag("encryption"));
        Console.WriteLine($"Has encryption capability: {hasEncryption}");

        // Test dependency version satisfaction
        var dependency = plugin.Dependencies.First();
        bool versionSatisfied = dependency.IsSatisfiedBy("2.0.0");
        Console.WriteLine($"Dependency version 2.0.0 satisfied: {versionSatisfied}");

        bool versionTooOld = dependency.IsSatisfiedBy("1.0.0");
        Console.WriteLine($"Dependency version 1.0.0 satisfied: {versionTooOld}");

        bool versionTooNew = dependency.IsSatisfiedBy("3.5.0");
        Console.WriteLine($"Dependency version 3.5.0 satisfied: {versionTooNew}");

        // Get version constraint string
        string constraint = dependency.GetVersionConstraint();
        Console.WriteLine($"Version constraint: {constraint}");

        // Test validation error messages
        var invalidPlugin = new Plugin
        {
            Id = Guid.Empty, // Invalid - empty GUID
            Name = "", // Invalid - empty name
            Version = "1.0.0",
            AssemblyPath = string.Empty // Invalid - empty assembly path
        };

        string validationError = invalidPlugin.GetValidationError();
        Console.WriteLine($"Validation error: {validationError}");

        // Remove a dependency
        bool removed = plugin.RemoveDependency(dependency.Id);
        Console.WriteLine($"Dependency removed successfully: {removed}");
        Console.WriteLine($"Remaining dependencies: {plugin.Dependencies.Count}");
    }
}

## HotSwapServiceTests

The `HotSwapServiceTests` class contains unit tests for the `HotSwapService` class, which provides functionality for hot-swapping plugin assemblies at runtime. It tests various operations including checking if a plugin can be swapped, performing plugin swaps with validation, rolling back swaps, managing swap history, and handling post-swap callbacks. The tests validate that the hot swap functionality properly handles different plugin states, validates inputs, and maintains swap history.

## PluginValidatorTests

The `PluginValidatorTests` class contains unit tests for the `PluginValidator` class, which is responsible for validating plugin entities. It tests various validation scenarios, including plugin name validation, version validation, metadata validation, and dependency validation. Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Tests;
using Xunit;

public class PluginValidatorTestsDemo
{
    public void DemonstratePluginValidatorTests()
    {
        var tests = new PluginValidatorTests();

        tests.Validate_WithValidPlugin_ReturnsValidResult();
        tests.Validate_WithEmptyName_ReturnsInvalidWithError();
        tests.Validate_WithWhitespaceName_ReturnsInvalidWithError();
        tests.Validate_WithNameStartingWithSystemPrefix_ReturnsInvalidWithError();
        tests.Validate_WithNameStartingWithMicrosoftPrefix_ReturnsInvalidWithError();
        tests.Validate_WithNameExceedingMaxLength_ReturnsInvalidWithError();
        tests.Validate_WithNameStartingWithSpecialCharacter_ReturnsInvalidWithError();
        tests.Validate_WithNameContainingInvalidCharacters_ReturnsInvalidWithError();
        tests.Validate_WithValidNames_ReturnsValidResult();
        tests.Validate_WithEmptyVersion_ReturnsInvalidWithError();
        tests.Validate_WithInvalidVersionFormat_ReturnsInvalidWithError();
        tests.Validate_WithValidVersions_ReturnsValidResult();
        tests.Validate_WithValidMetadata_ReturnsValidResult();
        tests.Validate_WithEmptyMetadataAuthor_ReturnsInvalidWithError();
        tests.Validate_WithValidDependencies_ReturnsValidResult();
        tests.Validate_WithInvalidDependencyMinimumVersion_ReturnsInvalidWithError();
        tests.Validate_WithDependencyMaximumVersionLowerThanMinimum_ReturnsInvalidWithError();
        tests.Validate_WithMultipleErrors_ReturnsAllErrorMessages();
        tests.Validate_IncludesPluginIdAndNameInResult();
    }
}
```