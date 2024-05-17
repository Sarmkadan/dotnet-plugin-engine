# PluginDiscoveryService

The `PluginDiscoveryService` is a service component responsible for discovering and inspecting plugin candidates from specified file paths. It provides functionality to scan assemblies, filter candidates based on criteria, and collect metadata about the discovery process.

## API

### `PluginDiscoveryService`
Initializes a new instance of the `PluginDiscoveryService` with required discovery parameters.

### `async Task<List<PluginCandidateInfo>> DiscoverPluginsAsync()`
Asynchronously discovers all plugin candidates matching the configured criteria from the specified file path.

- **Returns**: A task that resolves to a list of `PluginCandidateInfo` objects representing discovered plugins.
- **Exceptions**: Throws `ArgumentNullException` if `FilePath` is null or empty.
- **Exceptions**: Throws `DirectoryNotFoundException` if the directory specified by `FilePath` does not exist.
- **Exceptions**: Throws `UnauthorizedAccessException` if access to the directory is denied.

### `async Task<PluginCandidateInfo?> InspectPluginAsync()`
Asynchronously inspects a single plugin candidate at the specified file path and returns detailed metadata.

- **Returns**: A task that resolves to a `PluginCandidateInfo` object if the plugin is valid and inspectable, otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `FilePath` is null or empty.
- **Exceptions**: Throws `FileNotFoundException` if the file does not exist.
- **Exceptions**: Throws `BadImageFormatException` if the file is not a valid .NET assembly.

### `List<PluginCandidateInfo> FilterPlugins(List<PluginCandidateInfo> candidates)`
Filters a list of plugin candidates based on the configured criteria such as `ValidOnly`, `MinimumVersionInfo`, and `NamePattern`.

- **Parameters**:
  - `candidates`: The list of plugin candidates to filter.
- **Returns**: A filtered list of `PluginCandidateInfo` objects.
- **Exceptions**: Throws `ArgumentNullException` if `candidates` is `null`.

### `DiscoveryStatistics GetStatistics()`
Retrieves statistics about the plugin discovery process, including counts of discovered, valid, and filtered plugins.

- **Returns**: An instance of `DiscoveryStatistics` containing discovery metrics.

### `public required string FilePath`
Gets or sets the directory path where plugin candidates are discovered.

### `public required string FileName`
Gets or sets the name of the plugin file being inspected.

### `public required string AssemblyName`
Gets or sets the name of the assembly containing the plugin.

### `public required long FileSize`
Gets or sets the size of the plugin file in bytes.

### `public required DateTime ModifiedAtUtc`
Gets or sets the UTC timestamp when the plugin file was last modified.

### `public required bool IsValid`
Gets or sets a value indicating whether the plugin candidate is valid.

### `public required DateTime DiscoveredAtUtc`
Gets or sets the UTC timestamp when the plugin candidate was discovered.

### `public string? Version`
Gets or sets the version of the plugin.

### `public string? ProductName`
Gets or sets the product name of the plugin.

### `public string? Company`
Gets or sets the company that produced the plugin.

### `public string? Description`
Gets or sets a description of the plugin.

### `public List<string> CustomAttributes`
Gets or sets a list of custom attributes associated with the plugin.

### `public bool ValidOnly`
Gets or sets a value indicating whether only valid plugins should be considered during discovery.

### `public string? MinimumVersionInfo`
Gets or sets the minimum version requirement for the plugin.

### `public string? NamePattern`
Gets or sets a pattern to match plugin names against during filtering.

## Usage

### Example 1: Discover all plugins in a directory
