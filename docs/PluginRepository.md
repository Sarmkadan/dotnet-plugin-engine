# PluginRepository

The `PluginRepository` interface provides asynchronous operations for managing plugins within the dotnet-plugin-engine, including CRUD operations, dependency management, capability tracking, and search functionality.

## API

### `Task<Plugin> AddAsync(Plugin plugin)`

Adds a new plugin to the repository. The plugin must be in a valid state (e.g., have a unique name and valid metadata).

- **Parameters**: `plugin` – The plugin instance to add.
- **Return value**: A `Task<Plugin>` resolving to the added plugin, typically with any auto-generated identifiers populated.
- **Exceptions**: Throws if the plugin name already exists or if required fields are missing.

---

### `Task<bool> UpdateAsync(Plugin plugin)`

Updates an existing plugin in the repository. The plugin must already exist and be in a valid state.

- **Parameters**: `plugin` – The plugin instance with updated values.
- **Return value**: A `Task<bool>` indicating whether the update was successful (`true` if updated, `false` if the plugin did not exist).
- **Exceptions**: Throws if the plugin does not exist or if required fields are invalid.

---

### `Task<bool> DeleteAsync(string id)`

Deletes a plugin from the repository by its unique identifier.

- **Parameters**: `id` – The unique identifier of the plugin to delete.
- **Return value**: A `Task<bool>` indicating whether the deletion was successful (`true` if deleted, `false` if the plugin did not exist).
- **Exceptions**: Throws if the identifier is invalid or if the plugin is referenced by other plugins and deletion is not permitted.

---

### `Task<Plugin?> GetByIdAsync(string id)`

Retrieves a plugin by its unique identifier.

- **Parameters**: `id` – The unique identifier of the plugin to retrieve.
- **Return value**: A `Task<Plugin?>` resolving to the plugin if found, or `null` otherwise.
- **Exceptions**: Throws if the identifier is invalid.

---

### `Task<Plugin?> GetByNameAsync(string name)`

Retrieves a plugin by its name.

- **Parameters**: `name` – The name of the plugin to retrieve.
- **Return value**: A `Task<Plugin?>` resolving to the plugin if found, or `null` otherwise.
- **Exceptions**: Throws if the name is invalid or empty.

---
### `Task<IEnumerable<Plugin>> GetAllAsync()`

Retrieves all plugins in the repository.

- **Return value**: A `Task<IEnumerable<Plugin>>` resolving to a collection of all plugins.
- **Exceptions**: Throws if the repository is in an inconsistent state.

---
### `Task<IEnumerable<Plugin>> GetByStatusAsync(PluginStatus status)`

Retrieves all plugins matching the specified status.

- **Parameters**: `status` – The plugin status to filter by (e.g., `Active`, `Disabled`, `Error`).
- **Return value**: A `Task<IEnumerable<Plugin>>` resolving to a collection of plugins with the matching status.
- **Exceptions**: Throws if the status is invalid.

---
### `Task<bool> ExistsAsync(string id)`

Checks whether a plugin with the specified identifier exists.

- **Parameters**: `id` – The unique identifier to check.
- **Return value**: A `Task<bool>` resolving to `true` if the plugin exists, `false` otherwise.
- **Exceptions**: Throws if the identifier is invalid.

---
### `Task<int> CountAsync()`

Gets the total number of plugins in the repository.

- **Return value**: A `Task<int>` resolving to the count of plugins.
- **Exceptions**: Throws if the repository is in an inconsistent state.

---
### `Task<IEnumerable<Plugin>> SearchAsync(PluginSearchCriteria criteria)`

Searches for plugins matching the specified criteria.

- **Parameters**: `criteria` – The search criteria (e.g., name substring, status, tags).
- **Return value**: A `Task<IEnumerable<Plugin>>` resolving to a collection of matching plugins.
- **Exceptions**: Throws if the criteria are invalid.

---
### `Task<bool> AddDependencyAsync(string pluginId, string dependencyId)`

Adds a dependency from one plugin to another.

- **Parameters**:
  - `pluginId` – The identifier of the plugin that depends on another.
  - `dependencyId` – The identifier of the plugin being depended on.
- **Return value**: A `Task<bool>` indicating whether the dependency was added (`true` if added, `false` if the dependency already exists).
- **Exceptions**: Throws if either plugin does not exist or if a circular dependency would be created.

---
### `Task<bool> RemoveDependencyAsync(string pluginId, string dependencyId)`

Removes a dependency between two plugins.

- **Parameters**:
  - `pluginId` – The identifier of the plugin that depends on another.
  - `dependencyId` – The identifier of the plugin being depended on.
- **Return value**: A `Task<bool>` indicating whether the dependency was removed (`true` if removed, `false` if the dependency did not exist).
- **Exceptions**: Throws if either plugin does not exist.

---
### `Task<IEnumerable<PluginDependency>> GetDependenciesAsync(string pluginId)`

Retrieves all dependencies for a specified plugin.

- **Parameters**: `pluginId` – The identifier of the plugin.
- **Return value**: A `Task<IEnumerable<PluginDependency>>` resolving to a collection of dependencies.
- **Exceptions**: Throws if the plugin does not exist.

---
### `Task<bool> AddCapabilityAsync(string pluginId, PluginCapability capability)`

Adds a capability to a plugin.

- **Parameters**:
  - `pluginId` – The identifier of the plugin.
  - `capability` – The capability to add.
- **Return value**: A `Task<bool>` indicating whether the capability was added (`true` if added, `false` if the capability already exists).
- **Exceptions**: Throws if the plugin does not exist or if the capability is invalid.

---
### `Task<IEnumerable<PluginCapability>> GetCapabilitiesAsync(string pluginId)`

Retrieves all capabilities for a specified plugin.

- **Parameters**: `pluginId` – The identifier of the plugin.
- **Return value**: A `Task<IEnumerable<PluginCapability>>` resolving to a collection of capabilities.
- **Exceptions**: Throws if the plugin does not exist.

## Usage

### Adding a Plugin and Its Dependencies
