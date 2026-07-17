# PluginExtensions

`PluginExtensions` is a static utility class that provides helper methods for querying and manipulating plugin metadata and lifecycle states in the `dotnet-plugin-engine` system. These extensions simplify common operations such as checking plugin health, retrieving formatted dates, and managing plugin state transitions.

## API

### `public static bool IsActive(Plugin plugin)`

Determines whether the given plugin is currently active. A plugin is considered active if it is not in a failed or transitioning state and has no unresolved dependencies.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - `true` if the plugin is active; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---

### `public static bool IsFailed(Plugin plugin)`

Checks whether the plugin has entered a failed state, typically due to an error during execution or initialization.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - `true` if the plugin is in a failed state; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---

### `public static string GetDisplayName(Plugin plugin)`

Returns a human-readable display name for the plugin, suitable for UI or logging purposes. If the plugin lacks a custom display name, returns the plugin identifier.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - A non-null string representing the plugin’s display name.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---

### `public static string GetFormattedCreationDate(Plugin plugin)`

Formats the plugin’s creation timestamp into a human-readable string using the current culture.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - A non-null string representing the formatted creation date.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.
  - `InvalidOperationException` if the plugin has no creation timestamp.

---

### `public static string GetFormattedModificationDate(Plugin plugin)`

Formats the plugin’s last modification timestamp into a human-readable string using the current culture.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - A non-null string representing the formatted modification date.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.
  - `InvalidOperationException` if the plugin has no modification timestamp.

---
### `public static bool HasDependencies(Plugin plugin)`

Determines whether the plugin has any declared dependencies.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - `true` if the plugin has one or more dependencies; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---
### `public static bool HasCapabilities(Plugin plugin)`

Checks whether the plugin declares any capabilities, indicating it supports specific features or operations.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - `true` if the plugin has one or more capabilities; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---
### `public static string GetMetadataSummary(Plugin plugin)`

Generates a concise summary of the plugin’s metadata, including name, version, and status.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - A non-null string summarizing the plugin’s metadata.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---
### `public static void Touch(Plugin plugin)`

Updates the plugin’s last access timestamp to the current time, marking it as recently used.

- **Parameters**:
  - `plugin` – The `Plugin` instance to update.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---
### `public static bool IsTransitioning(Plugin plugin)`

Indicates whether the plugin is currently undergoing a state transition, such as activation or deactivation.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - `true` if the plugin is transitioning; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.

---
### `public static int GetAgeInDays(Plugin plugin)`

Calculates the number of days elapsed since the plugin was created.

- **Parameters**:
  - `plugin` – The `Plugin` instance to evaluate.
- **Returns**:
  - The age in days as a non-negative integer.
- **Throws**:
  - `ArgumentNullException` if `plugin` is `null`.
  - `InvalidOperationException` if the plugin has no creation timestamp.

## Usage
