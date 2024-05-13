# IVersioningService

The `IVersioningService` interface defines the contract for accessing semantic versioning components within the `dotnet-plugin-engine`. It exposes the core elements of a version string, including major, minor, and patch integers, along with optional prerelease and metadata identifiers, providing a standardized mechanism for plugins to inspect and report version information.

## API

### Major
```csharp
public int Major { get; }
```
Retrieves the major version number. This value typically increments when incompatible API changes are introduced. It returns a non-negative integer representing the current major version.

### Minor
```csharp
public int Minor { get; }
```
Retrieves the minor version number. This value typically increments when functionality is added in a backward-compatible manner. It returns a non-negative integer representing the current minor version.

### Patch
```csharp
public int Patch { get; }
```
Retrieves the patch version number. This value typically increments for backward-compatible bug fixes. It returns a non-negative integer representing the current patch level.

### Prerelease
```csharp
public string Prerelease { get; }
```
Retrieves the prerelease identifier associated with the version (e.g., "alpha", "beta.1"). If the version is a stable release, this property returns `null` or an empty string depending on the specific implementation strategy. It does not throw exceptions under normal operation.

### Metadata
```csharp
public string Metadata { get; }
```
Retrieves the build metadata identifier associated with the version (e.g., "git.sha123"). Build metadata is ignored when determining version precedence. If no metadata is present, this property returns `null` or an empty string. It does not throw exceptions under normal operation.

### ToString
```csharp
public override string ToString()
```
Constructs and returns the full semantic version string formatted according to SemVer 2.0.0 specifications (`Major.Minor.Patch-Prerelease+Metadata`).
*   **Returns**: A `string` representing the complete version.
*   **Throws**: No exceptions are expected during standard formatting operations unless the internal state is critically corrupted, which is outside the scope of normal usage.

## Usage

### Example 1: Version Comparison Logic
This example demonstrates how to access individual version components to implement custom compatibility logic within a plugin.

```csharp
public void CheckCompatibility(IVersioningService currentVersion)
{
    if (currentVersion.Major != 2)
    {
        throw new PluginException("This plugin requires engine major version 2.");
    }

    if (currentVersion.Minor < 5)
    {
        Console.WriteLine($"Warning: Features requiring v2.5+ may be unavailable. Detected: {currentVersion.Major}.{currentVersion.Minor}");
    }

    if (!string.IsNullOrEmpty(currentVersion.Prerelease))
    {
        Console.WriteLine($"Running against prerelease build: {currentVersion.Prerelease}");
    }
}
```

### Example 2: Logging Full Version String
This example shows how to utilize the overridden `ToString` method for logging or diagnostic reporting.

```csharp
public void LogStartupInfo(IVersioningService engineVersion)
{
    // Outputs format: "1.4.2-beta.1+build.99"
    Console.WriteLine($"DotNet Plugin Engine Version: {engineVersion.ToString()}");
    
    // Accessing metadata specifically for audit trails
    if (!string.IsNullOrEmpty(engineVersion.Metadata))
    {
        AuditLog.RecordBuildMetadata(engineVersion.Metadata);
    }
}
```

## Notes

*   **Immutability**: The properties exposed by `IVersioningService` appear to be read-only getters. Implementations should treat the version data as immutable once the service instance is constructed.
*   **Null Handling**: Consumers must handle potential `null` or empty values for `Prerelease` and `Metadata`, as these are optional components of the semantic versioning specification and may not be present in stable releases.
*   **Thread Safety**: As the interface exposes only data retrieval methods without state-modifying operations, implementations of `IVersioningService` are expected to be thread-safe for concurrent read access across multiple threads.
*   **Formatting Standards**: The `ToString` method is expected to strictly adhere to SemVer 2.0.0 formatting rules. If `Prerelease` is present, it is prefixed with a hyphen; if `Metadata` is present, it is prefixed with a plus sign.
