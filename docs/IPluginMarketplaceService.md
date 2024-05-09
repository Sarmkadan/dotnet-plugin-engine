# IPluginMarketplaceService

The `IPluginMarketplaceService` interface defines the contract for interacting with a plugin marketplace, enabling discovery, compatibility checks, and installation of plugins in a .NET application.

## API

### `PluginMarketplaceService`

The default implementation of `IPluginMarketplaceService` provided by the library. This class handles communication with the plugin marketplace, including search, retrieval, compatibility checks, and installation operations.

### `async Task<PluginOperationResult<List<MarketplaceEntry>>> SearchAsync(string query, CancellationToken cancellationToken = default)`

Searches the plugin marketplace for entries matching the given query.

- **Parameters**:
  - `query`: The search term to filter marketplace entries.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Return value**: A `PluginOperationResult<List<MarketplaceEntry>>` containing the search results or an error.
- **Exceptions**: Throws `ArgumentNullException` if `query` is null.

### `async Task<PluginOperationResult<MarketplaceEntry>> GetEntryAsync(string pluginId, CancellationToken cancellationToken = default)`

Retrieves a specific plugin entry from the marketplace by its unique identifier.

- **Parameters**:
  - `pluginId`: The unique identifier of the plugin to retrieve.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Return value**: A `PluginOperationResult<MarketplaceEntry>>` containing the plugin entry or an error.
- **Exceptions**: Throws `ArgumentNullException` or `ArgumentException` if `pluginId` is null or empty.

### `async Task<PluginOperationResult<VersionCompatibilityMatrix>> GetCompatibilityMatrixAsync(string pluginId, CancellationToken cancellationToken = default)`

Retrieves the compatibility matrix for a specific plugin, detailing supported host versions and dependencies.

- **Parameters**:
  - `pluginId`: The unique identifier of the plugin.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Return value**: A `PluginOperationResult<VersionCompatibilityMatrix>>` containing the compatibility matrix or an error.
- **Exceptions**: Throws `ArgumentNullException` or `ArgumentException` if `pluginId` is null or empty.

### `async Task<PluginOperationResult<CompatibilityStatus>> CheckCompatibilityAsync(string pluginId, string hostVersion, CancellationToken cancellationToken = default)`

Checks whether a plugin is compatible with a given host version.

- **Parameters**:
  - `pluginId`: The unique identifier of the plugin.
  - `hostVersion`: The version of the host application to check compatibility against.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Return value**: A `PluginOperationResult<CompatibilityStatus>>` indicating compatibility or an error.
- **Exceptions**: Throws `ArgumentNullException` or `ArgumentException` if `pluginId` or `hostVersion` is null or empty.

### `async Task<PluginOperationResult> InstallAsync(string pluginId, string version, CancellationToken cancellationToken = default)`

Installs a plugin with the specified version into the application.

- **Parameters**:
  - `pluginId`: The unique identifier of the plugin.
  - `version`: The version of the plugin to install.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Return value**: A `PluginOperationResult` indicating success or failure.
- **Exceptions**: Throws `ArgumentNullException` or `ArgumentException` if `pluginId` or `version` is null or empty.

## Usage

### Searching for plugins
