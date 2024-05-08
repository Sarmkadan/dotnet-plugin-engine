# VersionInfo

The `VersionInfo` class serves as the primary data contract for representing plugin version metadata within the `dotnet-plugin-engine`. It encapsulates semantic versioning details, release lifecycle states, compatibility constraints, and usage statistics, providing both immutable identification properties and mutable methods for version progression. This type ensures consistent version tracking, validation, and display formatting across the plugin ecosystem.

## API

### `public Guid Id`
Gets the unique identifier for this specific version record. This GUID distinguishes this specific release instance from all other versions of the same plugin or other plugins entirely.

### `public Guid EntityId`
Gets the unique identifier of the parent plugin entity to which this version belongs. Multiple `VersionInfo` instances may share the same `EntityId` if they represent different releases of the same plugin.

### `public string Version`
Gets the raw version string representation. This property holds the foundational version text before semantic parsing or formatting logic is applied.

### `public DateTime ReleaseDate`
Gets the date and time when this version was officially released. This value is used for sorting release histories and calculating plugin age.

### `public string ReleaseNotes`
Gets the textual description of changes, fixes, and features included in this release. This may contain markdown or plain text depending on the source input.

### `public bool IsPrerelease`
Gets a value indicating whether this version is a pre-release (e.g., alpha, beta, rc). If `true`, the version is considered unstable for production environments by default.

### `public string PrereleaseIdentifier`
Gets the specific label appended to the version for pre-release tracking (e.g., "alpha", "beta.1"). This property is populated only when `IsPrerelease` is `true`; otherwise, it may be null or empty.

### `public string BuildMetadata`
Gets the build metadata string appended to the version (e.g., commit hash, build number). This information is ignored in version precedence calculations but retained for traceability.

### `public string Compatibility`
Gets the compatibility constraint string defining which host engine versions or other plugin versions this release supports. The format depends on the engine's specific dependency resolution syntax.

### `public bool IsActive`
Gets a value indicating whether this version is currently active and available for download or installation. Deprecated or withdrawn versions may have this set to `false`.

### `public string DeprecationNotice`
Gets the warning message displayed to users if this version is deprecated. If the version is not deprecated, this property returns null or an empty string.

### `public int DownloadCount`
Gets the cumulative number of times this specific version has been downloaded. This value is updated asynchronously by the engine's telemetry services.

### `public string GetSemanticVersion`
Gets the standardized Semantic Versioning (SemVer) string representation of this version.
*   **Return Value**: A string formatted strictly as `Major.Minor.Patch[-Prerelease][+BuildMetadata]`.
*   **Remarks**: This property computes the final string based on component fields rather than returning the raw `Version` input.

### `public string GetDisplayString`
Gets a human-readable string suitable for UI presentation.
*   **Return Value**: A formatted string that typically includes the version number and pre-release status (e.g., "v1.2.3 (Beta)").
*   **Remarks**: The exact format may vary based on localization settings or engine configuration.

### `public bool IsCompatibleWith`
Determines if this version is compatible with a specified target version or environment.
*   **Parameters**: Implicitly compares against a target version string or environment context defined by the engine.
*   **Return Value**: `true` if the `Compatibility` constraints are satisfied; otherwise, `false`.
*   **Exceptions**: May throw a format exception if the internal `Compatibility` string is malformed.

### `public void IncrementPatch`
Increments the patch component of the version (e.g., 1.0.0 becomes 1.0.1).
*   **Side Effects**: Modifies the internal state of the instance. Resets pre-release identifiers if moving to a stable release, depending on implementation logic.
*   **Remarks**: Typically used during automated build pipelines for hotfixes.

### `public void IncrementMinor`
Increments the minor component of the version (e.g., 1.0.0 becomes 1.1.0) and resets the patch component to zero.
*   **Side Effects**: Modifies the internal state of the instance.
*   **Remarks**: Used for backward-compatible feature additions.

### `public void IncrementMajor`
Increments the major component of the version (e.g., 1.0.0 becomes 2.0.0) and resets minor and patch components to zero.
*   **Side Effects**: Modifies the internal state of the instance.
*   **Remarks**: Used for breaking changes.

### `public bool IsValid`
Gets a value indicating whether the current state of the `VersionInfo` instance represents a valid semantic version.
*   **Return Value**: `true` if all required fields conform to parsing rules; otherwise, `false`.
*   **Remarks**: Validation checks include non-negative integers for version parts and valid characters for identifiers.

## Usage

### Example 1: Inspecting Plugin Version Details
This example demonstrates retrieving and displaying critical metadata for a specific plugin version, including handling pre-release states.

```csharp
using DotNetPluginEngine.Models;

public void DisplayPluginVersion(VersionInfo version)
{
    if (!version.IsValid)
    {
        Console.WriteLine("Invalid version data detected.");
        return;
    }

    Console.WriteLine($"Plugin Entity: {version.EntityId}");
    Console.WriteLine($"Version Record: {version.Id}");
    Console.WriteLine($"Semantic Version: {version.GetSemanticVersion}");
    Console.WriteLine($"Display Name: {version.GetDisplayString}");
    Console.WriteLine($"Released On: {version.ReleaseDate:yyyy-MM-dd}");

    if (version.IsPrerelease)
    {
        Console.WriteLine($"[Pre-Release] Identifier: {version.PrereleaseIdentifier}");
    }

    if (!string.IsNullOrEmpty(version.DeprecationNotice))
    {
        Console.WriteLine($"WARNING: {version.DeprecationNotice}");
    }

    Console.WriteLine($"Downloads: {version.DownloadCount}");
    Console.WriteLine($"Active: {version.IsActive}");
}
```

### Example 2: Managing Version Progression
This example illustrates creating a new version record and programmatically incrementing version numbers based on the type of change being released.

```csharp
using DotNetPluginEngine.Models;
using System;

public VersionInfo PrepareNextRelease(VersionInfo currentVersion, ReleaseType type)
{
    // Assume a copy constructor or cloning mechanism exists or we are mutating a draft object
    // For this example, we assume we are working on a mutable draft instance derived from currentVersion
    
    switch (type)
    {
        case ReleaseType.Hotfix:
            currentVersion.IncrementPatch();
            currentVersion.ReleaseNotes = "Applied critical hotfix.";
            break;
            
        case ReleaseType.Feature:
            currentVersion.IncrementMinor();
            currentVersion.ReleaseNotes = "Added new features.";
            break;
            
        case ReleaseType.BreakingChange:
            currentVersion.IncrementMajor();
            currentVersion.ReleaseNotes = "Breaking changes introduced.";
            break;
    }

    // Reset prerelease flags for a stable release
    if (type != ReleaseType.PreRelease)
    {
        // Logic to clear prerelease state would typically accompany increment methods
        // or be handled explicitly depending on engine policy
    }

    currentVersion.ReleaseDate = DateTime.UtcNow;
    
    if (!currentVersion.IsValid)
    {
        throw new InvalidOperationException("Version increment resulted in an invalid state.");
    }

    return currentVersion;
}

public enum ReleaseType
{
    Hotfix,
    Feature,
    BreakingChange,
    PreRelease
}
```

## Notes

*   **Mutability and Thread Safety**: The presence of mutation methods (`IncrementPatch`, `IncrementMinor`, `IncrementMajor`) indicates that `VersionInfo` instances are mutable. This type is **not thread-safe**. If a `VersionInfo` instance is shared across multiple threads, external synchronization (e.g., `lock` statements) is required when invoking increment methods to prevent race conditions where version numbers may be corrupted.
*   **Validation State**: The `IsValid` property should be checked after any manual manipulation of string-based version fields or after using increment methods if the internal state can be compromised by invalid data sources. An invalid instance may cause `GetSemanticVersion` or `IsCompatibleWith` to throw formatting exceptions.
*   **Pre-release Logic**: When `IsPrerelease` is `false`, the `PrereleaseIdentifier` and `BuildMetadata` properties may return null or empty strings. Consumers should not assume these fields always contain data.
*   **Identity vs. Entity**: Care must be taken to distinguish between `Id` (unique to this specific version record) and `EntityId` (shared across all versions of a plugin). Queries filtering by plugin should use `EntityId`, while operations targeting a specific release artifact should use `Id`.
*   **Compatibility Parsing**: The `IsCompatibleWith` method relies on the `Compatibility` string format. If this string contains custom or malformed syntax not recognized by the engine's resolver, the method may return `false` or throw an exception depending on the engine's strictness configuration.
