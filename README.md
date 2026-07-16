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