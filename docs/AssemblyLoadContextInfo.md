# AssemblyLoadContextInfo

`AssemblyLoadContextInfo` provides runtime metadata and management capabilities for an `AssemblyLoadContext` in the plugin engine, including identity, lifecycle tracking, memory usage, and assembly loading state.

## API

### `public Guid Id`
Unique identifier for the `AssemblyLoadContext`. This value is assigned at creation and never changes.

### `public string ContextId`
Human-readable identifier for the context, typically derived from the plugin or component name. May be null or empty if not explicitly set.

### `public Guid PluginId`
Identifier of the plugin that owns this context. Used to correlate context state with plugin lifecycle events.

### `public string Name`
Display name of the context, intended for logging and diagnostics. May be null or empty.

### `public DateTime CreatedAt`
Timestamp when the context was initialized. Set once at construction and immutable thereafter.

### `public DateTime LastActivityAt`
Timestamp of the most recent assembly load, unload, or activity update. Updated via `UpdateActivity`.

### `public bool IsActive`
Indicates whether the context is currently active and accepting new assemblies. Returns `false` if the context has been unloaded or marked inactive.

### `public long MemoryUsageBytes`
Estimated memory usage of assemblies loaded into this context, in bytes. May be approximate and updated periodically.

### `public int LoadedTypeCount`
Number of types loaded into this context. Updated automatically as assemblies are loaded and unloaded.

### `public void AddLoadedAssembly(Assembly assembly)`
Adds an assembly to the context’s loaded assembly set.
**Parameters:** `assembly` – The assembly to track.
**Throws:** `ArgumentNullException` if `assembly` is null.

### `public bool RemoveLoadedAssembly(Assembly assembly)`
Removes an assembly from the context’s loaded set.
**Parameters:** `assembly` – The assembly to remove.
**Returns:** `true` if the assembly was present and removed; `false` otherwise.
**Throws:** `ArgumentNullException` if `assembly` is null.

### `public void ClearLoadedAssemblies()`
Removes all assemblies from the context’s loaded set. Does not affect context activity state.

### `public bool IsAssemblyLoaded(Assembly assembly)`
Checks whether an assembly is currently loaded in this context.
**Parameters:** `assembly` – The assembly to check.
**Returns:** `true` if the assembly is loaded; `false` otherwise.
**Throws:** `ArgumentNullException` if `assembly` is null.

### `public void UpdateActivity()`
Updates `LastActivityAt` to the current UTC time. Call this method after significant operations to keep activity state current.

### `public int GetAssemblyCount()`
Returns the number of assemblies currently loaded in this context.

### `public bool IsValid()`
Checks whether the context is still valid for use. A context may become invalid after unload or error conditions.
**Returns:** `true` if the context is valid; `false` otherwise.

### `public string GetStatusSummary()`
Generates a concise diagnostic summary of the context, including `ContextId`, `PluginId`, `IsActive`, `LoadedTypeCount`, `MemoryUsageBytes`, and `LastActivityAt`.

## Usage
