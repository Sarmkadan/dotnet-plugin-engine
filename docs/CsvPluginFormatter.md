# CsvPluginFormatter

The `CsvPluginFormatter` class provides methods to serialize plugin information into CSV-formatted strings. It is designed for use in the `dotnet-plugin-engine` ecosystem, where plugin metadata, detailed reports, and health status need to be exported in a structured, machine-readable format. Each method returns a `Task<string>` to support asynchronous formatting operations.

## API

### `Task<string> FormatPluginAsync(Plugin plugin)`

Formats a single plugin into a CSV string.

- **Parameters**  
  `plugin` – The `Plugin` object to format. Must not be `null`.

- **Returns**  
  A `Task<string>` that resolves to a CSV string representing the plugin. The string includes a header row and one data row.

- **Throws**  
  `ArgumentNullException` if `plugin` is `null`.  
  `InvalidOperationException` if the plugin’s internal state is inconsistent (e.g., missing required properties).

### `Task<string> FormatPluginsAsync(IEnumerable<Plugin> plugins)`

Formats a collection of plugins into a single CSV string.

- **Parameters**  
  `plugins` – An `IEnumerable<Plugin>` to format. May be empty; must not be `null`.

- **Returns**  
  A `Task<string>` that resolves to a CSV string containing a header row followed by one data row per plugin.

- **Throws**  
  `ArgumentNullException` if `plugins` is `null`.  
  `InvalidOperationException` if any plugin in the collection has an inconsistent state.

### `Task<string> FormatDetailedReportAsync(Plugin plugin)`

Formats a detailed report for a single plugin into a CSV string. The report typically includes extended metadata beyond the basic plugin information.

- **Parameters**  
  `plugin` – The `Plugin` object to format. Must not be `null`.

- **Returns**  
  A `Task<string>` that resolves to a CSV string containing a header row and one data row with detailed fields.

- **Throws**  
  `ArgumentNullException` if `plugin` is `null`.  
  `InvalidOperationException` if the plugin’s detailed data cannot be retrieved or is malformed.

### `Task<string> FormatHealthReportAsync(Plugin plugin)`

Formats a health report for a single plugin into a CSV string. The report includes health metrics, status flags, and timestamps.

- **Parameters**  
  `plugin` – The `Plugin` object to format. Must not be `null`.

- **Returns**  
  A `Task<string>` that resolves to a CSV string containing a header row and one data row with health-related fields.

- **Throws**  
  `ArgumentNullException` if `plugin` is `null`.  
  `InvalidOperationException` if the plugin’s health data is unavailable or inconsistent.

## Usage

### Example 1: Formatting a single plugin

```csharp
using System;
using System.Threading.Tasks;
using PluginEngine.Formatters;

public async Task ExportPluginAsync(Plugin plugin)
{
    var formatter = new CsvPluginFormatter();
    string csv = await formatter.FormatPluginAsync(plugin);
    Console.WriteLine(csv);
}
```

### Example 2: Formatting multiple plugins and writing to a file

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PluginEngine.Formatters;

public async Task ExportAllPluginsAsync(IEnumerable<Plugin> plugins)
{
    var formatter = new CsvPluginFormatter();
    string csv = await formatter.FormatPluginsAsync(plugins);
    await File.WriteAllTextAsync("plugins.csv", csv);
}
```

## Notes

- **Null arguments** – All methods throw `ArgumentNullException` when a required parameter is `null`. Always validate inputs before calling.
- **Empty collections** – `FormatPluginsAsync` accepts an empty `IEnumerable<Plugin>` and returns a CSV string containing only the header row.
- **Exception behavior** – `InvalidOperationException` is thrown when plugin data is incomplete or inconsistent. Callers should handle this exception to avoid partial output.
- **Thread safety** – Instances of `CsvPluginFormatter` are not guaranteed to be thread-safe. If the same instance is used concurrently from multiple threads, external synchronization (e.g., a lock) is required. For typical single-threaded or sequential async usage, no additional synchronization is needed.
