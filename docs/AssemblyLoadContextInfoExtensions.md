# AssemblyLoadContextInfoExtensions

Extension methods for `AssemblyLoadContext` that provide diagnostic and monitoring capabilities for plugin engine scenarios. These methods help track memory usage, age, staleness, and assembly loading patterns in dynamically loaded contexts.

## API

### `HasMemoryExceeded(AssemblyLoadContext context, long thresholdBytes)`

Determines whether the specified `AssemblyLoadContext` has exceeded a given memory threshold.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
  - `thresholdBytes`: The memory usage threshold in bytes.
- **Returns**
  - `true` if the context's memory usage exceeds `thresholdBytes`; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `context` is `null`.

---

### `GetAgeInMinutes(AssemblyLoadContext context)`

Calculates the age of the `AssemblyLoadContext` in minutes since its creation.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
- **Returns**
  - The age in minutes as a `double`.
- **Throws**
  - `ArgumentNullException`: If `context` is `null`.

---

### `GetInactivityMinutes(AssemblyLoadContext context)`

Calculates the duration in minutes since the last assembly load or unload activity in the context.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
- **Returns**
  - The inactivity duration in minutes as a `double`.
- **Throws**
  - `ArgumentNullException`: If `context` is `null`.

---
### `IsStale(AssemblyLoadContext context, TimeSpan maxAge)`

Determines whether the `AssemblyLoadContext` is considered stale based on its age.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
  - `maxAge`: The maximum allowed age before the context is considered stale.
- **Returns**
  - `true` if the context's age exceeds `maxAge`; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `context` is `null`.

---
### `GetDetailedStatusReport(AssemblyLoadContext context)`

Generates a human-readable diagnostic report of the `AssemblyLoadContext`'s state.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
- **Returns**
  - A `string` containing a detailed status report.
- **Throws**
  - `ArgumentNullException`: If `context` is `null`.

---
### `IsHealthy(AssemblyLoadContext context)`

Determines whether the `AssemblyLoadContext` is in a healthy state based on memory usage and staleness.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
- **Returns**
  - `true` if the context is healthy; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `context` is `null`.

---
### `FindAssembliesByPattern(AssemblyLoadContext context, string pattern)`

Finds assemblies loaded in the context that match a given pattern.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
  - `pattern`: A wildcard pattern (e.g., `*.Plugin.*`) to match assembly names.
- **Returns**
  - An `IEnumerable<string>` of assembly names that match the pattern.
- **Throws**
  - `ArgumentNullException`: If `context` or `pattern` is `null`.

---
### `GetMemoryUsageString(AssemblyLoadContext context)`

Returns a formatted string representing the memory usage of the `AssemblyLoadContext`.

- **Parameters**
  - `context`: The `AssemblyLoadContext` to inspect.
- **Returns**
  - A `string` with the memory usage in a human-readable format (e.g., "12.5 MB").
- **Throws**
  - `ArgumentNullException`: If `context` is `null`.

## Usage

### Monitoring Context Health
