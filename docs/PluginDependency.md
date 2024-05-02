# PluginDependency
Represents a declared dependency between two plugins, specifying version constraints and metadata used by the plugin engine to resolve and validate plugin loads.

## API
| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `Id` | Unique identifier for this dependency record. | None | `Guid` | None |
| `PluginId` | Identifier of the plugin that declares the dependency. | None | `Guid` | None |
| `DependencyPluginId` | Identifier of the plugin that is required. | None | `Guid` | None |
| `MinimumVersion` | Inclusive lower bound of the allowed version range for the dependency, expressed as a semantic version string (e.g., "1.2.0"). May be `null` or empty to indicate no lower bound. | None | `string` | None |
| `MaximumVersion` | Inclusive upper bound of the allowed version range for the dependency, expressed as a semantic version string. May be `null` or empty to indicate no upper bound. | None | `string` | None |
| `IsOptional` | Indicates whether the dependency is required for the plugin to load (`false`) or can be omitted (`true`). | None | `bool` | None |
| `Type` | Categorizes the dependency (e.g., `Runtime`, `BuildTime`, `Optional`) using the `DependencyType` enum. | None | `DependencyType` | None |
| `Description` | Human‑readable explanation of why the dependency exists. | None | `string` | None |
| `CreatedAt` | Timestamp when the dependency record was created. | None | `DateTime` | None |
| `IsSatisfiedBy` | Evaluates whether a given plugin version satisfies the dependency’s version constraints and optionality. The method does **not** take parameters; it uses the current state of the instance (typically after the engine has populated it with the candidate plugin’s version). | None | `bool` – `true` if the dependency is satisfied, otherwise `false`. | None |
| `GetVersionConstraint` | Returns a string representation of the version range, formatted as `"[MinimumVersion, MaximumVersion]"` with unbounded sides omitted (e.g., `"[1.0.0,)"` or `"(,2.5.0]"`). If both bounds are unspecified, returns an empty string. | None | `string` | Throws `ArgumentException` if `MinimumVersion` or `MaximumVersion` cannot be parsed as a valid semantic version when they are non‑null/non‑empty. |
| `IsValid` | Indicates whether the dependency record is internally consistent (e.g., `Id` is not empty, `PluginId` and `DependencyPluginId` are not equal unless self‑dependency is allowed, version strings are parsable, and `Type` is a defined enum value). | None | `bool` – `true` if the record passes validation, otherwise `false`. | None |

## Usage
```csharp
using DotnetPluginEngine;

// Example 1: Creating and inspecting a dependency
var dep = new PluginDependency
{
    Id = Guid.NewGuid(),
    PluginId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    DependencyPluginId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    MinimumVersion = "3.0.0",
    MaximumVersion = "4.0.0",
    IsOptional = false,
    Type = DependencyType.Runtime,
    Description = "Required runtime library for core features.",
    CreatedAt = DateTime.UtcNow
};

if (dep.IsValid)
{
    string constraint = dep.GetVersionConstraint(); // "[3.0.0,4.0.0]"
    Console.WriteLine($"Dependency constraint: {constraint}");
}
else
{
    Console.WriteLine("Dependency record is malformed.");
}
```

```csharp
using DotnetPluginEngine;

// Example 2: Checking satisfaction against a candidate plugin version
var dep = new PluginDependency
{
    PluginId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
    DependencyPluginId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
    MinimumVersion = "2.1.0",
    MaximumVersion = null,   // no upper bound
    IsOptional = true,
    Type = DependencyType.Optional,
    Description = "Optional plugin for extended analytics.",
    CreatedAt = DateTime.UtcNow
};

// Assume the engine has resolved the candidate plugin's version to "2.3.5"
string candidateVersion = "2.3.5";

// In a real implementation IsSatisfiedBy would compare the stored version
// with the constraint; here we illustrate the call.
bool satisfied = dep.IsSatisfiedBy; // true if candidateVersion meets constraints
Console.WriteLine($"Dependency satisfied: {satisfied}");
```

## Notes
- **Version parsing**: `GetVersionConstraint` throws `ArgumentException` when `MinimumVersion` or `MaximumVersion` is set but does not conform to the semantic versioning pattern (major.minor.patch with optional pre‑release and build metadata). Empty or `null` values are treated as unbounded and do not cause an exception.
- **Self‑dependency**: The type does not prevent `PluginId` from equalling `DependencyPluginId`; whether such a relationship is allowed is governed by higher‑level validation logic in the plugin engine.
- **Mutability**: All members are public get/set properties; consequently, instances are **not** immutable. Concurrent modification of the same instance from multiple threads without external synchronization can lead to race conditions and inconsistent state. It is safe to read properties after construction if no thread is writing to the instance at the same time.
- **Optional dependencies**: When `IsOptional` is `true`, the engine treats a missing or unsatisfied dependency as non‑fatal; however, `IsValid` still returns `false` if the version constraints themselves are malformed.
- **Thread‑safety of reads**: Reading any property or invoking `GetVersionConstraint`/`IsSatisfiedBy` after the instance has been fully populated is thread‑safe provided no other thread mutates the instance simultaneously. For shared use across threads, consider creating a copy or employing synchronization mechanisms.
