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
```

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
```

## HotSwapServiceTests

The `HotSwapServiceTests` class contains unit tests for the `HotSwapService` class, which provides functionality for hot-swapping plugin assemblies at runtime. It tests various operations including checking if a plugin can be swapped, performing plugin swaps with validation, rolling back swaps, managing swap history, and handling post-swap callbacks. The tests validate that the hot swap functionality properly handles different plugin states, validates inputs, and maintains swap history.

## VersionHelperTests

The `VersionHelperTests` class contains unit tests for the `VersionHelper` utility class, which provides functionality for parsing, comparing, and validating semantic version strings. It tests various version operations including parsing versions with and without version prefixes, handling invalid version strings, comparing version numbers, and validating version constraints using semantic versioning rules like caret (`^`) and tilde (`~`) operators.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Helpers;
using System;

public class VersionHelperDemo
{
    public void DemonstrateVersionHelperTests()
    {
        // Parse version strings with different formats
        var version1 = VersionHelper.ParseVersion("v1.2.3");
        Console.WriteLine($"Parsed v1.2.3: {version1}");
        
        var version2 = VersionHelper.ParseVersion("2.5.1-beta");
        Console.WriteLine($"Parsed 2.5.1-beta: {version2}");
        
        var version3 = VersionHelper.ParseVersion("  3.0.0  ");
        Console.WriteLine($"Parsed '  3.0.0  ': {version3}");
        
        var version4 = VersionHelper.ParseVersion("invalid");
        Console.WriteLine($"Parsed 'invalid': {version4}");
        
        // Compare versions
        var comparison1 = VersionHelper.CompareVersions("2.0.0", "1.9.9");
        Console.WriteLine($"Compare 2.0.0 vs 1.9.9: {comparison1} (positive means first is greater)");
        
        var comparison2 = VersionHelper.CompareVersions("1.5.0", "1.5.0");
        Console.WriteLine($"Compare 1.5.0 vs 1.5.0: {comparison2} (zero means equal)");
        
        // Check version constraints
        var satisfies1 = VersionHelper.SatisfiesConstraint("2.0.0", ">=1.5.0");
        Console.WriteLine($"Does 2.0.0 satisfy >=1.5.0: {satisfies1}");
        
        var satisfies2 = VersionHelper.SatisfiesConstraint("1.0.0", ">=1.5.0");
        Console.WriteLine($"Does 1.0.0 satisfy >=1.5.0: {satisfies2}");
        
        // Caret operator (^1.2.3) - allows patch and minor updates, but not major
        var caret1 = VersionHelper.SatisfiesConstraint("1.3.0", "^1.2.3");
        Console.WriteLine($"Does 1.3.0 satisfy ^1.2.3: {caret1}");
        
        var caret2 = VersionHelper.SatisfiesConstraint("2.0.0", "^1.2.3");
        Console.WriteLine($"Does 2.0.0 satisfy ^1.2.3: {caret2}");
        
        // Tilde operator (~1.2.3) - allows patch updates only
        var tilde1 = VersionHelper.SatisfiesConstraint("1.2.4", "~1.2.3");
        Console.WriteLine($"Does 1.2.4 satisfy ~1.2.3: {tilde1}");
        
        var tilde2 = VersionHelper.SatisfiesConstraint("1.3.0", "~1.2.3");
        Console.WriteLine($"Does 1.3.0 satisfy ~1.2.3: {tilde2}");
        
        // Get latest version from a list
        var latest = VersionHelper.GetLatestVersion(new[] { "1.0.0", "2.3.1", "1.9.9", "2.0.0-alpha" });
        Console.WriteLine($"Latest version from list: {latest}");
        
        // Check if a string is a valid semantic version
        var isValid1 = VersionHelper.IsValidSemanticVersion("1.2.3");
        Console.WriteLine($"Is '1.2.3' a valid semantic version: {isValid1}");
        
        var isValid2 = VersionHelper.IsValidSemanticVersion("1.2.3-beta.1+build.123");
        Console.WriteLine($"Is '1.2.3-beta.1+build.123' a valid semantic version: {isValid2}");
        
        var isValid3 = VersionHelper.IsValidSemanticVersion("not-a-version");
        Console.WriteLine($"Is 'not-a-version' a valid semantic version: {isValid3}");
        
        // Get version info for prerelease versions
        var info = VersionHelper.GetVersionInfo("1.0.0-alpha.1");
        Console.WriteLine($"Version 1.0.0-alpha.1 is prerelease: {info.IsPrerelease}");
        Console.WriteLine($"Core version: {info.CoreVersion}");
    }
}
```

## StringExtensionsTests

The `StringExtensionsTests` class contains unit tests for string extension methods in the `PluginEngine.Utils.Extensions` namespace. It tests various string manipulation and validation utilities used for plugin path handling, version validation, filename sanitization, and time/byte formatting. The tests validate that string operations properly handle edge cases and maintain expected behavior across different input scenarios.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Utils.Extensions;
using System;

public class StringExtensionsDemo
{
    public void DemonstrateStringExtensions()
    {
        // Normalize plugin paths - convert forward slashes and remove trailing separators
        var normalizedPath = "plugins/auth/MyPlugin.dll".NormalizePluginPath();
        Console.WriteLine($"Normalized path: {normalizedPath}");
        
        // Validate plugin IDs (GUID strings)
        var isValidPluginId = "550e8400-e29b-41d4-a716-446655440000".IsValidPluginId();
        Console.WriteLine($"Is valid plugin ID: {isValidPluginId}");
        
        // Validate semantic versions
        var isValidVersion = "2.1.0".IsValidVersion();
        Console.WriteLine($"Is valid version: {isValidVersion}");
        
        // Sanitize strings for filenames
        var sanitizedName = "My_Plugin-Name.txt".SanitizeForFilename();
        Console.WriteLine($"Sanitized filename: {sanitizedName}");
        
        // Extract assembly names from paths
        var assemblyName = "/opt/plugins/MyPlugin.dll".GetAssemblyName();
        Console.WriteLine($"Assembly name: {assemblyName}");
        
        // Check if paths are assembly files
        var isAssembly = "/plugins/Tool.exe".IsAssemblyPath();
        Console.WriteLine($"Is assembly path: {isAssembly}");
        
        // Truncate strings with ellipsis
        var truncated = "This is a very long string that needs truncation".TruncateWithEllipsis(20);
        Console.WriteLine($"Truncated: {truncated}");
    }
}
```

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

## DependencyGraphAnalyzerTests

`DependencyGraphAnalyzerTests` provides a comprehensive test suite for the `DependencyGraphAnalyzer` service, verifying its ability to locate dependent plugins, analyze dependency graphs, generate visualizations, and assess complexity levels. The tests cover scenarios ranging from empty dependency sets to circular dependencies and ensure the analyzer reports accurate metrics and labels.

Below is a realistic usage example that demonstrates how the test class can be instantiated and its public test methods invoked directly. This example assumes the test project references the required packages (`Moq`, `FluentAssertions`, `Microsoft.Extensions.Logging`) and runs in an async‑compatible context.

```csharp
using System;
using System.Threading.Tasks;

public class DependencyGraphAnalyzerDemo
{
    public static async Task Main()
    {
        // Create an instance of the test class
        var tests = new DependencyGraphAnalyzerTests();

        // Execute the async test methods
        await tests.FindDependentsAsync_WithNoDependents_ReturnsEmpty();
        await tests.FindDependentsAsync_WithOneDependentPlugin_ReturnsThatPlugin();
        await tests.FindDependentsAsync_WithMultipleDependents_ReturnsAll();
        await tests.FindDependentsAsync_WithEmptyPluginList_ReturnsEmpty();

        await tests.AnalyzeAsync_WithNoDependencies_ReturnsZeroCounts();
        await tests.AnalyzeAsync_WithCircularDependency_ReportsIssue();
        await tests.AnalyzeAsync_WithFewDependencies_HasSimpleComplexityLevel();
        await tests.AnalyzeAsync_ReturnsPluginName();

        await tests.GenerateGraphVisualizationAsync_WithSimplePlugin_ReturnsNonEmptyString();

        // Execute the synchronous test method
        tests.GetComplexityLevel_MapsScoreToExpectedLabel();

        Console.WriteLine("All DependencyGraphAnalyzerTests executed successfully.");
    }
}
```

This demo showcases how each test method can be called programmatically, allowing developers to reuse the verification logic outside of a traditional test runner when needed.