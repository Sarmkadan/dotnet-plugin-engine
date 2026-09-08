# Plugin capability extensions

`PluginCapabilityExtensions` provides convenience methods for formatting and comparing `PluginCapability` instances and for checking several required tags at once. The extensions are in the `PluginEngine.Domain.Entities` namespace.

## Signatures

| Method | Signature | Returns |
| --- | --- | --- |
| `GetDisplayNameWithVersion` | `public static string GetDisplayNameWithVersion(this PluginCapability capability)` | The result of `capability.GetDisplayName()`, followed by ` v` and `capability.Version`. |
| `HasAllTags` | `public static bool HasAllTags(this PluginCapability capability, IEnumerable<string> requiredTags)` | `true` when every required tag is present; otherwise, `false`. |
| `IsInterfaceCompatible` | `public static bool IsInterfaceCompatible(this PluginCapability capability, PluginCapability other)` | `true` when the two `InterfaceTypeName` strings are equal; otherwise, `false`. |

## `GetDisplayNameWithVersion`

This method calls `PluginCapability.GetDisplayName()` and then appends the capability version prefixed with ` v`. Because `GetDisplayName()` itself formats its result as `"{Name} v{Version}"`, the version appears twice in the extension method's result. A null capability causes an `ArgumentNullException`.

```csharp
using PluginEngine.Domain.Entities;

var capability = new PluginCapability
{
    Name = "Search",
    Version = "2.1.0"
};

string displayName = capability.GetDisplayNameWithVersion();
Console.WriteLine(displayName); // Search v2.1.0 v2.1.0
```

## `HasAllTags`

`HasAllTags` enumerates `requiredTags` and calls `PluginCapability.HasTag` for each value. Tag matching is case-insensitive because `HasTag` uses `StringComparison.OrdinalIgnoreCase`. The method returns `false` as soon as a required tag is absent. If `requiredTags` is empty, it returns `true`.

A null capability or null `requiredTags` causes an `ArgumentNullException`. A null or empty string encountered in `requiredTags` causes an `ArgumentException`. Whitespace-only strings are not rejected as empty, but normally do not match a stored tag because stored tags are trimmed and blank entries are removed by `GetTags()`.

```csharp
var capability = new PluginCapability
{
    Tags = "search, indexed, public"
};

bool supportsSearch = capability.HasAllTags(new[] { "SEARCH", "indexed" }); // true
bool isInternal = capability.HasAllTags(new[] { "search", "internal" });   // false
bool acceptsNoRequirements = capability.HasAllTags(Array.Empty<string>()); // true
```

## `IsInterfaceCompatible`

This method compares `capability.InterfaceTypeName` and `other.InterfaceTypeName` with the `string` equality operator. The comparison is exact and case-sensitive; it does not resolve or inspect the interface types. A null value for either capability causes an `ArgumentNullException`.

```csharp
var first = new PluginCapability
{
    InterfaceTypeName = "Example.Contracts.ISearch"
};

var sameInterface = new PluginCapability
{
    InterfaceTypeName = "Example.Contracts.ISearch"
};

var differentCase = new PluginCapability
{
    InterfaceTypeName = "example.contracts.isearch"
};

bool compatible = first.IsInterfaceCompatible(sameInterface);       // true
bool caseSensitive = first.IsInterfaceCompatible(differentCase);    // false
```
