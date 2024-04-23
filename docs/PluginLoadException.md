# PluginLoadException

`PluginLoadException` is a custom exception class within the `dotnet-plugin-engine` used to encapsulate errors that occur during the lifecycle of loading a plugin assembly. It provides critical diagnostic metadata, including the plugin's name, the target assembly path, and the specific stage of the loading process where the failure occurred.

## API

### Properties

- `public string PluginName`
  Gets or sets the name of the plugin associated with the load failure.
- `public string AssemblyPath`
  Gets or sets the file system path of the assembly that failed to load.
- `public PluginLoadStage LoadStage`
  Gets or sets the `PluginLoadStage` value indicating the point in the loading process at which the exception was thrown.

### Constructors

- `public PluginLoadException() : base`
  Initializes a new instance of the `PluginLoadException` class with default property values.
- `public PluginLoadException(string message) : base`
  Initializes a new instance of the `PluginLoadException` class with a specified error message.
- `public PluginLoadException(string pluginName, string assemblyPath, PluginLoadStage loadStage)`
  Initializes a new instance of the `PluginLoadException` class with the specified plugin name, assembly path, and load stage.
- `public PluginLoadException(string pluginName, string assemblyPath, PluginLoadStage loadStage, string message)`
  Initializes a new instance of the `PluginLoadException` class with the specified plugin metadata and an error message.
- `public PluginLoadException(string pluginName, string assemblyPath, PluginLoadStage loadStage, string message, Exception innerException)`
  Initializes a new instance of the `PluginLoadException` class with specified metadata, an error message, and a reference to the inner exception that caused the failure.

### Methods

- `public override string ToString`
  Returns a string representation of the exception, including the message, plugin name, assembly path, load stage, and stack trace, providing a comprehensive diagnostic summary.

## Usage

### Catching and Logging Plugin Load Errors

```csharp
try
{
    pluginLoader.Load("MyPlugin", "/path/to/myplugin.dll");
}
catch (PluginLoadException ex)
{
    logger.LogError(
        "Failed to load plugin '{PluginName}' at '{AssemblyPath}' during stage '{LoadStage}': {Message}",
        ex.PluginName,
        ex.AssemblyPath,
        ex.LoadStage,
        ex.Message
    );
}
```

### Inspecting Exception Metadata

```csharp
catch (PluginLoadException ex)
{
    if (ex.LoadStage == PluginLoadStage.Resolution)
    {
        // Handle assembly resolution issues specifically
        PerformAssemblyDiscoveryFix(ex.AssemblyPath);
    }
    else
    {
        throw; // Re-throw if the error stage is unrecoverable
    }
}
```

## Notes

- **Edge Cases:** If `PluginName` or `AssemblyPath` are not provided during construction, they will default to `null`. It is recommended to validate these inputs when catching the exception if logic depends on their presence.
- **Thread Safety:** `PluginLoadException` instances are generally thread-safe for reading after they have been thrown. However, like all exceptions, they should be treated as immutable once they have been raised to ensure consistency during error handling and logging operations.
- **Diagnostics:** The `ToString()` method override is designed for logging and debugging; it is recommended to use it for capturing diagnostic context in log files or monitoring systems.
