# PluginAssembly

Represents metadata and state for a plugin assembly within the dotnet-plugin-engine, including identity, file information, load status, and timestamps.

## API

### `Id`
A unique identifier for this assembly instance. This value is generated when the assembly metadata is created and remains constant for the lifetime of the object.

### `PluginId`
The identifier of the plugin to which this assembly belongs. Used to correlate assemblies with their parent plugin definitions.

### `AssemblyName`
The simple name of the assembly (e.g., "MyPlugin.Core"). This is derived from the assembly file and does not include version or culture information.

### `AssemblyVersion`
The version of the assembly as specified in its metadata (e.g., "1.0.0.0"). Used to determine compatibility and load order.

### `FilePath`
The absolute file system path from which the assembly was loaded or will be loaded. This path is used for file operations such as reading, hashing, and validation.

### `FileSizeBytes`
The size of the assembly file in bytes, measured at the time of metadata creation or last update. Used for validation and diagnostics.

### `FileHash`
A cryptographic hash (typically SHA-256) of the assembly file contents. Used to detect file changes and ensure integrity prior to loading.

### `PublicKeyToken`
The public key token of the assembly's strong name, if signed. Used to verify identity and origin of the assembly.

### `IsMainAssembly`
Indicates whether this assembly is the primary entry point for the plugin. Only one assembly per plugin can be marked as main.

### `LoadContextId`
The identifier of the assembly load context in which this assembly was or will be loaded. Used to manage isolation and unloading boundaries.

### `LastModifiedAt`
The UTC timestamp when the assembly file was last modified on disk. Used to detect stale or outdated assemblies.

### `LoadedAt`
The UTC timestamp when the assembly was successfully loaded, or `null` if not yet loaded or failed to load. Used for diagnostics and lifecycle tracking.

### `Status`
The current load status of the assembly, represented by the `AssemblyLoadStatus` enum. Indicates whether the assembly is pending, loaded, failed, or otherwise.

### `ErrorMessage`
A human-readable message describing the reason for a failed load, if applicable. Populated only when `Status` indicates a failure.

### `IsValid`
A computed property indicating whether the assembly metadata is considered valid. This is typically `true` when required fields are present and file integrity checks pass.

### `GetQualifiedName()`
Returns a string combining the `AssemblyName`, `AssemblyVersion`, and `PublicKeyToken` (if present) into a standard assembly identity format (e.g., "MyPlugin.Core, Version=1.0.0.0, PublicKeyToken=abc123").

- **Returns**: A string representing the qualified name of the assembly.
- **Throws**: `InvalidOperationException` if required identity fields are missing.

### `UpdateFileInfo()`
Refreshes file-based metadata such as `FileSizeBytes`, `FileHash`, `LastModifiedAt`, and `PublicKeyToken` by re-reading the file from disk. This should be called before attempting to load the assembly to ensure current state.

- **Throws**: `FileNotFoundException` if the file no longer exists.
- **Throws**: `IOException` if file access fails.
- **Throws**: `UnauthorizedAccessException` if file access is denied.

### `MarkAsLoaded()`
Updates the internal state to reflect that the assembly has been successfully loaded, setting `Status` to `Loaded`, populating `LoadedAt` with the current UTC time, and clearing any prior `ErrorMessage`.

### `MarkAsFailedLoad(string errorMessage)`
Updates the internal state to reflect that the assembly failed to load, setting `Status` to `Failed`, populating `ErrorMessage` with the provided message, and setting `LoadedAt` to `null`.

- **Parameters**:
  - `errorMessage`: A non-null, non-empty string describing the failure reason.
- **Throws**: `ArgumentNullException` if `errorMessage` is `null`.
- **Throws**: `ArgumentException` if `errorMessage` is empty.

## Usage

### Validating and Loading an Assembly
