# dotnet-plugin-engine

A plugin runtime for .NET: isolated assembly loading, dependency resolution, versioning, and hot reload.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full picture - component breakdown, data flow, design decisions and their trade-offs, extension points, and known limitations. Short version: each plugin loads into its own collectible `AssemblyLoadContext`, a set of singleton services (loader, dependency resolver, versioning, hot reload, manager) does the actual work, and a thin `PluginEngine` facade ties them together. Everything is wired through `AddPluginEngine()` / `AddPluginEngineStack()`.

## PluginAssembly

`PluginAssembly` represents an assembly that belongs to a plugin. It stores details such as the assembly's unique identifier, plugin ID, assembly name, version, file path, and load status.

### Usage Example

```csharp
using PluginEngine.Domain.Entities;

// Create a new PluginAssembly instance
var assembly = new PluginAssembly
{
    PluginId = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
    AssemblyName = "MyAssembly",
    AssemblyVersion = "1.0.0",
    FilePath = "/path/to/MyAssembly.dll",
    FileSizeBytes = 1024 * 1024 * 10, // 10 MB
    FileHash = "some-file-hash",
    PublicKeyToken = "some-public-key-token",
    IsMainAssembly = true,
    LoadContextId = "my-load-context",
    LastModifiedAt = DateTime.UtcNow,
    LoadedAt = DateTime.UtcNow,
    Status = AssemblyLoadStatus.Loaded
};

// Validate the assembly
bool isValid = assembly.IsValid();
Console.WriteLine($"Is assembly valid? {isValid}");

// Get the assembly qualified name
string qualifiedName = assembly.GetQualifiedName();
Console.WriteLine($"Assembly qualified name: {qualifiedName}");

// Update the assembly file information
assembly.UpdateFileInfo("/path/to/new-assembly.dll", 1024 * 1024 * 20, "new-file-hash");

// Mark the assembly as loaded
assembly.MarkAsLoaded("my-load-context");

// Mark the assembly as failed load
assembly.MarkAsFailedLoad("some-error-message");
```

// ... rest of content ...

## PluginExecutionContext

`PluginExecutionContext` represents the execution context for a plugin operation. It tracks the entire lifecycle of a plugin execution including timing metrics, state management, data storage, and result handling. The context provides methods to mark execution as successful, failed, or cancelled, and includes comprehensive metrics collection.

### Usage Example

```csharp
using PluginEngine.Execution;
using PluginEngine.Domain.Entities;
using System;
using System.Collections.Generic;

// Create a new execution context for a plugin operation
var executionContext = new PluginExecutionContext
{
    ExecutionId = Guid.NewGuid(),
    Plugin = new Plugin
    {
        Id = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
        Name = "DataProcessorPlugin",
        Version = "1.0.0",
        Description = "Processes data files and generates reports"
    },
    OperationType = "DataProcessing",
    StartedAtUtc = DateTime.UtcNow,
    Data = new Dictionary<string, object>
    {
        { "InputFilePath", "/data/input/file.csv" },
        { "OutputDirectory", "/data/output/" },
        { "BatchSize", 1000 }
    },
    State = ExecutionState.Running,
    CpuTimeMs = 0,
    MemoryBytesAllocated = 0,
    GarbageCollections = 0,
    CollectedAtUtc = DateTime.UtcNow,
    CustomMetrics = new Dictionary<string, long>()
};

// Track CPU time during execution
for (int i = 0; i < 100; i++)
{
    // Simulate work
    System.Threading.Thread.Sleep(10);
    executionContext.CpuTimeMs += 10;
    executionContext.MemoryBytesAllocated += 1024 * 1024; // 1MB
}

// Track garbage collections
GC.Collect();
executionContext.GarbageCollections++;

// Add custom metrics
if (executionContext.CustomMetrics.ContainsKey("FilesProcessed"))
    executionContext.CustomMetrics["FilesProcessed"]++;
else
    executionContext.CustomMetrics.Add("FilesProcessed", 1);

// Complete execution successfully with result
var processingResult = new { Success = true, RecordsProcessed = 5000 };
executionContext.CompleteSuccess(processingResult);

Console.WriteLine($"Execution completed in {executionContext.CpuTimeMs}ms");
Console.WriteLine($"Memory allocated: {executionContext.MemoryBytesAllocated / (1024 * 1024)}MB");
Console.WriteLine($"Garbage collections: {executionContext.GarbageCollections}");

// Get execution summary
var summary = executionContext.GetSummary();
Console.WriteLine($"Status: {summary.State}");
Console.WriteLine($"Duration: {summary.Duration.TotalSeconds}s");
```

## VersionInfo

`VersionInfo` represents versioning information for plugins and assemblies. It stores details such as the version string, release date, compatibility information, pre-release status, and download metrics. The class provides methods for version manipulation, compatibility checking, and formatted display strings.

### Usage Example

```csharp
using PluginEngine.Domain.Entities;
using System;

// ... rest of content ...


## MarketplaceEntry

`MarketplaceEntry` represents a plugin listing in the marketplace, including discovery metadata and the list of published versions. It contains essential information for displaying plugins in a marketplace interface such as identification details, versioning information, author data, download statistics, ratings, and compatibility information.


### Usage Example

```csharp
using PluginEngine.Marketplace;
using System;

// Create a new marketplace entry for a plugin listing
var entry = new MarketplaceEntry
{
    Name = "DataProcessor",
    Author = "Acme Corp",
    Description = "A plugin for processing data files and generating reports",
    LatestVersion = "2.1.0",
    Tags = new List<string> { "data", "processing", "reports", "analytics" },
    Downloads = 15423,
    Rating = 4.7,
    PublishedAtUtc = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc),
    UpdatedAtUtc = new DateTime(2024, 6, 10, 0, 0, 0, DateTimeKind.Utc),
    IsVerified = true,
    LicenseType = "MIT",
    RepositoryUrl = "https://github.com/acme/DataProcessor",
    AvailableVersions = new List<PluginVersionInfo>
    {
        new PluginVersionInfo
        {
            Version = "2.1.0",
            ReleaseDate = new DateTime(2024, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            Prerelease = false,
            Downloads = 5423,
            FileSizeBytes = 2 * 1024 * 1024 // 2MB
        },
        new PluginVersionInfo
        {
            Version = "2.0.0",
            ReleaseDate = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Prerelease = false,
            Downloads = 10000,
            FileSizeBytes = 1 * 1024 * 1024 // 1MB
        }
    }
};

// Access the plugin details
Console.WriteLine($"Plugin: {entry.Name}");
Console.WriteLine($"Author: {entry.Author}");
Console.WriteLine($"Latest Version: {entry.LatestVersion}");
Console.WriteLine($"Total Downloads: {entry.Downloads:N0}");
Console.WriteLine($"Rating: {entry.Rating:F1}/5.0");
Console.WriteLine($"Published: {entry.PublishedAtUtc:yyyy-MM-dd}");
Console.WriteLine($"Updated: {entry.UpdatedAtUtc:yyyy-MM-dd}");
Console.WriteLine($"Verified: {entry.IsVerified}");
Console.WriteLine($"License: {entry.LicenseType}");
Console.WriteLine($"Repository: {entry.RepositoryUrl}");

// Access version history
foreach (var version in entry.AvailableVersions.OrderByDescending(v => v.Version))
{
    Console.WriteLine($"  Version {version.Version} - {version.ReleaseDate:yyyy-MM-dd}, {version.Downloads:N0} downloads");
}
```
