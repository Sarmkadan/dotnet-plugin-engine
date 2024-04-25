# RemotePluginRegistry

The `RemotePluginRegistry` class provides functionality for interacting with a remote plugin registry, enabling discovery, retrieval, and publication of plugin metadata and binaries. It serves as an abstraction layer for querying plugin information, downloading plugin packages, and managing cached data.

## API

### `public RemotePluginRegistry`
The constructor initializes a new instance of the `RemotePluginRegistry` class. Required properties must be set before invoking any methods.

---

### `public async Task<List<PluginInfo>> SearchAsync`
Searches the remote registry for plugins matching unspecified criteria (implementation-specific).

**Returns:**
- A `Task<List<PluginInfo>>` containing a list of `PluginInfo` objects representing the discovered plugins.

**Throws:**
- `HttpRequestException` if the remote request fails.
- `InvalidOperationException` if required properties are not initialized.

---

### `public async Task<PluginInfo?> GetPluginAsync`
Retrieves detailed information for a specific plugin from the remote registry.

**Returns:**
- A `Task<PluginInfo?>` containing the `PluginInfo` for the requested plugin, or `null` if the plugin is not found.

**Throws:**
- `HttpRequestException` if the remote request fails.
- `InvalidOperationException` if required properties are not initialized.

---

### `public async Task<List<PluginVersionInfo>> GetVersionsAsync`
Fetches all available versions of a specified plugin from the remote registry.

**Returns:**
- A `Task<List<PluginVersionInfo>>` containing a list of `PluginVersionInfo` objects for each available version.

**Throws:**
- `HttpRequestException` if the remote request fails.
- `InvalidOperationException` if required properties are not initialized.

---

### `public async Task<string?> DownloadPluginAsync`
Downloads the plugin binary from the remote registry and saves it to a local path.

**Returns:**
- A `Task<string?>` containing the local file path where the plugin was downloaded, or `null` if the download fails.

**Throws:**
- `HttpRequestException` if the remote request fails.
- `IOException` if the file cannot be written locally.
- `InvalidOperationException` if required properties are not initialized.

---

### `public async Task<bool> PublishPluginAsync`
Publishes a plugin to the remote registry, making it available for discovery and download.

**Returns:**
- A `Task<bool>` indicating whether the publication was successful (`true`) or failed (`false`).

**Throws:**
- `HttpRequestException` if the remote request fails.
- `InvalidOperationException` if required properties are not initialized.

---

### `public void InvalidateCache`
Clears any cached data, forcing subsequent operations to fetch fresh data from the remote registry.

---

### `public required string Version`
The version of the plugin. Required for initialization.

---

### `public required DateTime PublishedAtUtc`
The UTC timestamp when the plugin version was published. Required for initialization.

---

### `public required string DownloadUrl`
The URL from which the plugin binary can be downloaded. Required for initialization.

---

### `public bool IsStable`
Indicates whether the plugin version is a stable release (`true`) or a prerelease (`false`).

---

### `public bool IsPrerelease`
Indicates whether the plugin version is a prerelease (`true`) or a stable release (`false`).

---

### `public string? ReleaseNotes`
Optional release notes describing changes or features in the plugin version.

---

### `public required string PluginName`
The name of the plugin. Required for initialization.

---

### `public required string Description`
A description of the plugin's purpose and functionality. Required for initialization.

---

### `public required string Author`
The author or maintainer of the plugin. Required for initialization.

---

### `public string? Company`
Optional company or organization associated with the plugin.

---

### `public List<string> Tags`
A list of tags categorizing the plugin for easier discovery.

---

### `public string? LicenseType`
The license under which the plugin is distributed (e.g., MIT, GPL). Optional.

## Usage

### Example 1: Discovering and Downloading a Plugin
