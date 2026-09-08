# PluginIncompatibleException extension methods

`PluginIncompatibleExceptionExtensions` provides helper methods for inspecting a
`PluginIncompatibleException` and formatting its stored version information. The
extensions are in the `PluginEngine.Exceptions` namespace.

## Signatures

| Method | Signature | Return value |
| --- | --- | --- |
| `IsVersionIncompatible` | `public static bool IsVersionIncompatible(this PluginIncompatibleException exception)` | `true` when `HostEngineVersion` or `DeclaredConstraint` is `null` or empty; otherwise `false`. |
| `GetIncompatibilityReason` | `public static string GetIncompatibilityReason(this PluginIncompatibleException exception)` | A sentence containing the declared constraint and host engine version. |
| `HasDeclaredConstraint` | `public static bool HasDeclaredConstraint(this PluginIncompatibleException exception)` | `true` when `DeclaredConstraint` is neither `null` nor empty; otherwise `false`. |

All three methods throw `ArgumentNullException` when `exception` is `null`.

## Behavior

### `IsVersionIncompatible`

This method returns the result of checking whether either
`exception.HostEngineVersion` or `exception.DeclaredConstraint` is null or the
empty string. It does not parse either value or compare version numbers and
version constraints. If both properties contain non-empty strings, it returns
`false`.

Because the check uses `string.IsNullOrEmpty`, a whitespace-only string is
treated as present rather than empty.

### `GetIncompatibilityReason`

This method returns a string with the following format:

```text
Plugin {DeclaredConstraint} is incompatible with host engine version {HostEngineVersion}
```

The property values are inserted directly. A `null` value is rendered as an
empty interpolation value; the method does not substitute a placeholder or
validate the version strings.

### `HasDeclaredConstraint`

This method returns `true` when `DeclaredConstraint` is not null and is not the
empty string. It returns `false` for either of those missing values. As with
`IsVersionIncompatible`, whitespace-only text is considered present.

## Example

```csharp
using PluginEngine.Exceptions;

var exception = new PluginIncompatibleException(
    "ReportingPlugin",
    "[2.0.0,3.0.0)",
    "4.1.0");

bool missingVersionInformation = exception.IsVersionIncompatible(); // false
bool hasConstraint = exception.HasDeclaredConstraint();              // true
string reason = exception.GetIncompatibilityReason();

Console.WriteLine(reason);
// Plugin [2.0.0,3.0.0) is incompatible with host engine version 4.1.0
```

The `false` result from `IsVersionIncompatible` means only that both stored
strings are non-empty. It does not indicate that the declared constraint and
host engine version were evaluated as compatible.
