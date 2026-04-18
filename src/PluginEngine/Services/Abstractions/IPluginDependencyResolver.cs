#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Advanced dependency resolver that operates on a set of plugins to compute
/// installation order, detect cross-plugin version conflicts, and produce
/// actionable resolution plans.  Complements <see cref="IDependencyResolutionService"/>
/// with higher-level, set-oriented algorithms.
/// </summary>
public interface IPluginDependencyResolver
{
    /// <summary>
    /// Computes the topologically sorted installation order for the given set of plugins
    /// so that every plugin's dependencies are installed before the plugin itself.
    /// Throws <see cref="Exceptions.DependencyResolutionException"/> if an unresolvable
    /// circular dependency is detected.
    /// </summary>
    /// <param name="plugins">The full set of plugins to order.</param>
    Task<PluginOperationResult<List<Plugin>>> GetInstallOrderAsync(
        IEnumerable<Plugin> plugins, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans the provided plugin set for version conflicts: cases where two plugins
    /// declare incompatible version constraints on the same dependency.
    /// </summary>
    /// <param name="plugins">Plugins to analyse.</param>
    Task<PluginOperationResult<List<DependencyConflict>>> FindConflictsAsync(
        IEnumerable<Plugin> plugins, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a complete, human-readable resolution plan for a single plugin:
    /// lists all transitive dependencies in install order, flags conflicts,
    /// and recommends actions.
    /// </summary>
    /// <param name="pluginId">Root plugin to resolve.</param>
    Task<PluginOperationResult<DependencyResolutionPlan>> BuildResolutionPlanAsync(
        Guid pluginId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Describes a version conflict between two plugins competing over the same dependency.
/// </summary>
public sealed class DependencyConflict
{
    /// <summary>Gets or sets the identifier of the shared dependency.</summary>
    public Guid DependencyPluginId { get; set; }

    /// <summary>Gets or sets the dependency's display name.</summary>
    public string DependencyName { get; set; } = string.Empty;

    /// <summary>Gets or sets the plugins involved in the conflict, with their version constraints.</summary>
    public List<ConflictingRequirement> ConflictingRequirements { get; set; } = [];

    /// <summary>Gets or sets a human-readable description of why the conflict exists.</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// A single requirement entry within a <see cref="DependencyConflict"/>.
/// </summary>
public sealed class ConflictingRequirement
{
    /// <summary>Gets or sets the plugin that declares this requirement.</summary>
    public Guid RequiringPluginId { get; set; }

    /// <summary>Gets or sets the requiring plugin's display name.</summary>
    public string RequiringPluginName { get; set; } = string.Empty;

    /// <summary>Gets or sets the version constraint the requiring plugin declares.</summary>
    public string VersionConstraint { get; set; } = string.Empty;
}

/// <summary>
/// A fully resolved plan describing all steps needed to install a plugin and its dependencies.
/// </summary>
public sealed class DependencyResolutionPlan
{
    /// <summary>Gets or sets the root plugin this plan was built for.</summary>
    public Guid RootPluginId { get; set; }

    /// <summary>Gets or sets the ordered list of installation steps.</summary>
    public List<ResolutionStep> Steps { get; set; } = [];

    /// <summary>Gets or sets any conflicts detected during planning.</summary>
    public List<DependencyConflict> Conflicts { get; set; } = [];

    /// <summary>Gets or sets whether the plan can be executed without manual intervention.</summary>
    public bool IsExecutable => Conflicts.Count == 0;

    /// <summary>Gets or sets when this plan was generated.</summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A single actionable step within a <see cref="DependencyResolutionPlan"/>.
/// </summary>
public sealed class ResolutionStep
{
    /// <summary>Gets or sets the execution order (1-based).</summary>
    public int Order { get; set; }

    /// <summary>Gets or sets the plugin this step applies to.</summary>
    public Guid PluginId { get; set; }

    /// <summary>Gets or sets the plugin display name.</summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>Gets or sets the version to install or verify.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the recommended action for this step.</summary>
    public ResolutionAction Action { get; set; }

    /// <summary>Gets or sets whether this step is for an optional dependency.</summary>
    public bool IsOptional { get; set; }
}

/// <summary>Recommended action for a resolution step.</summary>
public enum ResolutionAction
{
    /// <summary>Install a new plugin.</summary>
    Install,

    /// <summary>The dependency is already installed and satisfies the constraint.</summary>
    AlreadySatisfied,

    /// <summary>An incompatible version is installed and must be upgraded.</summary>
    Upgrade,

    /// <summary>A conflict exists; manual resolution required.</summary>
    ManualResolutionRequired
}
