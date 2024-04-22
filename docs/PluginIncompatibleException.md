# PluginIncompatibleException

The `PluginIncompatibleException` is an exception thrown by the `dotnet-plugin-engine` when a plugin assembly cannot be loaded or initialized because its defined compatibility constraints are not met by the current host engine. This exception serves to explicitly inform the consumer of the plugin loading process that the failure is due to a version mismatch rather than other potential causes, such as missing dependencies or file access errors.

## API

### `DeclaredConstraint`
Gets the version constraint requirement specified in the plugin's metadata. This string represents the range or version that the plugin expects from the host.

### `HostEngineVersion`
Gets the version string of the currently running host engine.

### `PluginIncompatibleException()`
Initializes a new instance of the `PluginIncompatibleException` class.

### `ToString()`
Returns a string representation of the exception, including the `DeclaredConstraint` and `HostEngineVersion` to assist in debugging the incompatibility.

## Usage

### Catching and Logging Incompatibility
```csharp
try
{
    pluginLoader.LoadPlugin("MyPlugin.dll");
}
catch (PluginIncompatibleException ex)
{
    Console.Error.WriteLine($"Plugin is incompatible: {ex.Message}");
    Console.Error.WriteLine($"Plugin required: {ex.DeclaredConstraint}");
    Console.Error.WriteLine($"Host engine version: {ex.HostEngineVersion}");
}
```

### Analyzing Mismatch Details
```csharp
public void ValidatePlugin(PluginInfo info, string engineVersion)
{
    if (!IsCompatible(info.Constraint, engineVersion))
    {
        throw new PluginIncompatibleException
        {
            DeclaredConstraint = info.Constraint,
            HostEngineVersion = engineVersion
        };
    }
}
```

## Notes

- **Edge Cases:** If `DeclaredConstraint` or `HostEngineVersion` are not set, their values will be `null`. Ensure handlers are capable of processing potential `null` values for these properties.
- **Thread Safety:** As an exception class, `PluginIncompatibleException` is serializable and thread-safe for standard usage patterns in multi-threaded environments. However, instances should generally be instantiated and thrown within a single thread context to maintain stack trace integrity.
