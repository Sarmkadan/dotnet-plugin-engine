# XmlPluginFormatter

The `XmlPluginFormatter` class provides asynchronous methods for converting plugin-related data into XML-formatted strings. It is intended for use in scenarios where plugin metadata, reports, or health information need to be serialized to XML for storage, transmission, or display.

## API

### FormatPluginAsync
- **Purpose**: Serializes a single plugin's metadata into an XML string.
- **Parameters**: 
  - `plugin`: The plugin object to format. Must not be null.
- **Return Value**: A `Task<string>` that completes with the XML representation of the plugin.
- **Exceptions**: 
  - `ArgumentNullException` if `plugin` is null.
  - `InvalidOperationException` if the plugin data cannot be serialized to XML.

### FormatPluginsAsync
- **Purpose**: Serializes a collection of plugins into a single XML string.
- **Parameters**: 
  - `plugins`: An enumerable collection of plugin objects to format. Must not be null; individual items must not be null.
- **Return Value**: A `Task<string>` that completes with the XML representation of the plugin collection.
- **Exceptions**: 
  - `ArgumentNullException` if `plugins` is null.
  - `InvalidOperationException` if any plugin in the collection cannot be serialized.

### FormatDetailedReportAsync
- **Purpose**: Serializes a detailed execution report into an XML string.
- **Parameters**: 
  - `report`: The detailed report object containing execution data. Must not be null.
- **Return Value**: A `Task<string>` that completes with the XML representation of the report.
- **Exceptions**: 
  - `ArgumentNullException` if `report` is null.
  - `InvalidOperationException` if the report data cannot be serialized to XML.

### FormatHealthReportAsync
- **Purpose**: Serializes a health status report into an XML string.
- **Parameters**: 
  - `healthReport`: The health report object containing status information. Must not be null.
- **Return Value**: A `Task<string>` that completes with the XML representation of the health report.
- **Exceptions**: 
  - `ArgumentNullException` if `healthReport` is null.
  - `InvalidOperationException` if the health report data cannot be serialized to XML.

## Usage

```csharp
using System.Threading.Tasks;
using DotNetPluginEngine.Models;

// Example 1: Format a single plugin
var formatter = new XmlPluginFormatter();
Plugin myPlugin = GetPluginFromSomewhere();
string pluginXml = await formatter.FormatPluginAsync(myPlugin);
// pluginXml now contains the XML for myPlugin
```

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetPluginEngine.Models;

// Example 2: Format multiple plugins and a detailed report
var formatter = new XmlPluginFormatter();
IEnumerable<Plugin> plugins = GetPluginList();
string pluginsXml = await formatter.FormatPluginsAsync(plugins);

DetailedReport report = GenerateDetailedReport();
string reportXml = await formatter.FormatDetailedReportAsync(report);
// pluginsXml and reportXml contain the respective XML outputs
```

## Notes
- All methods are stateless and do not modify the supplied input objects; they are safe to invoke concurrently from multiple threads as long as the caller ensures that the passed-in arguments are not mutated during execution.
- Passing null for any required argument will result in an `ArgumentNullException`.
- If the input data contains characters that are not valid in XML (e.g., illegal control characters), the serialization may throw an `InvalidOperationException`; callers should sanitize data where appropriate.
- Empty collections passed to `FormatPluginsAsync` will produce a valid XML string representing an empty list.
- The returned XML strings use UTF‑8 encoding; callers should treat the result as plain text unless further processing is required.
