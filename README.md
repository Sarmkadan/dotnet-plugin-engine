# dotnet-plugin-engine

[... existing content ...]

## MemoryPluginCache

The `MemoryPluginCache` class provides an in-memory cache implementation for plugin data, supporting automatic expiration and eviction. It stores data in memory using .NET's `IMemoryCache` and provides thread-safe operations for getting, setting, removing, and clearing cache entries.

Here's a realistic usage example leveraging its public members:

```csharp
using PluginEngine.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class CacheDemo
{
    private readonly MemoryPluginCache _cache;
    private readonly ILogger<CacheDemo> _logger;

    public CacheDemo(MemoryPluginCache cache, ILogger<CacheDemo> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // Get a value from cache
        var cachedValue = await _cache.GetAsync<string>("my-key");
        _logger.LogInformation($"Cached value: {cachedValue}");

        // Set a value in cache with expiration
        await _cache.SetAsync("my-key", "Hello, World!", TimeSpan.FromMinutes(30));

        // Remove a value from cache
        await _cache.RemoveAsync("my-key");

        // Clear entire cache
        await _cache.ClearAsync();

        // Get cache statistics
        var stats = await _cache.GetStatisticsAsync();
        _logger.LogInformation($"Cache hits: {stats.TotalHits}, misses: {stats.TotalMisses}, entries: {stats.CurrentEntries}");
    }
}
```

The `MemoryPluginCache` ensures efficient data caching for plugins with features like automatic expiration, sliding expiration, and detailed performance statistics.

```csharp

```

## VersionHelper

The `VersionHelper` class provides utilities for parsing, comparing, and validating semantic version strings in the plugin system. It supports common version formats, constraint checking, and provides detailed version information extraction with built-in logging through `ILogger<VersionHelper>`.


Here's a realistic usage example leveraging its public members:

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PluginEngine.Utils.Helpers;

public class VersionManagement
{
    private readonly VersionHelper _versionHelper;
    private readonly ILogger<VersionManagement> _logger;

    public VersionManagement(VersionHelper versionHelper, ILogger<VersionManagement> logger)
    {
        _versionHelper = versionHelper;
        _logger = logger;
    }

    public void ManagePluginVersions()
    {
        // Parse version strings
        var version1 = _versionHelper.ParseVersion("2.1.0");
        var version2 = _versionHelper.ParseVersion("v1.5.3");
        var version3 = _versionHelper.ParseVersion("3.0.0-beta");
        
        _logger.LogInformation("Parsed versions: {v1}, {v2}, {v3}", 
            version1?.ToString(), version2?.ToString(), version3?.ToString());

        // Compare versions
        int comparison = _versionHelper.CompareVersions("2.1.0", "1.5.3");
        _logger.LogInformation("Comparison result: {Result}", comparison > 0 ? "v2.1.0 is newer" : 
            comparison < 0 ? "v1.5.3 is newer" : "versions are equal");

        // Check version constraints
        bool satisfies = _versionHelper.SatisfiesConstraint("2.1.0", ">=2.0.0");
        _logger.LogInformation("Version 2.1.0 satisfies >=2.0.0: {Result}", satisfies);

        bool prereleaseCheck = _versionHelper.SatisfiesConstraint("3.0.0-beta", "^3.0.0");
        _logger.LogInformation("Beta version satisfies ^3.0.0: {Result}", prereleaseCheck);

        // Get latest version from a list
        var versions = new List<string> { "1.2.0", "1.10.0", "2.0.0-alpha", "1.9.9" };
        string? latest = _versionHelper.GetLatestVersion(versions);
        _logger.LogInformation("Latest version: {Latest}", latest);

        // Get detailed version information
        var info = _versionHelper.GetVersionInfo("3.2.1-beta.1");
        _logger.LogInformation("Version info for {Original}: Major={Major}, Minor={Minor}, Patch={Patch}, " +
            "IsPrerelease={IsPrerelease}, IsStable={IsStable}",
            info.Original, info.Major, info.Minor, info.Patch, info.IsPrerelease, info.IsStable);

        // Validate semantic version format
        bool isValid = _versionHelper.IsValidSemanticVersion("1.2.3");
        _logger.LogInformation("Is '1.2.3' a valid semantic version: {Result}", isValid);
        
        bool isInvalid = _versionHelper.IsValidSemanticVersion("not-a-version");
        _logger.LogInformation("Is 'not-a-version' valid: {Result}", isInvalid);
    }
}
```

## PluginDiscoveryService

The `PluginDiscoveryService` class provides functionality for discovering and inspecting plugin assemblies on the file system. It scans directories for valid plugin candidates, extracts metadata from assemblies, and provides filtering and statistics capabilities. The service integrates with `FileSystemHelper` for file operations and `VersionHelper` for version validation.


Here's a realistic usage example leveraging its public members:

```csharp
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PluginEngine.Utils.Helpers;

public class PluginDiscoveryExample
{
    private readonly PluginDiscoveryService _discoveryService;
    private readonly ILogger<PluginDiscoveryExample> _logger;

    public PluginDiscoveryExample(
        PluginDiscoveryService discoveryService,
        ILogger<PluginDiscoveryExample> logger)
    {
        _discoveryService = discoveryService;
        _logger = logger;
    }

    public async Task DiscoverAndAnalyzePluginsAsync(string pluginDirectory)
    {
        // Discover all plugins in the directory
        var candidates = await _discoveryService.DiscoverPluginsAsync(pluginDirectory);
        _logger.LogInformation("Discovered {Count} plugin candidates", candidates.Count);

        // Filter plugins to only valid ones
        var validPlugins = _discoveryService.FilterPlugins(candidates, new PluginDiscoveryFilter
        {
            ValidOnly = true,
            MinimumVersionInfo = "2.0.0"
        });
        _logger.LogInformation("Found {Count} valid plugins", validPlugins.Count);

        // Inspect a specific plugin file
        var specificPluginPath = Path.Combine(pluginDirectory, "MyPlugin.dll");
        var specificCandidate = await _discoveryService.InspectPluginAsync(specificPluginPath);
        
        if (specificCandidate is not null)
        {
            _logger.LogInformation("Plugin Details:");
            _logger.LogInformation("- File: {FileName}", specificCandidate.FileName);
            _logger.LogInformation("- Assembly: {AssemblyName}", specificCandidate.AssemblyName);
            _logger.LogInformation("- Size: {Size} bytes", specificCandidate.FileSize);
            _logger.LogInformation("- Modified: {Modified}", specificCandidate.ModifiedAtUtc);
            _logger.LogInformation("- Valid: {IsValid}", specificCandidate.IsValid);
            
            if (specificCandidate.Version is not null)
            {
                _logger.LogInformation("- Version: {Version}", specificCandidate.Version);
            }
            
            if (specificCandidate.ProductName is not null)
            {
                _logger.LogInformation("- Product: {Product}", specificCandidate.ProductName);
            }
            
            if (specificCandidate.Company is not null)
            {
                _logger.LogInformation("- Company: {Company}", specificCandidate.Company);
            }
            
            if (specificCandidate.Description is not null)
            {
                _logger.LogInformation("- Description: {Description}", specificCandidate.Description);
            }
        }

        // Get discovery statistics
        var stats = _discoveryService.GetStatistics(candidates);
        _logger.LogInformation("Discovery Statistics:");
        _logger.LogInformation("- Total Candidates: {Total}", stats.TotalCandidates);
        _logger.LogInformation("- Valid Plugins: {Valid}", stats.ValidPlugins);
        _logger.LogInformation("- Invalid Plugins: {Invalid}", stats.InvalidPlugins);
        _logger.LogInformation("- Valid Percentage: {Percentage:F2}%", stats.ValidPercentage);
        _logger.LogInformation("- Total Size: {Size} bytes", stats.TotalSizeBytes);
    }
}
```

## FileSystemHelper

The `FileSystemHelper` class provides utilities for file system operations related to plugins. It abstracts directory management, file discovery, backup creation, and cleanup operations with built-in error handling and logging. The class is designed to handle cross-platform file system operations safely and provides detailed logging through `ILogger<FileSystemHelper>`.




Here's a realistic usage example leveraging its public members:

```csharp
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PluginEngine.Utils.Helpers;

public class PluginFileOperations
{
    private readonly FileSystemHelper _fileSystem;
    private readonly ILogger<PluginFileOperations> _logger;

    public PluginFileOperations(FileSystemHelper fileSystem, ILogger<PluginFileOperations> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public void SetupPluginEnvironment(string pluginDirectory)
    {
        // Ensure plugin directory exists
        bool directoryCreated = _fileSystem.EnsureDirectoryExists(pluginDirectory);
        _logger.LogInformation("Directory ensured: {Directory} - {Result}", 
            pluginDirectory, directoryCreated ? "Success" : "Failed");

        // Check if directory is writable before proceeding
        bool isWritable = _fileSystem.IsDirectoryWritable(pluginDirectory);
        _logger.LogInformation("Directory writable: {Writable}", isWritable);

        // Discover all plugin assemblies in the directory
        var pluginFiles = _fileSystem.DiscoverPlugins(pluginDirectory).ToList();
        _logger.LogInformation("Found {Count} plugin assemblies", pluginFiles.Count);
        
        foreach (var pluginFile in pluginFiles)
        {
            // Get file information
            var fileInfo = _fileSystem.GetFileInfo(pluginFile);
            if (fileInfo.HasValue)
            {
                _logger.LogInformation("Plugin: {File} - Size: {Size} bytes, Modified: {Modified}",
                    Path.GetFileName(pluginFile), fileInfo.Value.Size, fileInfo.Value.Modified);
            }
        }
        
        // Calculate total size of plugin directory
        long directorySize = _fileSystem.GetDirectorySize(pluginDirectory);
        _logger.LogInformation("Total plugin directory size: {Size} bytes", directorySize);
    }

    public bool DeployPlugin(string sourcePluginPath, string targetDirectory)
    {
        // Ensure target directory exists
        if (!_fileSystem.EnsureDirectoryExists(targetDirectory))
        {
            _logger.LogError("Failed to create target directory: {Target}", targetDirectory);
            return false;
        }

        // Create backup of existing plugin if it exists
        var backupPath = _fileSystem.CreateBackup(sourcePluginPath);
        if (backupPath != null)
        {
            _logger.LogInformation("Created backup: {Backup}", backupPath);
        }

        // Safely copy the plugin file
        string targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourcePluginPath));
        bool copySuccess = _fileSystem.SafeCopyFile(sourcePluginPath, targetPath, overwrite: true);
        
        if (copySuccess)
        {
            _logger.LogInformation("Plugin successfully deployed to: {Target}", targetPath);
            return true;
        }

        _logger.LogError("Failed to deploy plugin");
        return false;
    }

    public void CleanupOldPlugins(string pluginDirectory)
    {
        // Recursively delete old plugin cache
        bool deleted = _fileSystem.DeleteDirectoryRecursive(pluginDirectory);
        _logger.LogInformation("Cleanup completed: {Result}", deleted ? "Success" : "Failed");
    }
}
```

## PluginValidator

The `PluginValidator` class provides comprehensive validation for plugin metadata, versioning, and dependency relationships. It enforces naming conventions, semantic versioning rules, and dependency constraints while generating detailed validation reports with specific error messages.

Here's a realistic usage example leveraging its public members:

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PluginEngine;
using PluginEngine.Utils.Validators;

public class PluginValidationExample
{
    private readonly PluginValidator _validator;
    private readonly ILogger<PluginValidationExample> _logger;

    public PluginValidationExample(PluginValidator validator, ILogger<PluginValidationExample> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public void ValidatePluginMetadata(Plugin plugin)
    {
        // Validate plugin metadata and dependencies
        var validationResult = _validator.Validate(plugin);
        
        _logger.LogInformation("Validating plugin: {PluginName} ({PluginId})", 
            validationResult.PluginName, validationResult.PluginId);
        
        if (validationResult.IsValid)
        {
            _logger.LogInformation("✓ Plugin validation passed");
        }
        else
        {
            _logger.LogWarning("✗ Plugin validation failed with {Count} errors", 
                validationResult.Errors.Count);
            _logger.LogWarning("Error summary:\n{Summary}", validationResult.GetErrorSummary());
        }
    }

    public bool ValidateDependency(Plugin dependentPlugin, Plugin dependencyPlugin, 
        PluginDependency dependencySpec)
    {
        // Validate a specific dependency relationship
        bool isValid = _validator.ValidateDependencyRelationship(
            dependentPlugin, dependencyPlugin, dependencySpec);
        
        if (isValid)
        {
            _logger.LogInformation("✓ Dependency relationship is valid: {Dependent} -> {Dependency}",
                dependentPlugin.Name, dependencyPlugin.Name);
        }
        else
        {
            _logger.LogError("✗ Dependency relationship failed validation");
        }
        
        return isValid;
    }

    public void ValidateMultiplePlugins(IEnumerable<Plugin> plugins)
    {
        // Batch validate multiple plugins
        foreach (var plugin in plugins)
        {
            var result = _validator.Validate(plugin);
            
            Console.WriteLine($"Plugin: {result.PluginName}");
            Console.WriteLine($"  Valid: {result.IsValid}");
            Console.WriteLine($"  Errors: {result.Errors.Count}");
            
            if (!result.IsValid)
            {
                Console.WriteLine("  Error details:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"    - {error}");
                }
            }
            Console.WriteLine();
        }
    }
}
```

## DependencyGraphAnalyzer

`DependencyGraphAnalyzer` helps visualise and analyse plugin dependency graphs. It can generate a textual graph representation, produce a detailed `DependencyAnalysisReport`, and find plugins that depend on a given plugin.

```csharp
using System; 
using System.Collections.Generic; 
using System.Threading.Tasks; 
using Microsoft.Extensions.Logging; 
using PluginEngine.Utils.Helpers; 

public class AnalyzerDemo
{
    private readonly DependencyGraphAnalyzer _analyzer; 

    public AnalyzerDemo(IDependencyResolutionService resolver, ILogger<DependencyGraphAnalyzer> logger)
    {
        _analyzer = new DependencyGraphAnalyzer(resolver, logger); 
    }

    public async Task RunAsync(Plugin rootPlugin, IEnumerable<Plugin> allPlugins)
    {
        // Visualise the dependency graph
        string graph = await _analyzer.GenerateGraphVisualizationAsync(rootPlugin);
        Console.WriteLine(graph);

        // Analyse the plugin's dependencies
        DependencyAnalysisReport report = await _analyzer.AnalyzeAsync(rootPlugin);
        Console.WriteLine($"Plugin: {report.PluginName}");
        Console.WriteLine($"Complexity: {report.GetComplexityLevel()}");
        Console.WriteLine($"Issues: {string.Join(", ", report.Issues)}");

        // Find plugins that depend on the root plugin
        List<Guid> dependents = await _analyzer.FindDependentsAsync(allPlugins, rootPlugin.Id);
        Console.WriteLine($"Number of dependents: {dependents.Count}");
    }
}
```