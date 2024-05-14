# PluginManagerService
The `PluginManagerService` class is a core component of the dotnet-plugin-engine project, responsible for managing plugins within the application. It provides a range of methods for initializing, shutting down, and interacting with plugins, including activation, deactivation, and retrieval of plugin details and statistics.

## API
* `public PluginManagerService`: The constructor for the `PluginManagerService` class.
* `public async Task InitializeAsync`: Initializes the plugin manager service. This method should be called before using any other methods of the service.
* `public async Task ShutdownAsync`: Shuts down the plugin manager service, releasing any resources it holds. This method should be called when the service is no longer needed.
* `public async Task<PluginManagerStatus> GetStatusAsync`: Retrieves the current status of the plugin manager service.
* `public async Task<IEnumerable<Plugin>> GetAllPluginsAsync`: Retrieves a list of all plugins managed by the service.
* `public async Task<IEnumerable<Plugin>> GetPluginsByStatusAsync`: Retrieves a list of plugins filtered by their current status.
* `public async Task<bool> ActivatePluginAsync`: Attempts to activate a plugin. Returns `true` if the activation is successful, `false` otherwise.
* `public async Task<bool> DeactivatePluginAsync`: Attempts to deactivate a plugin. Returns `true` if the deactivation is successful, `false` otherwise.
* `public async Task<PluginDetails?> GetPluginDetailsAsync`: Retrieves detailed information about a plugin. Returns `null` if the plugin is not found.
* `public async Task<IEnumerable<Plugin>> SearchPluginsAsync`: Searches for plugins based on a set of criteria.
* `public async Task<PluginManagerStatistics> GetStatisticsAsync`: Retrieves statistical information about the plugins managed by the service.

## Usage
The following examples demonstrate how to use the `PluginManagerService` class:
```csharp
// Example 1: Initialize and shutdown the plugin manager service
var pluginManager = new PluginManagerService();
await pluginManager.InitializeAsync();
// Use the plugin manager service
await pluginManager.ShutdownAsync();
```

```csharp
// Example 2: Activate and deactivate a plugin
var pluginManager = new PluginManagerService();
await pluginManager.InitializeAsync();
var plugin = await pluginManager.GetAllPluginsAsync();
if (plugin.Any())
{
    var firstPlugin = plugin.First();
    var activationResult = await pluginManager.ActivatePluginAsync(firstPlugin);
    if (activationResult)
    {
        Console.WriteLine("Plugin activated successfully");
    }
    else
    {
        Console.WriteLine("Plugin activation failed");
    }
    var deactivationResult = await pluginManager.DeactivatePluginAsync(firstPlugin);
    if (deactivationResult)
    {
        Console.WriteLine("Plugin deactivated successfully");
    }
    else
    {
        Console.WriteLine("Plugin deactivation failed");
    }
}
await pluginManager.ShutdownAsync();
```

## Notes
* The `PluginManagerService` class is designed to be used in a single-threaded or multi-threaded environment, but it is not thread-safe by default. Users should ensure that access to the service is properly synchronized to avoid concurrency issues.
* The `InitializeAsync` and `ShutdownAsync` methods should be called only once, respectively, to avoid unexpected behavior.
* The `ActivatePluginAsync` and `DeactivatePluginAsync` methods may throw exceptions if the plugin is not found or if the activation/deactivation process fails.
* The `GetPluginDetailsAsync` method may return `null` if the plugin is not found, and the `SearchPluginsAsync` method may return an empty list if no plugins match the search criteria.
