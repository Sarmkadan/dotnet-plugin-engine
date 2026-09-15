# JsonExtensions Reference

This document lists all `*JsonExtensions` classes in the `src/PluginEngine` namespace with their `ToJson`, `FromJson`, and `TryFromJson` method signatures, along with their null/empty behavior.

## PluginJsonExtensions

**File:** `src/PluginEngine/Domain/Entities/PluginJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this Plugin value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static Plugin? FromJson(string json)` | Throws `ArgumentNullException` if `json` is null; returns null if deserialization fails |
| `TryFromJson` | `public static bool TryFromJson(string json, out Plugin? value)` | Throws `ArgumentNullException` if `json` is null; returns false if `json` is null or deserialization fails |

## PluginAssemblyJsonExtensions

**File:** `src/PluginEngine/Domain/Entities/PluginAssemblyJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this PluginAssembly value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static PluginAssembly? FromJson(string json)` | Throws `ArgumentException` if `json` is null or whitespace; returns null if deserialization fails |
| `TryFromJson` | `public static bool TryFromJson(string json, out PluginAssembly? value)` | Returns false if `json` is null or whitespace; returns false if deserialization fails |

## WebhookConfigurationJsonExtensions

**File:** `src/PluginEngine/Configuration/WebhookConfigurationJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this WebhookConfiguration value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static WebhookConfiguration? FromJson(string json)` | Throws `ArgumentException` if `json` is null or empty; throws `JsonException` if JSON is malformed; returns null if deserialization fails |
| `TryFromJson` | `public static bool TryFromJson(string json, out WebhookConfiguration? value)` | Throws `ArgumentException` if `json` is null or empty; throws `JsonException` if JSON is malformed; returns false if deserialization fails |

## RateLimitMiddlewareJsonExtensions

**File:** `src/PluginEngine/Middleware/RateLimitMiddlewareJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this RateLimitMiddleware value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static RateLimitMiddleware? FromJson(string json)` | Throws `ArgumentNullException` if `json` is null; returns null if `json` is null, empty, or whitespace, or if deserialization fails |
| `TryFromJson` | `public static bool TryFromJson(string json, out RateLimitMiddleware? value)` | Throws `ArgumentNullException` if `json` is null; returns false if deserialization fails |

## CsvPluginFormatterJsonExtensions

**File:** `src/PluginEngine/Formatters/CsvPluginFormatterJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this CsvPluginFormatter value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static CsvPluginFormatter? FromJson(string json)` | Throws `ArgumentException` if `json` is null or empty; throws `JsonException` if JSON is invalid; returns null if `json` is empty or whitespace |
| `TryFromJson` | `public static bool TryFromJson(string json, out CsvPluginFormatter? value)` | Throws `ArgumentException` if `json` is null or empty; returns false if deserialization fails |

## VersionMismatchExceptionJsonExtensions

**File:** `src/PluginEngine/Exceptions/VersionMismatchExceptionJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this VersionMismatchException value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static VersionMismatchException? FromJson(string? json)` | Throws `ArgumentNullException` if `json` is null; returns null if `json` is null, empty, whitespace, or if deserialization fails |
| `TryFromJson` | `public static bool TryFromJson(string? json, out VersionMismatchException? value)` | Throws `ArgumentNullException` if `json` is null; returns false if `json` is null, empty, whitespace, or if deserialization fails |

## PluginManagerServiceJsonExtensions

**File:** `src/PluginEngine/Services/Implementations/PluginManagerServiceJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this PluginManagerService value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static PluginManagerService FromJson(string json)` | Throws `ArgumentException` if `json` is null or empty; throws `JsonException` if JSON is invalid or cannot be deserialized; throws `JsonException` with message "Deserialization returned null, indicating invalid JSON or missing required properties." if deserialization returns null |
| `TryFromJson` | `public static bool TryFromJson(string json, out PluginManagerService? value)` | Throws `ArgumentException` if `json` is null or empty; returns false if deserialization fails |

## PluginDependencyResolverJsonExtensions

**File:** `src/PluginEngine/Services/Implementations/PluginDependencyResolverJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this PluginDependencyResolver value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static PluginDependencyResolver? FromJson(string json)` | Throws `ArgumentException` if `json` is null or empty; throws `JsonException` if JSON is malformed or cannot be deserialized; returns null if deserialization fails |
| `TryFromJson` | `public static bool TryFromJson(string json, out PluginDependencyResolver? value)` | Throws `ArgumentException` if `json` is null or empty; returns false if deserialization fails |

## TypeExtensionsJsonExtensions

**File:** `src/PluginEngine/Utils/Extensions/TypeExtensionsJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this Type type, bool indented = false)` | Throws `ArgumentNullException` if `type` is null |
| `FromJson` | `public static Type? FromJson(string json)` | Throws `ArgumentException` if `json` is null or empty; throws `JsonException` if JSON is invalid or type cannot be resolved; returns null if `json` is empty or invalid |
| `TryFromJson` | `public static bool TryFromJson(string json, out Type? type)` | Throws `ArgumentException` if `json` is null or empty; returns false if deserialization fails or type cannot be resolved |

## FileSystemHelperJsonExtensions

**File:** `src/PluginEngine/Utils/Helpers/FileSystemHelperJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this FileSystemHelper value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static FileSystemHelper? FromJson(string json)` | Throws `ArgumentException` if `json` is null or empty; returns null if JSON is invalid or cannot be deserialized |
| `TryFromJson` | `public static bool TryFromJson(string json, out FileSystemHelper? value)` | Throws `ArgumentException` if `json` is null or empty; returns false if deserialization fails |

## PluginDiscoveryServiceJsonExtensions

**File:** `src/PluginEngine/Utils/Helpers/PluginDiscoveryServiceJsonExtensions.cs`

### Methods

| Method | Signature | Null/Empty Behavior |
|--------|-----------|---------------------|
| `ToJson` | `public static string ToJson(this PluginDiscoveryService value, bool indented = false)` | Throws `ArgumentNullException` if `value` is null |
| `FromJson` | `public static PluginDiscoveryService FromJson(string json)` | Throws `ArgumentNullException` if `json` is null; throws `ArgumentException` if `json` is empty; throws `JsonException` if JSON is invalid or cannot be deserialized; throws `JsonException` with message "Deserialization returned null, indicating invalid JSON or missing required properties." if deserialization returns null |
| `TryFromJson` | `public static bool TryFromJson(string json, out PluginDiscoveryService? value)` | Throws `ArgumentNullException` if `json` is null; throws `ArgumentException` if `json` is empty; returns false if deserialization fails |