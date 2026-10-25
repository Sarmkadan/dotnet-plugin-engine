# PluginHealthInfo

`PluginHealthInfo` is a sealed, mutable data class that carries the health snapshot of a single plugin into the `IPluginFormatter.FormatHealthReportAsync` method. It is declared in `src/PluginEngine/Formatters/IPluginFormatter.cs` in the `PluginEngine.Formatters` namespace, alongside `IPluginFormatter` and `FormatterFactory`.

The class holds data only. It performs no validation and computes no health on its own: callers fill in the status, counts, timing, health flag, and issues, and the formatter renders them as given.

## API

### `public sealed class PluginHealthInfo`

- **Members**
  - `PluginId`, `PluginName`, `Status`, `DependencyCount`, `CapabilityCount`: `required` (must be set in an object initializer or the compiler reports an error).
  - `LoadTimeMs`, `LastAccessedUtc`, `IsHealthy`, `Issues`: optional, with the defaults listed below.

| Property | Type | Required | Default | Meaning |
| --- | --- | --- | --- | --- |
| `PluginId` | `Guid` | Yes | none | Unique identifier of the plugin. |
| `PluginName` | `string` | Yes | none | Display name of the plugin. |
| `Status` | `string` | Yes | none | Free-form status label of the plugin (for example `"Loaded"` or `"Unloaded"`). The type does not restrict the values. |
| `DependencyCount` | `int` | Yes | none | Number of dependencies the plugin declares. |
| `CapabilityCount` | `int` | Yes | none | Number of capabilities the plugin exposes. |
| `LoadTimeMs` | `long` | No | `0` | Time taken to load the plugin, in milliseconds. |
| `LastAccessedUtc` | `DateTime` | No | `default(DateTime)` | Time the plugin was last accessed, in UTC. The type does not enforce `DateTimeKind.Utc`; callers must supply UTC values. |
| `IsHealthy` | `bool` | No | `false` | Health flag supplied by the caller. |
| `Issues` | `List<string>` | No | empty list | Human-readable descriptions of problems found with the plugin. Initialized to `[]`, so it is never `null` unless the caller assigns `null`. |

Because `IsHealthy` defaults to `false`, an object that never sets it is reported as unhealthy. Set it explicitly.

## Extension methods

`PluginHealthInfoExtensions` (`src/PluginEngine/Formatters/PluginHealthInfoExtensions.cs`) adds convenience checks to `PluginHealthInfo`. Each method throws `ArgumentNullException` when `health` is `null`.

### `public static bool HasIssues(this PluginHealthInfo health)`

- **Returns**
  - `true` if `Issues` contains at least one entry; otherwise, `false`.

---

### `public static string GetIssueSummary(this PluginHealthInfo health)`

- **Returns**
  - The entries of `Issues` joined with `"; "`, or `string.Empty` if there are none.

---

### `public static bool IsStale(this PluginHealthInfo health, TimeSpan maxAge)`

- **Parameters**
  - `maxAge`: The maximum acceptable time since `LastAccessedUtc`.
- **Returns**
  - `true` if `DateTime.UtcNow - LastAccessedUtc` is greater than `maxAge`; otherwise, `false`.

## Rendering with FormatHealthReportAsync

```csharp
Task<string> FormatHealthReportAsync(PluginHealthInfo health);
```

Each built-in formatter renders the same model in its own format. None of them validates the input: passing `null` throws a `NullReferenceException` from the method body rather than an `ArgumentNullException`.

### JSON (`JsonPluginFormatter`)

Produces indented JSON with the following shape. Property names are lowercase camel case. `issues` is always present, and is an empty array when there are no issues.

| JSON path | Source |
| --- | --- |
| `plugin.id` | `PluginId` (serialized as a GUID string) |
| `plugin.name` | `PluginName` |
| `plugin.status` | `Status` |
| `health.isHealthy` | `IsHealthy` |
| `health.dependencyCount` | `DependencyCount` |
| `health.capabilityCount` | `CapabilityCount` |
| `health.loadTimeMs` | `LoadTimeMs` |
| `health.lastAccessedUtc` | `LastAccessedUtc` (ISO 8601) |
| `issues` | `Issues` |
| `reportGeneratedAtUtc` | `DateTime.UtcNow` at the moment of formatting; not part of the model |

### CSV (`CsvPluginFormatter`)

Produces a header row followed by one data row:

```
PluginId,PluginName,Status,IsHealthy,DependencyCount,CapabilityCount,LoadTimeMs,LastAccessedUtc
```

- `PluginId`, `PluginName`, `Status`, `IsHealthy`, and `LastAccessedUtc` are wrapped in double quotes. `LastAccessedUtc` uses the round-trip `"O"` format.
- `DependencyCount`, `CapabilityCount`, and `LoadTimeMs` are written unquoted.
- `Issues` is **not** rendered, and no report timestamp is included.
- Only `PluginName` is passed through the internal CSV escaping routine. `Status` is written as-is, so a value containing a double quote produces malformed CSV.

### XML (`XmlPluginFormatter`)

Produces an indented XML document with a `HealthReport` root element:

- `Plugin` contains `Id`, `Name`, and `Status`.
- `Health` contains `IsHealthy`, `DependencyCount`, `CapabilityCount`, `LoadTimeMs`, and `LastAccessed` (round-trip `"O"` format).
- `Issues` contains one `Issue` element per entry. The element is written **only when `Issues` is non-empty**.
- No report timestamp is included.

## Example

```csharp
using PluginEngine.Formatters;

var health = new PluginHealthInfo
{
    PluginId = Guid.Parse("3f2b8c1e-6a4d-4e2f-9b7a-1c2d3e4f5a6b"),
    PluginName = "Invoice Exporter",
    Status = "Loaded",
    DependencyCount = 3,
    CapabilityCount = 2,
    LoadTimeMs = 42,
    LastAccessedUtc = new DateTime(2026, 10, 25, 9, 30, 0, DateTimeKind.Utc),
    IsHealthy = false,
    Issues = ["Dependency 'Newtonsoft.Json' 13.0.1 could not be resolved", "No access in the last 24 hours"]
};

var factory = new FormatterFactory(
    new JsonPluginFormatter(),
    new CsvPluginFormatter(),
    new XmlPluginFormatter());

IPluginFormatter formatter = factory.GetFormatter("json")
    ?? throw new InvalidOperationException("Unsupported format.");

string json = await formatter.FormatHealthReportAsync(health);
Console.WriteLine(json);

Console.WriteLine(health.HasIssues());           // True
Console.WriteLine(health.GetIssueSummary());     // Dependency 'Newtonsoft.Json' 13.0.1 could not be resolved; No access in the last 24 hours
Console.WriteLine(health.IsStale(TimeSpan.FromDays(1)));
```

Output of the JSON formatter for this input (the `reportGeneratedAtUtc` value depends on when the report is generated):

```json
{
  "plugin": {
    "id": "3f2b8c1e-6a4d-4e2f-9b7a-1c2d3e4f5a6b",
    "name": "Invoice Exporter",
    "status": "Loaded"
  },
  "health": {
    "isHealthy": false,
    "dependencyCount": 3,
    "capabilityCount": 2,
    "loadTimeMs": 42,
    "lastAccessedUtc": "2026-10-25T09:30:00Z"
  },
  "issues": [
    "Dependency 'Newtonsoft.Json' 13.0.1 could not be resolved",
    "No access in the last 24 hours"
  ],
  "reportGeneratedAtUtc": "2026-10-25T12:00:00.0000000Z"
}
```

The JSON encoder escapes the apostrophes in the first issue as `'`; parsers decode them back to `'`.

Switching to `"csv"` or `"xml"` on the same factory renders the same model in those formats; in both cases the two issue strings are omitted from CSV and included under `Issues` in XML.

---

## FormatterFactory

`FormatterFactory` is a sealed class that maps a format key to one of the three built-in formatters. It is declared in the same file as `PluginHealthInfo`.

### `public FormatterFactory(JsonPluginFormatter json, CsvPluginFormatter csv, XmlPluginFormatter xml)`

Stores the supplied formatters under the keys `json`, `csv`, and `xml`.

---

### `public IPluginFormatter? GetFormatter(string formatType)`

Looks up a formatter by key. The key is converted with `ToLowerInvariant()`, so lookup is case-insensitive. Whitespace is not trimmed.

- **Parameters**
  - `formatType`: The format key, such as `"json"`, `"CSV"`, or `"xml"`.
- **Returns**
  - The matching formatter, or `null` if the key is not supported.
- **Throws**
  - `NullReferenceException`: If `formatType` is `null`. The method has no explicit null check.

---

### `public IEnumerable<string> GetSupportedFormats()`

- **Returns**
  - The supported keys: `json`, `csv`, and `xml`.
- **Throws**
  - None.

## Usage

Use `GetFormatter` to select a formatter from user input, then pass a populated `PluginHealthInfo` to `FormatHealthReportAsync`, as shown in the example above. Set `IsHealthy` and `Issues` explicitly; the formatters render them as provided and do not derive them from the other fields.
