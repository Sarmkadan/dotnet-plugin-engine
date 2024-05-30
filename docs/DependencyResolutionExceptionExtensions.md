# DependencyResolutionExceptionExtensions

Provides extension methods for enriching a `DependencyResolutionException` with contextual information about plugin dependencies, version constraints, and unresolved dependency summaries. These methods are designed to be used in a fluent style to build detailed error messages during plugin resolution failures.

## API

### `WithDependencyPluginId`

```csharp
public static DependencyResolutionException WithDependencyPluginId(
    this DependencyResolutionException exception,
    string pluginId)
```

Attaches the identifier of the plugin that caused the dependency resolution failure.

- **Parameters**  
  `exception` – The original `DependencyResolutionException`.  
  `pluginId` – The plugin identifier (e.g., `"MyPlugin"`). Must not be null or empty.

- **Returns**  
  The same `DependencyResolutionException` instance with the plugin ID stored internally.

- **Throws**  
  `ArgumentNullException` if `exception` is null.  
  `ArgumentException` if `pluginId` is null or empty.

### `WithVersionConstraint`

```csharp
public static DependencyResolutionException WithVersionConstraint(
    this DependencyResolutionException exception,
    string versionConstraint)
```

Attaches a version constraint string (e.g., `"^1.0.0"` or `">=2.0.0 <3.0.0"`) that was being enforced when the resolution failed.

- **Parameters**  
  `exception` – The original `DependencyResolutionException`.  
  `versionConstraint` – The version constraint expression. Must not be null or empty.

- **Returns**  
  The same `DependencyResolutionException` instance with the version constraint stored internally.

- **Throws**  
  `ArgumentNullException` if `exception` is null.  
  `ArgumentException` if `versionConstraint` is null or empty.

### `AddUnresolvedDependencies`

```csharp
public static DependencyResolutionException AddUnresolvedDependencies(
    this DependencyResolutionException exception,
    IEnumerable<string> unresolvedDependencies)
```

Adds a collection of dependency identifiers that could not be resolved.

- **Parameters**  
  `exception` – The original `DependencyResolutionException`.  
  `unresolvedDependencies` – A sequence of dependency identifiers (e.g., `["PluginA", "PluginB"]`). May be empty; null is treated as an empty collection.

- **Returns**  
  The same `DependencyResolutionException` instance with the unresolved dependencies appended to any previously stored list.

- **Throws**  
  `ArgumentNullException` if `exception` is null.

### `GetUnresolvedDependenciesSummary`

```csharp
public static string GetUnresolvedDependenciesSummary(
    this DependencyResolutionException exception)
```

Returns a human-readable summary of all unresolved dependencies that have been recorded on the exception.

- **Parameters**  
  `exception` – The `DependencyResolutionException` to query.

- **Returns**  
  A formatted string listing the unresolved dependencies. If no dependencies have been added, returns an empty string.

- **Throws**  
  `ArgumentNullException` if `exception` is null.

## Usage

The following examples demonstrate typical usage within a plugin resolution pipeline.

### Example 1: Building a detailed exception with fluent calls

```csharp
var exception = new DependencyResolutionException("Unable to resolve dependencies for plugin 'Core'.")
    .WithDependencyPluginId("Core")
    .WithVersionConstraint(">=2.0.0")
    .AddUnresolvedDependencies(new[] { "Logging", "Configuration" });

// Later, when logging or displaying the error:
string summary = exception.GetUnresolvedDependenciesSummary();
Console.WriteLine(summary);
// Output: "Unresolved dependencies: Logging, Configuration"
```

### Example 2: Accumulating unresolved dependencies across multiple resolution attempts

```csharp
var baseException = new DependencyResolutionException("Multiple resolution failures.");

// First attempt
baseException.AddUnresolvedDependencies(new[] { "PluginA" });

// Second attempt (same exception instance)
baseException.AddUnresolvedDependencies(new[] { "PluginB", "PluginC" });

string finalSummary = baseException.GetUnresolvedDependenciesSummary();
// finalSummary == "Unresolved dependencies: PluginA, PluginB, PluginC"
```

## Notes

- **Thread safety** – These extension methods are not thread-safe. If the same `DependencyResolutionException` instance is modified concurrently from multiple threads, the internal state may become inconsistent. Callers should synchronize access or use separate exception instances per thread.
- **Null handling** – All methods throw `ArgumentNullException` when the `exception` parameter is null. The `AddUnresolvedDependencies` method accepts a null `unresolvedDependencies` parameter and treats it as an empty collection (no dependencies added).
- **Empty or whitespace identifiers** – `WithDependencyPluginId` and `WithVersionConstraint` reject empty strings. Whitespace-only strings are also considered invalid and will throw `ArgumentException`.
- **Fluent chaining** – The methods `WithDependencyPluginId`, `WithVersionConstraint`, and `AddUnresolvedDependencies` return the same exception instance, enabling fluent chaining. The order of calls does not affect the final summary.
- **Summary formatting** – The output of `GetUnresolvedDependenciesSummary` is implementation-defined but is guaranteed to be a non-null string. It may include the plugin ID and version constraint if they were set, though the current signature does not expose those in the summary.
