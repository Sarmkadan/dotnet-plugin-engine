# DependencyGraph

This document describes the data structures used for dependency resolution in the Plugin Engine, including dependency graphs, nodes, edges, conflict resolution, and resolution plans.

## API

### `public sealed class DependencyGraph`
Represents a dependency graph for a plugin.

- **RootPluginId**: Gets or sets the unique identifier of the root plugin.
- **Nodes**: Gets or sets the list of nodes in the dependency graph.
- **Edges**: Gets or sets the list of edges in the dependency graph.

---

### `public sealed class DependencyNode`
Represents a node in the dependency graph.

- **PluginId**: Gets or sets the unique identifier of the plugin.
- **PluginName**: Gets or sets the name of the plugin.
- **Version**: Gets or sets the version of the plugin.
- **Level**: Gets or sets the level of the node in the dependency graph.

---

### `public sealed class DependencyEdge`
Represents an edge in the dependency graph.

- **FromPluginId**: Gets or sets the unique identifier of the source plugin.
- **ToPluginId**: Gets or sets the unique identifier of the target plugin.
- **VersionConstraint**: Gets or sets the version constraint for the dependency.
- **IsOptional**: Gets or sets a value indicating whether the dependency is optional.

---

### `public sealed class DependencyConflict`
Describes a version conflict between two plugins competing over the same dependency.

- **DependencyPluginId**: Gets or sets the identifier of the shared dependency.
- **DependencyName**: Gets or sets the dependency's display name.
- **ConflictingRequirements**: Gets or sets the plugins involved in the conflict, with their version constraints.
- **Description**: Gets or sets a human-readable description of why the conflict exists.

---

### `public sealed class ConflictingRequirement`
A single requirement entry within a <see cref="DependencyConflict"/>.

- **RequiringPluginId**: Gets or sets the plugin that declares this requirement.
- **RequiringPluginName**: Gets or sets the requiring plugin's display name.
- **VersionConstraint**: Gets or sets the version constraint the requiring plugin declares.

---

### `public sealed class DependencyResolutionPlan`
A fully resolved plan describing all steps needed to install a plugin and its dependencies.

- **RootPluginId**: Gets or sets the root plugin this plan was built for.
- **Steps**: Gets or sets the ordered list of installation steps.
- **Conflicts**: Gets or sets any conflicts detected during planning.
- **IsExecutable**: Gets or sets whether the plan can be executed without manual intervention.
- **GeneratedAtUtc**: Gets or sets when this plan was generated.

---

### `public sealed class ResolutionStep`
A single actionable step within a <see cref="DependencyResolutionPlan"/>.

- **Order**: Gets or sets the execution order (1-based).
- **PluginId**: Gets or sets the plugin this step applies to.
- **PluginName**: Gets or sets the plugin display name.
- **Version**: Gets or sets the version to install or verify.
- **Action**: Gets or sets the recommended action for this step.
- **IsOptional**: Gets or sets whether this step is for an optional dependency.
- **ToString()**: Returns a string representation of the resolution step in the format: "{Order}. {Action} {PluginName} v{Version}{(IsOptional ? " (optional)" : string.Empty)}"

---

### `public enum ResolutionAction`
Recommended action for a resolution step.

- **Install**: Install a new plugin.
- **AlreadySatisfied**: The dependency is already installed and satisfies the constraint.
- **Upgrade**: An incompatible version is installed and must be upgraded.
- **ManualResolutionRequired**: A conflict exists; manual resolution required.