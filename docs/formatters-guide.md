# Plugin formatter guide

The formatter API provides CSV, JSON, and XML representations of plugin data. All three implementations implement `IPluginFormatter` and return their result as a `Task<string>`.

## `IPluginFormatter` contract

| Member | Signature | Purpose |
| --- | --- | --- |
| Format type | `string FormatType { get; }` | Identifies the implementation's format. The built-in values are `csv`, `json`, and `xml`. |
| Single plugin | `Task<string> FormatPluginAsync(Plugin plugin)` | Formats one plugin as a summary. |
| Plugin collection | `Task<string> FormatPluginsAsync(IEnumerable<Plugin> plugins)` | Formats multiple plugins. |
| Detailed report | `Task<string> FormatDetailedReportAsync(Plugin plugin)` | Formats plugin metadata, dependencies, and capabilities. |
| Health report | `Task<string> FormatHealthReportAsync(PluginHealthInfo health)` | Formats plugin statistics and health information. |

The implementations currently build their output synchronously and wrap the resulting string with `Task.FromResult`. The interface does not accept a cancellation token.

## Choosing a formatter

| Formatter | `FormatType` | Output characteristics |
| --- | --- | --- |
| `CsvPluginFormatter` | `csv` | Header-and-row output suited to tabular consumers. Plugin names and other string values are quoted; embedded double quotes are doubled. |
| `JsonPluginFormatter` | `json` | Indented JSON with camel-cased property names. Health output includes issues and a report-generation timestamp. |
| `XmlPluginFormatter` | `xml` | Indented XML with Pascal-cased elements. Health issues are emitted only when the issue list is non-empty. |

`FormatterFactory` is constructed with one instance of each built-in formatter. `GetFormatter` converts the requested name to lowercase using invariant casing, so names such as `JSON` and `Json` select the JSON formatter. An unsupported name returns `null`. `GetSupportedFormats()` exposes the registered keys: `json`, `csv`, and `xml`.

```csharp
using PluginEngine.Formatters;

var factory = new FormatterFactory(
    new JsonPluginFormatter(),
    new CsvPluginFormatter(),
    new XmlPluginFormatter());

IPluginFormatter formatter = factory.GetFormatter("JSON")
    ?? throw new InvalidOperationException("Unsupported formatter.");

string output = await formatter.FormatPluginAsync(plugin);
```

## `FormatPluginAsync` output

Given a plugin with ID `11111111-1111-1111-1111-111111111111`, name `Example`, version `1.2.3`, status `Loaded`, modification time `2026-01-02T03:04:05.0000000Z`, two dependencies, and one capability, the output has the following shape.

### CSV

```csv
ID,Name,Version,Status,LoadedAtUtc,Dependencies,Capabilities
"11111111-1111-1111-1111-111111111111","Example","1.2.3","Loaded","2026-01-02T03:04:05.0000000Z",2,1
```

The method always writes the header followed by one plugin row. Although the column is named `LoadedAtUtc`, its value comes from `Plugin.ModifiedAt` formatted with the round-trip (`O`) format.

### JSON

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "Example",
  "version": "1.2.3",
  "status": "Loaded",
  "loadedAtUtc": "2026-01-02T03:04:05Z",
  "dependencyCount": 2,
  "capabilityCount": 1
}
```

The output is an indented object. `status` is produced with `Plugin.Status.ToString()`, and `loadedAtUtc` is serialized from `Plugin.ModifiedAt`.

### XML

```xml
<?xml version="1.0" encoding="utf-16"?>
<Plugin>
  <Id>11111111-1111-1111-1111-111111111111</Id>
  <Name>Example</Name>
  <Version>1.2.3</Version>
  <Status>Loaded</Status>
  <LoadedAt>2026-01-02T03:04:05.0000000Z</LoadedAt>
  <DependencyCount>2</DependencyCount>
  <CapabilityCount>1</CapabilityCount>
</Plugin>
```

The XML document uses a `Plugin` root. The modification time is named `LoadedAt` and formatted with the round-trip (`O`) format. Because the implementation writes through a `StringWriter`, its XML declaration reports UTF-16.

## `FormatHealthReportAsync` output

For health information with the same ID and name, status `Loaded`, `IsHealthy` set to `false`, dependency and capability counts of `2` and `1`, a load time of `42` ms, last access at `2026-01-02T04:05:06.0000000Z`, and one issue (`Dependency unavailable`), the output has the following shape.

### CSV

```csv
PluginId,PluginName,Status,IsHealthy,DependencyCount,CapabilityCount,LoadTimeMs,LastAccessedUtc
"11111111-1111-1111-1111-111111111111","Example","Loaded","False",2,1,42,"2026-01-02T04:05:06.0000000Z"
```

CSV health output contains one header and one data row. It does not include `PluginHealthInfo.Issues`.

### JSON

```json
{
  "plugin": {
    "id": "11111111-1111-1111-1111-111111111111",
    "name": "Example",
    "status": "Loaded"
  },
  "health": {
    "isHealthy": false,
    "dependencyCount": 2,
    "capabilityCount": 1,
    "loadTimeMs": 42,
    "lastAccessedUtc": "2026-01-02T04:05:06Z"
  },
  "issues": [
    "Dependency unavailable"
  ],
  "reportGeneratedAtUtc": "<current UTC time>"
}
```

JSON groups identity fields under `plugin` and statistics under `health`. It always includes the `issues` array, even when empty, and sets `reportGeneratedAtUtc` from `DateTime.UtcNow` when the method runs.

### XML

```xml
<?xml version="1.0" encoding="utf-16"?>
<HealthReport>
  <Plugin>
    <Id>11111111-1111-1111-1111-111111111111</Id>
    <Name>Example</Name>
    <Status>Loaded</Status>
  </Plugin>
  <Health>
    <IsHealthy>False</IsHealthy>
    <DependencyCount>2</DependencyCount>
    <CapabilityCount>1</CapabilityCount>
    <LoadTimeMs>42</LoadTimeMs>
    <LastAccessed>2026-01-02T04:05:06.0000000Z</LastAccessed>
  </Health>
  <Issues>
    <Issue>Dependency unavailable</Issue>
  </Issues>
</HealthReport>
```

XML uses separate `Plugin` and `Health` elements. It adds an `Issues` element only when `PluginHealthInfo.Issues` contains at least one item; each item becomes an `Issue` child.
