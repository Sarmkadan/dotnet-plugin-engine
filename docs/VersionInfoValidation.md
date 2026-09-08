# VersionInfoValidation

Provides validation helpers for `VersionInfo` instances.

## Signatures

| Method | Signature | Description |
|--------|-----------|-------------|
| Validate | `public static IReadOnlyList<string> Validate(this VersionInfo value)` | Validates a VersionInfo instance and returns a list of human-readable validation problems. |
| IsValid | `public static bool IsValid(this VersionInfo value)` | Determines whether the specified VersionInfo is valid. |
| EnsureValid | `public static void EnsureValid(this VersionInfo value)` | Ensures that the specified VersionInfo is valid, throwing an exception if not. |

## Rules

The `Validate` method enforces the following rules on the VersionInfo instance:

- **Id**: Must not be empty (Guid.Empty)
- **EntityId**: Must not be empty (Guid.Empty)
- **Version**: 
  - Must not be null or whitespace
  - Must be a valid semantic version string (e.g., 1.0.0) as determined by `Version.TryParse`
- **ReleaseDate**:
  - Must not be the default DateTime value (0001-01-01 00:00:00)
  - Must not be in the future (cannot be later than DateTime.UtcNow.AddDays(1))
  - Must be a valid date after January 1, 2000 (>= 2000-01-01)
- **ReleaseNotes**: Must not be null (can be empty string)
- **IsPrerelease and PrereleaseIdentifier**:
  - If `IsPrerelease` is true, then `PrereleaseIdentifier` must not be null or whitespace
- **BuildMetadata**: Must not be null (can be empty string)
- **Compatibility**: Must not be null (can be empty collection)
- **DeprecationNotice**: Must not be null (can be empty string)
- **DownloadCount**: Must not be negative

## Exceptions

All methods throw an `ArgumentNullException` if the `value` parameter is null.

Additionally, `EnsureValid` throws an `ArgumentException` if the VersionInfo is invalid, containing all validation errors in its message.

## Example

```csharp
using PluginEngine.Domain.Entities;

// Creating a valid VersionInfo
var versionInfo = new VersionInfo
{
    Id = Guid.NewGuid(),
    EntityId = Guid.NewGuid(),
    Version = "2.5.0",
    ReleaseDate = new DateTime(2026, 1, 15),
    ReleaseNotes = "Added new features and fixed bugs",
    IsPrerelease = false,
    PrereleaseIdentifier = string.Empty,
    BuildMetadata = string.Empty,
    Compatibility = new List<string> { "net6.0", "net7.0" },
    DeprecationNotice = string.Empty,
    DownloadCount = 1500
};

// Validate and check for errors
var errors = versionInfo.Validate();
if (errors.Count == 0)
{
    Console.WriteLine("VersionInfo is valid.");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Alternative validation methods
bool isValid = versionInfo.IsValid(); // Returns true
versionInfo.EnsValid(); // Does not throw if valid

// Example of invalid VersionInfo (missing Version)
var invalidInfo = new VersionInfo
{
    Id = Guid.NewGuid(),
    EntityId = Guid.NewGuid(),
    // Version is intentionally left null/default
    ReleaseDate = DateTime.UtcNow,
    ReleaseNotes = "Test",
    IsPrerelease = false,
    PrereleaseIdentifier = string.Empty,
    BuildMetadata = string.Empty,
    Compatibility = new List<string>(),
    DeprecationNotice = string.Empty,
    DownloadCount = 0
};

try
{
    invalidInfo.EnsureValid();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    // Output will list all validation errors, e.g.:
    // VersionInfo is invalid. Validation errors:
    // Version must not be null or whitespace.
}
```