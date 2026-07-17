# RemotePluginRegistryValidation

Provides static methods to validate the integrity and consistency of a remote plugin registry. It exposes overloads that accept different representations of registry data—such as raw JSON strings, streams, or already-parsed object models—and returns lists of validation errors, boolean validity indicators, or throws on invalid input. The type is designed for pre-flight checks before a registry is used to resolve or load plugins.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(string json)
public static IReadOnlyList<string> Validate(Stream stream)
public static IReadOnlyList<string> Validate(RemotePluginRegistry registry)
public static IReadOnlyList<string> Validate(RemotePluginRegistry registry, ValidationOptions options)
```

Validates the given registry representation and returns a read-only list of error messages. An empty list indicates a valid registry.

- **Parameters**:
  - `json`: A raw JSON string containing the serialized registry.
  - `stream`: A readable stream positioned at the start of the serialized registry data.
  - `registry`: An already-deserialized `RemotePluginRegistry` instance.
  - `options`: A `ValidationOptions` value controlling strictness or specific checks to perform.
- **Returns**: `IReadOnlyList<string>` containing zero or more human-readable error descriptions.
- **Throws**: `ArgumentNullException` if `json`, `stream`, or `registry` is `null`. `JsonException` if the JSON or stream content cannot be parsed as a registry.

### IsValid

```csharp
public static bool IsValid(string json)
public static bool IsValid(Stream stream)
public static bool IsValid(RemotePluginRegistry registry)
public static bool IsValid(RemotePluginRegistry registry, ValidationOptions options)
```

Convenience methods that return `true` if validation produces no errors, `false` otherwise.

- **Parameters**: Same as the corresponding `Validate` overloads.
- **Returns**: `true` when the registry is valid; `false` when one or more validation errors exist.
- **Throws**: Same as `Validate`—`ArgumentNullException` for null inputs, `JsonException` for unparseable data.

### EnsureValid

```csharp
public static void EnsureValid(string json)
public static void EnsureValid(Stream stream)
public static void EnsureValid(RemotePluginRegistry registry)
public static void EnsureValid(RemotePluginRegistry registry, ValidationOptions options)
```

Performs validation and throws an aggregate exception if any errors are found. Use when invalid registry data must halt execution immediately.

- **Parameters**: Same as the corresponding `Validate` overloads.
- **Returns**: Nothing on success.
- **Throws**: `ArgumentNullException` for null inputs. `JsonException` for unparseable data. `RemotePluginRegistryValidationException` (or an `AggregateException` containing validation error strings) when the registry is invalid.

## Usage

### Example 1: Validating a JSON file before loading

```csharp
using var fileStream = File.OpenRead("plugins.registry.json");
var errors = RemotePluginRegistryValidation.Validate(fileStream);

if (errors.Count > 0)
{
    foreach (var error in errors)
        Console.WriteLine($"Registry error: {error}");
    return;
}

// Proceed to load and use the registry
var registry = RemotePluginRegistry.Load(fileStream);
```

### Example 2: Failing fast on invalid registry with custom options

```csharp
var options = new ValidationOptions
{
    RequireSignatures = true,
    MaxPluginCount = 500
};

try
{
    RemotePluginRegistryValidation.EnsureValid(registry, options);
}
catch (RemotePluginRegistryValidationException ex)
{
    Console.WriteLine($"Registry rejected: {ex.Message}");
    foreach (var detail in ex.Errors)
        Console.WriteLine($"  - {detail}");
    throw;
}

// Registry is guaranteed valid; proceed with plugin resolution
```

## Notes

- All methods are static and stateless; they are safe to call concurrently from multiple threads without external synchronization.
- The `Validate` overloads that accept `Stream` do not close or dispose the stream. Callers remain responsible for stream lifetime.
- The `ValidationOptions` overloads allow callers to enforce additional constraints (e.g., required metadata fields, signature presence, plugin count limits). Omitting `ValidationOptions` applies default rules.
- `EnsureValid` throws an exception that aggregates all validation failures, not just the first. Callers inspecting the exception can access the complete error list.
- When validating raw JSON or streams, parsing errors are surfaced as `JsonException` before semantic validation runs. A malformed document will never produce a partial validation result.
- The return value of `Validate` is a read-only list; callers should not attempt to modify it. An empty list is the sole indicator of validity—`null` is never returned.
