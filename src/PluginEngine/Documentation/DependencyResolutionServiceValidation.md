# DependencyResolutionServiceValidation

`DependencyResolutionServiceValidation` provides a set of static validation helpers for the
dependency‑resolution subsystem. The methods are implemented as extension methods and
operate on the core domain objects involved in building a dependency graph. They return a
collection of validation error messages, a boolean indicating validity, or throw an
exception when the object is not valid.

## API

### `public static IReadOnlyList<string> Validate(this DependencyResolutionService value)`

Validates the internal state of a `DependencyResolutionService` instance.

* **Parameters**
  * `value` – The `DependencyResolutionService` to validate.
* **Returns**
  * An immutable list of error messages. The list is empty when the instance is valid.
* **Throws**
  * No exceptions are thrown; validation errors are reported via the returned list.

### `public static IReadOnlyList<string> Validate(this Plugin plugin)`

Validates a `Plugin` entity, checking required fields and consistency of its metadata.

* **Parameters**
  * `plugin` – The `Plugin` instance to validate.
* **Returns**
  * A read‑only list of validation error strings; empty if the plugin is valid.
* **Throws**
  * None.

### `public static IReadOnlyList<string> Validate(this PluginDependency dependency)`

Ensures that a `PluginDependency` has a parsable version constraint and a non‑empty identifier.

* **Parameters**
  * `dependency` – The `PluginDependency` to validate.
* **Returns**
  * An `IReadOnlyList<string>` containing any validation errors.
* **Throws**
  * None.

### `public static IReadOnlyList<string> Validate(this PluginMetadata metadata)`

Checks that custom properties, required fields, and version information in `PluginMetadata`
conform to the expected format.

* **Parameters**
  * `metadata` – The `PluginMetadata` instance to validate.
* **Returns**
  * A list of error messages; empty when the metadata is valid.
* **Throws**
  * None.

### `public static IReadOnlyList<string> Validate(this DependencyNode node)`

Validates a node in the dependency graph, ensuring it references a known plugin and that
its edges are well‑formed.

* **Parameters**
  * `node` – The `DependencyNode` to validate.
* **Returns**
  * An immutable list of validation errors.
* **Throws**
  * None.

### `public static IReadOnlyList<string> Validate(this DependencyEdge edge)`

Verifies that a graph edge correctly represents a dependency relationship, including
compatible version constraints.

* **Parameters**
  * `edge` – The `DependencyEdge` to validate.
* **Returns**
  * A read‑only list of error strings; empty if the edge is valid.
* **Throws**
  * None.

### `public static IReadOnlyList<string> Validate(this DependencyGraph graph)`

Performs a comprehensive validation of the entire dependency graph, checking for cycles,
missing nodes, and inconsistent version requirements.

* **Parameters**
  * `graph` – The `DependencyGraph` to validate.
* **Returns**
  * A list of validation error messages; an empty list indicates a valid graph.
* **Throws**
  * None.

### `public static bool IsValid(this DependencyResolutionService value)`

Convenience method that returns `true` when `Validate` reports no errors.

* **Parameters**
  * `value` – The `DependencyResolutionService` to test.
* **Returns**
  * `true` if the service instance is valid; otherwise `false`.
* **Throws**
  * None.

### `public static void EnsureValid(this DependencyResolutionService value)`

Ensures that a `DependencyResolutionService` instance is valid; otherwise throws an
`InvalidOperationException` containing the aggregated validation errors.

* **Parameters**
  * `value` – The `DependencyResolutionService` to validate.
* **Returns**
  * Nothing; the method either completes silently or throws.
* **Throws**
  * `InvalidOperationException` – Thrown when validation errors are present. The exception
    message includes all error strings returned by `Validate`.

## Usage

