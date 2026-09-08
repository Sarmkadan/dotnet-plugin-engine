# Plugin dependency extensions

`PluginDependencyExtensions` provides helpers for checking a dependency's version range, comparing two dependency ranges, and formatting a dependency for display. The extensions are in the `PluginEngine.Domain.Entities` namespace.

## Signatures

| Method | Signature | Result |
| --- | --- | --- |
| `IsVersionSatisfied` | `public static bool IsVersionSatisfied(this PluginDependency dependency, string version)` | Whether the supplied version falls within the dependency's inclusive bounds. |
| `OverlapsWith` | `public static bool OverlapsWith(this PluginDependency first, PluginDependency second)` | Whether dependencies for the same plugin have overlapping inclusive version ranges. |
| `ToSummary` | `public static string ToSummary(this PluginDependency dependency)` | A concise description of the dependency. |

## `IsVersionSatisfied`

`IsVersionSatisfied` parses `version` as a `System.Version` and compares it with `MinimumVersion` and `MaximumVersion`. Both bounds are inclusive. A null, empty, or whitespace configured bound is treated as unbounded. A configured bound that cannot be parsed is also skipped.

The method throws `ArgumentNullException` when `dependency` or `version` is null. It throws `ArgumentException` when `version` is empty or cannot be parsed as a `Version`.

```csharp
using System;
using PluginEngine.Domain.Entities;

var dependency = new PluginDependency
{
    MinimumVersion = "1.2.0",
    MaximumVersion = "2.0.0"
};

bool supported = dependency.IsVersionSatisfied("1.5.0"); // true
bool tooNew = dependency.IsVersionSatisfied("2.1.0");    // false
```

## `OverlapsWith`

`OverlapsWith` first compares `DependencyPluginId`. It returns `false` immediately when the IDs differ. For matching IDs, the method treats the ranges as inclusive, so ranges that meet at an endpoint overlap.

Each `MinimumVersion` must parse as a `Version`; null, empty, whitespace, or invalid minimum values cause a `FormatException`. A null, empty, or whitespace `MaximumVersion` is an open upper bound, while an invalid non-empty maximum causes a `FormatException`. Passing null for either dependency causes an `ArgumentNullException`.

```csharp
using System;
using PluginEngine.Domain.Entities;

var pluginId = Guid.NewGuid();
var first = new PluginDependency
{
    DependencyPluginId = pluginId,
    MinimumVersion = "1.0.0",
    MaximumVersion = "2.0.0"
};
var second = new PluginDependency
{
    DependencyPluginId = pluginId,
    MinimumVersion = "2.0.0",
    MaximumVersion = "3.0.0"
};

bool overlaps = first.OverlapsWith(second); // true: 2.0.0 is in both ranges
```

## `ToSummary`

`ToSummary` returns text in this form:

```text
{DependencyPluginId} [{minimum-or-any} - {maximum-or-any}] {Optional-or-Required} - {Description}
```

Null, empty, or whitespace version bounds are displayed as `any`. The status is `Optional` when `IsOptional` is true and `Required` otherwise. The description is appended as stored, including when it is empty. No version parsing or validation is performed. A null dependency causes an `ArgumentNullException`.

```csharp
using System;
using PluginEngine.Domain.Entities;

var dependency = new PluginDependency
{
    DependencyPluginId = Guid.Parse("d2719d1b-652b-4a25-a435-bbfb616f8f95"),
    MinimumVersion = "1.0.0",
    MaximumVersion = string.Empty,
    IsOptional = true,
    Description = "Adds reporting support"
};

string summary = dependency.ToSummary();
// d2719d1b-652b-4a25-a435-bbfb616f8f95 [1.0.0 - any] Optional - Adds reporting support
```
