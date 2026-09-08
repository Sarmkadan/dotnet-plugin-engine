# PluginLoadException extension methods

`PluginLoadExceptionExtensions` provides helpers for inspecting, summarizing,
and copying a `PluginLoadException`. The extension methods are in the
`PluginEngine.Exceptions` namespace.

## Signatures

| Method | Signature | Return value |
| --- | --- | --- |
| `IsLoadStage` | `public static bool IsLoadStage(this PluginLoadException exception, PluginLoadStage stage)` | `true` when `exception.LoadStage` equals `stage`; otherwise, `false`. |
| `GetLoadFailureSummary` | `public static string GetLoadFailureSummary(this PluginLoadException exception)` | A formatted sentence containing the plugin name, assembly path, and load stage. |
| `WithLoadStage` | `public static PluginLoadException WithLoadStage(this PluginLoadException exception, PluginLoadStage stage)` | A new exception with the requested load stage and copied failure details. |

All three methods throw `ArgumentNullException` when `exception` is `null`.

## Behavior

### `IsLoadStage`

`IsLoadStage` compares `exception.LoadStage` directly with the supplied
`PluginLoadStage` value. It returns `true` only when the enum values are equal.

### `GetLoadFailureSummary`

`GetLoadFailureSummary` returns a string in this exact format:

```text
Failed to load plugin '{PluginName}' from '{AssemblyPath}' at stage {LoadStage}.
```

The method inserts the exception's `PluginName`, `AssemblyPath`, and
`LoadStage` values directly into the string. It does not include the exception
message or inner exception.

### `WithLoadStage`

`WithLoadStage` creates and returns a new `PluginLoadException`; it does not
change the original instance. The new exception receives the supplied
`PluginLoadStage` and preserves the original exception's:

- `Message`
- `PluginName`
- `AssemblyPath`
- `InnerException`

## Example

```csharp
using PluginEngine.Exceptions;

var cause = new InvalidOperationException("A dependency could not be resolved.");
var exception = new PluginLoadException(
    "Failed to load",
    "MyPlugin",
    "path/to/plugin.dll",
    PluginLoadStage.AssemblyResolution,
    cause);

bool isAtResolution = exception.IsLoadStage(
    PluginLoadStage.AssemblyResolution); // true

string summary = exception.GetLoadFailureSummary();
Console.WriteLine(summary);
// Failed to load plugin 'MyPlugin' from 'path/to/plugin.dll' at stage AssemblyResolution.

PluginLoadException updated = exception.WithLoadStage(
    PluginLoadStage.TypeLoading);

Console.WriteLine(exception.LoadStage); // AssemblyResolution
Console.WriteLine(updated.LoadStage);   // TypeLoading
Console.WriteLine(ReferenceEquals(exception, updated)); // false
Console.WriteLine(ReferenceEquals(cause, updated.InnerException)); // true
```
