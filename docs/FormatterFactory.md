# FormatterFactory and PluginHealthInfo

`FormatterFactory` selects one of the built-in `IPluginFormatter` implementations by a string key. `PluginHealthInfo` is the data model passed to `IPluginFormatter.FormatHealthReportAsync`.

Both types are declared in the `PluginEngine.Formatters` namespace.

## FormatterFactory API

| Member | Signature | Behavior |
| --- | --- | --- |
| Constructor | `FormatterFactory(JsonPluginFormatter json, CsvPluginFormatter csv, XmlPluginFormatter xml)` | Stores the supplied formatter instances under the `json`, `csv`, and `xml` keys. |
| Get a formatter | `IPluginFormatter? GetFormatter(string formatType)` | Converts `formatType` to lowercase with invariant casing and returns the matching formatter. Returns `null` for an unsupported key. |
| List formats | `IEnumerable<string> GetSupportedFormats()` | Returns the keys held by the factory. |

The supported keys are:

| Key | Returned implementation |
| --- | --- |
| `csv` | `CsvPluginFormatter` |
| `json` | `JsonPluginFormatter` |
| `xml` | `XmlPluginFormatter` |

Lookup is case-insensitive for these keys because `GetFormatter` calls `ToLowerInvariant`. For example, `"JSON"`, `"Json"`, and `"json"` all select the same `JsonPluginFormatter` instance supplied to the constructor. The method does not trim whitespace, so `" json "` is unsupported. Passing `null` causes the call to `ToLowerInvariant` to throw; the implementation has no explicit null check.

`GetSupportedFormats` exposes the dictionary keys. The current constructor inserts them in `json`, `csv`, `xml` order, but callers should use the returned values as supported identifiers rather than depend on their ordering.

## PluginHealthInfo

`PluginHealthInfo` is a sealed mutable class used by the formatter health-report method:

```csharp
Task<string> FormatHealthReportAsync(PluginHealthInfo health);
```

| Property | Signature | Initialization in the type |
| --- | --- | --- |
| Plugin ID | `required Guid PluginId { get; set; }` | Required from the object initializer or constructor. |
| Plugin name | `required string PluginName { get; set; }` | Required from the object initializer or constructor. |
| Status | `required string Status { get; set; }` | Required from the object initializer or constructor. |
| Dependency count | `required int DependencyCount { get; set; }` | Required from the object initializer or constructor. |
| Capability count | `required int CapabilityCount { get; set; }` | Required from the object initializer or constructor. |
| Load time | `long LoadTimeMs { get; set; }` | Default value of `0`. |
| Last access | `DateTime LastAccessedUtc { get; set; }` | Default `DateTime` value. |
| Health flag | `bool IsHealthy { get; set; }` | Default value of `false`. |
| Issues | `List<string> Issues { get; set; }` | Initialized to an empty list. |

The model itself contains no validation or derived health calculation. Callers provide the status, counts, timing, health flag, and issues that a formatter renders.

## Dependency injection registration

`ServiceCollectionExtensions.AddPluginEngineStack` registers `JsonPluginFormatter`, `CsvPluginFormatter`, `XmlPluginFormatter`, and `FormatterFactory` as concrete singleton services. Resolving the factory therefore injects the registered singleton formatter instances, and repeated successful lookups return those same instances.

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Formatters;
using PluginEngine.Utils.Extensions;

var services = new ServiceCollection();
services.AddPluginEngineStack();

await using ServiceProvider provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<FormatterFactory>();

IPluginFormatter formatter = factory.GetFormatter("JSON")
    ?? throw new InvalidOperationException("Unsupported format.");

var health = new PluginHealthInfo
{
    PluginId = Guid.NewGuid(),
    PluginName = "Example plugin",
    Status = "Loaded",
    DependencyCount = 2,
    CapabilityCount = 1,
    LoadTimeMs = 25,
    LastAccessedUtc = DateTime.UtcNow,
    IsHealthy = true
};

string report = await formatter.FormatHealthReportAsync(health);
```

The registration adds the concrete formatter types, not `IPluginFormatter` service mappings. Consumers select an `IPluginFormatter` through `FormatterFactory` after resolving the factory.
