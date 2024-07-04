## PluginException

The `PluginException` class represents a custom exception thrown by the plugin engine. It provides additional context and information about the error, including an error code, entity ID, and context dictionary.

### Usage Example

```csharp
try
{
    // Code that may throw a PluginException
}
catch (PluginException ex)
{
    Console.WriteLine($"Error code: {ex.ErrorCode}");
    Console.WriteLine($"Entity ID: {ex.EntityId}");
    Console.WriteLine($"Context: {ex.Context}");
    Console.WriteLine(ex.ToString());
}
```

## VersionMismatchException

The `VersionMismatchException` is thrown when version constraints between components are not satisfied. It provides detailed information about the expected and actual versions, along with the component type and name that caused the mismatch.

### Usage Example

```csharp
using PluginEngine.Exceptions;

// Validate plugin version compatibility
if (plugin.Version != expectedVersion)
{
    throw new VersionMismatchException(
        message: $"Plugin version mismatch detected",
        expectedVersion: expectedVersion,
        actualVersion: plugin.Version,
        componentType: "Plugin",
        componentName: plugin.Name
    );
}

// Or when validating assembly dependencies
if (assembly.GetName().Version?.ToString() != requiredAssemblyVersion)
{
    throw new VersionMismatchException(
        message: $"Assembly version constraint violated",
        expectedVersion: requiredAssemblyVersion,
        actualVersion: assembly.GetName().Version?.ToString() ?? "unknown",
        componentType: "Assembly",
        componentName: assembly.GetName().Name ?? "unknown"
    );
}

// Catch and handle the exception
try
{
    pluginEngine.LoadPlugin(pluginPath);
}
catch (VersionMismatchException ex)
{
    Console.WriteLine($"Version mismatch error: {ex.Message}");
    Console.WriteLine(ex.ToString());
    Console.WriteLine($"Expected: {ex.ExpectedVersion}");
    Console.WriteLine($"Actual: {ex.ActualVersion}");
    Console.WriteLine($"Component: {ex.ComponentType} - {ex.ComponentName}");
}
```

## PluginLoadException

The `PluginLoadException` class represents an error that occurs during the loading of a plugin. It provides details about the plugin name, assembly path, and the specific stage at which the load failed.

### Usage Example

```csharp
try
{
    // Simulate a plugin load failure
    throw new PluginLoadException(
        message: "Failed to resolve plugin dependencies",
        pluginName: "MyPlugin",
        assemblyPath: "/plugins/MyPlugin.dll",
        stage: PluginLoadStage.DependencyResolution
    );
}
catch (PluginLoadException ex)
{
    Console.WriteLine($"Error loading plugin: {ex.Message}");
    Console.WriteLine($"Plugin: {ex.PluginName}");
    Console.WriteLine($"Path: {ex.AssemblyPath}");
    Console.WriteLine($"Stage: {ex.LoadStage}");
    Console.WriteLine(ex.ToString());
}
```

## DependencyResolutionException

The `DependencyResolutionException` represents an error that occurs during the resolution of plugin dependencies. It provides details about the required dependency plugin ID, version constraint, and the reason for the resolution failure.

### Usage Example

```csharp
try
{
    // Simulate a dependency resolution failure
    throw new DependencyResolutionException(
        message: "Failed to resolve dependency",
        dependencyPluginId: Guid.NewGuid(),
        versionConstraint: ">= 1.0.0",
        reason: DependencyResolutionReason.DependencyNotFound
    );
}
catch (DependencyResolutionException ex)
{
    Console.WriteLine($"Dependency resolution error: {ex.Message}");
    Console.WriteLine($"Dependency ID: {ex.DependencyPluginId}");
    Console.WriteLine($"Version constraint: {ex.VersionConstraint}");
    Console.WriteLine($"Reason: {ex.Reason}");
    Console.WriteLine($"Unresolved dependencies: {string.Join(", ", ex.UnresolvedDependencies)}");
    Console.WriteLine(ex.ToString());
}
```