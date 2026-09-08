# DependencyResolutionExceptionValidation

Provides validation helpers for `DependencyResolutionException` instances.

## Public Methods

| Method | Description |
|--------|-------------|
| `Validate(this DependencyResolutionException value)` | Validates the specified exception instance and returns a list of validation problems; empty if the instance is valid. Throws `ArgumentNullException` if `value` is null. |
| `IsValid(this DependencyResolutionException value)` | Determines whether the specified exception instance is valid. Returns `true` if valid; otherwise, `false`. Throws `ArgumentNullException` if `value` is null. |
| `EnsureValid(this DependencyResolutionException value)` | Ensures that the specified exception instance is valid. Throws `ArgumentNullException` if `value` is null, or `ArgumentException` if the instance is invalid, containing a list of validation problems. |

## Problem Messages

The `Validate` method may return the following problem messages:

- "VersionConstraint exceeds maximum length of 256 characters."
- "VersionConstraint contains control characters."
- "Reason has an invalid enum value."
- "UnresolvedDependencies collection is null."
- "UnresolvedDependencies collection exceeds maximum size of 1000 items."
- "UnresolvedDependencies contains {count} duplicate entries: {list}." (where `{count}` is the number of duplicates and `{list}` is a comma-separated list of up to 5 duplicates, followed by "..." if there are more than 5)
- "UnresolvedDependencies contains null, empty, or whitespace-only entries."
- "UnresolvedDependencies contains entries exceeding maximum length of 256 characters."
- "UnresolvedDependencies contains entries with control characters."

## Usage Example

```csharp
using PluginEngine.Exceptions;
using PluginEngine.Validation;

var ex = new DependencyResolutionException(
    "Missing dependency",
    "MyPlugin",
    new[] { "DependencyA", "DependencyB" });

var problems = DependencyResolutionExceptionValidation.Validate(ex);
if (problems.Any())
{
    foreach (var p in problems)
    {
        Console.WriteLine(p);
    }
}
else
{
    Console.WriteLine("Exception is valid.");
}
```