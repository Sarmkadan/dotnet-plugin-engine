# DependencyResolutionException

The `DependencyResolutionException` is a specialized exception thrown by the `dotnet-plugin-engine` when the system fails to resolve dependencies required by a plugin. This exception provides diagnostic information, including the identity of the plugin that failed to resolve, the specific version constraints that could not be satisfied, the underlying reason for the failure, and a list of dependencies that could not be resolved.

## API

### Properties

*   **`public Guid? DependencyPluginId`**
    Gets the unique identifier of the plugin for which dependency resolution failed, if available.
*   **`public string VersionConstraint`**
    Gets the version constraint string that could not be satisfied for the dependency.
*   **`public DependencyResolutionReason Reason`**
    Gets the `DependencyResolutionReason` enum value that indicates the specific cause of the resolution failure.
*   **`public List<string> UnresolvedDependencies`**
    Gets the list of dependency names or identifiers that could not be resolved.

### Constructors

*   **`public DependencyResolutionException()`**
    Initializes a new instance of the `DependencyResolutionException` class.
*   **`public DependencyResolutionException(string message)`**
    Initializes a new instance of the `DependencyResolutionException` class with a specified error message.
*   **`public DependencyResolutionException`**
    Additional overloads are provided for initializing the exception with context regarding the plugin and the resolution reason.

### Methods

*   **`public DependencyResolutionException AddUnresolvedDependency(string dependencyName)`**
    Adds a dependency name or identifier to the `UnresolvedDependencies` list and returns the current `DependencyResolutionException` instance to allow for fluent configuration.
*   **`public override string ToString()`**
    Returns a string representation of the exception, including the error message, resolution reason, and the list of unresolved dependencies.

## Usage

### Example 1: Catching and Logging Resolution Errors
```csharp
try
{
    pluginEngine.LoadPlugin(pluginId);
}
catch (DependencyResolutionException ex)
{
    Console.WriteLine($"Failed to load plugin {ex.DependencyPluginId}.");
    Console.WriteLine($"Reason: {ex.Reason}");
    Console.WriteLine($"Unresolved: {string.Join(", ", ex.UnresolvedDependencies)}");
}
```

### Example 2: Throwing a Configured Exception
```csharp
if (!dependenciesSatisfied)
{
    throw new DependencyResolutionException("Required plugin dependencies are missing.")
    {
        DependencyPluginId = currentPluginId,
        Reason = DependencyResolutionReason.MissingDependencies
    }
    .AddUnresolvedDependency("Core.Abstractions")
    .AddUnresolvedDependency("Logging.Provider");
}
```

## Notes

*   **Edge Cases:** The `DependencyPluginId` property may be null if the failure occurs during a global resolution phase where no specific plugin context is available. The `UnresolvedDependencies` list is initialized by the constructor and should be checked for emptiness if no dependencies were specifically added.
*   **Thread Safety:** This exception is not inherently thread-safe. Once the exception is thrown, its properties should be treated as immutable for safe concurrent access during exception handling or logging. Modifying the `UnresolvedDependencies` list while the exception is being propagated across threads is not supported.
