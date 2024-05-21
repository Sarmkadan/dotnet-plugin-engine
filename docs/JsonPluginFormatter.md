# JsonPluginFormatter

A utility class for formatting plugin-related data into JSON strings, enabling consistent serialization of plugin lists, detailed reports, and health diagnostics.

## API

### FormatPluginAsync
Formats a single plugin's metadata into a JSON string.

- **Parameters**
  - `plugin`: The plugin instance to serialize.
- **Return value**
  - A `Task<string>` resolving to the JSON representation of the plugin.
- **Exceptions**
  - Throws `ArgumentNullException` if `plugin` is `null`.

### FormatPluginsAsync
Formats a collection of plugins into a JSON array string.

- **Parameters**
  - `plugins`: The sequence of plugins to serialize.
- **Return value**
  - A `Task<string>` resolving to a JSON array of plugin objects.
- **Exceptions**
  - Throws `ArgumentNullException` if `plugins` is `null`.

### FormatDetailedReportAsync
Formats a detailed report of plugin states and diagnostics into JSON.

- **Parameters**
  - `report`: The detailed report object to serialize.
- **Return value**
  - A `Task<string>` resolving to the JSON representation of the report.
- **Exceptions**
  - Throws `ArgumentNullException` if `report` is `null`.

### FormatHealthReportAsync
Formats a health report summarizing plugin statuses into JSON.

- **Parameters**
  - `healthReport`: The health report object to serialize.
- **Return value**
  - A `Task<string>` resolving to the JSON representation of the health report.
- **Exceptions**
  - Throws `ArgumentNullException` if `healthReport` is `null`.

## Usage
