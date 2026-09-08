# JSON extensions

The `*JsonExtensions` classes under `src/PluginEngine` provide a small, type-specific wrapper around `System.Text.Json`. Each class exposes the same general operation set: serialize an instance with `ToJson`, deserialize with `FromJson`, and attempt deserialization with `TryFromJson`. The methods are static; only `ToJson` uses extension-method syntax.

All eleven classes start with `JsonSerializerDefaults.Web`, explicitly use camel-case property names, and emit compact JSON by default. Passing `indented: true` to `ToJson` uses a copy of the class's shared options with `WriteIndented` enabled, so the shared options are not modified.

## Signatures

`T` in the common form below is the type named in the **Target type** column. Nullable annotations and parameter validation differ where shown.

| Extension class | Target type | `ToJson` | `FromJson` | `TryFromJson` |
| --- | --- | --- | --- | --- |
| `PluginJsonExtensions` | `Plugin` | `string ToJson(this Plugin value, bool indented = false)` | `Plugin? FromJson(string json)` | `bool TryFromJson(string json, out Plugin? value)` |
| `PluginAssemblyJsonExtensions` | `PluginAssembly` | `string ToJson(this PluginAssembly value, bool indented = false)` | `PluginAssembly? FromJson(string json)` | `bool TryFromJson(string json, out PluginAssembly? value)` |
| `WebhookConfigurationJsonExtensions` | `WebhookConfiguration` | `string ToJson(this WebhookConfiguration value, bool indented = false)` | `WebhookConfiguration? FromJson(string json)` | `bool TryFromJson(string json, out WebhookConfiguration? value)` |
| `VersionMismatchExceptionJsonExtensions` | `VersionMismatchException` | `string ToJson(this VersionMismatchException value, bool indented = false)` | `VersionMismatchException? FromJson(string? json)` | `bool TryFromJson(string? json, out VersionMismatchException? value)` |
| `RateLimitMiddlewareJsonExtensions` | `RateLimitMiddleware` | `string ToJson(this RateLimitMiddleware value, bool indented = false)` | `RateLimitMiddleware? FromJson(string json)` | `bool TryFromJson(string json, out RateLimitMiddleware? value)` |
| `TypeExtensionsJsonExtensions` | `Type` | `string ToJson(this Type type, bool indented = false)` | `Type? FromJson(string json)` | `bool TryFromJson(string json, out Type? type)` |
| `FileSystemHelperJsonExtensions` | `FileSystemHelper` | `string ToJson(this FileSystemHelper value, bool indented = false)` | `FileSystemHelper? FromJson(string json)` | `bool TryFromJson(string json, out FileSystemHelper? value)` |
| `PluginDiscoveryServiceJsonExtensions` | `PluginDiscoveryService` | `string ToJson(this PluginDiscoveryService value, bool indented = false)` | `PluginDiscoveryService FromJson(string json)` | `bool TryFromJson(string json, out PluginDiscoveryService? value)` |
| `PluginManagerServiceJsonExtensions` | `PluginManagerService` | `string ToJson(this PluginManagerService value, bool indented = false)` | `PluginManagerService FromJson(string json)` | `bool TryFromJson(string json, out PluginManagerService? value)` |
| `PluginDependencyResolverJsonExtensions` | `PluginDependencyResolver` | `string ToJson(this PluginDependencyResolver value, bool indented = false)` | `PluginDependencyResolver? FromJson(string json)` | `bool TryFromJson(string json, out PluginDependencyResolver? value)` |
| `CsvPluginFormatterJsonExtensions` | `CsvPluginFormatter` | `string ToJson(this CsvPluginFormatter value, bool indented = false)` | `CsvPluginFormatter? FromJson(string json)` | `bool TryFromJson(string json, out CsvPluginFormatter? value)` |

## Common pattern and differences

`ToJson` delegates to `JsonSerializer.Serialize`. Every implementation except `PluginDiscoveryServiceJsonExtensions.ToJson` explicitly rejects a null value with `ArgumentNullException`; the discovery-service implementation passes its value directly to the serializer.

`FromJson` delegates to `JsonSerializer.Deserialize<T>`, but its error contract is class-specific:

- `Plugin`, `PluginAssembly`, `WebhookConfiguration`, `VersionMismatchException`, `FileSystemHelper`, and `RateLimitMiddleware` return `null` for malformed JSON. `VersionMismatchException` returns `null` for null, empty, or whitespace input; `RateLimitMiddleware` rejects null but returns `null` for empty or whitespace input. `PluginAssembly` rejects null or whitespace before deserializing, while `WebhookConfiguration` and `FileSystemHelper` reject null or empty input.
- `CsvPluginFormatter` rejects null or empty input, returns `null` for whitespace, and lets `JsonException` propagate for malformed non-whitespace JSON.
- `Type` rejects null or empty input, returns `null` for whitespace or an unresolved type name, and lets malformed JSON raise `JsonException`. Its JSON representation is the type's assembly-qualified name rather than the properties of `System.Type`.
- `PluginDependencyResolver` rejects null or empty input and lets deserialization exceptions propagate. A JSON `null` can produce a null result.
- `PluginManagerService` and `PluginDiscoveryService` return non-nullable results. They reject null or empty input, propagate malformed-JSON errors, and throw `JsonException` if deserialization returns `null`.

`TryFromJson` writes the result to an `out` parameter and normally returns `false` after catching `JsonException`, but there are visible exceptions to that convention:

- `Plugin` returns `false` for a null string, malformed JSON, or a null deserialization result.
- `PluginAssembly` returns `false` for null, empty, or whitespace input without throwing.
- `VersionMismatchException` and `RateLimitMiddleware` return `false` for empty or whitespace input. `RateLimitMiddleware` rejects a null string first.
- `WebhookConfiguration`, `FileSystemHelper`, and `CsvPluginFormatter` reject null or empty input. Their implementations return `true` when `JsonSerializer.Deserialize` does not throw, even if its result is `null`.
- `Type`, `PluginDependencyResolver`, and `PluginManagerService` reject null or empty input and return `true` only for a non-null result. `Type` also requires the deserialized name to resolve through `Type.GetType`.
- `PluginDiscoveryService` rejects null or empty input but does not catch `JsonException`; it returns `true` only for a non-null result.

## Serializer options

The following table lists options explicitly configured in addition to the `JsonSerializerDefaults.Web` constructor preset.

| Extension class | Explicit options |
| --- | --- |
| `PluginJsonExtensions` | camel-case names; compact output; ignore null properties; ignore reference cycles |
| `PluginAssemblyJsonExtensions` | camel-case names; compact output; ignore null properties; serialize enums as camel-case strings |
| `WebhookConfigurationJsonExtensions` | camel-case names; compact output; default reflection-based type-info resolver; ignore reference cycles; disallow unmapped JSON members; ignore null properties |
| `VersionMismatchExceptionJsonExtensions` | camel-case names; compact output |
| `RateLimitMiddlewareJsonExtensions` | camel-case names; compact output; ignore null properties |
| `TypeExtensionsJsonExtensions` | camel-case names; compact output; default reflection-based type-info resolver; ignore reference cycles |
| `FileSystemHelperJsonExtensions` | camel-case names; compact output; ignore null properties; ignore reference cycles |
| `PluginDiscoveryServiceJsonExtensions` | camel-case names; compact output; ignore null properties; ignore reference cycles |
| `PluginManagerServiceJsonExtensions` | camel-case names; compact output; default reflection-based type-info resolver; ignore reference cycles; allow named floating-point literals |
| `PluginDependencyResolverJsonExtensions` | camel-case names; compact output; default reflection-based type-info resolver; ignore reference cycles; allow named floating-point literals |
| `CsvPluginFormatterJsonExtensions` | camel-case names; compact output |

## Example

The following example uses the `Plugin` extensions. Serialization uses extension syntax, while deserialization calls the static class methods.

```csharp
using PluginEngine.Domain.Entities;

var plugin = new Plugin
{
    Id = Guid.NewGuid(),
    Name = "SamplePlugin",
    Version = "1.0.0",
    AssemblyPath = "plugins/SamplePlugin.dll"
};

string json = plugin.ToJson(indented: true);

Plugin? restored = PluginJsonExtensions.FromJson(json);

if (PluginJsonExtensions.TryFromJson(json, out Plugin? parsed))
{
    Console.WriteLine(parsed.Name);
}
```
