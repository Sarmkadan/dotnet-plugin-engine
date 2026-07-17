#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Provides validation helpers for <see cref="DependencyResolutionService"/> instances.
/// </summary>
public static class DependencyResolutionServiceValidation
{
    /// <summary>
    /// Validates a <see cref="DependencyResolutionService"/> instance.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DependencyResolutionService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // DependencyResolutionService is a stateless service class with dependencies injected via constructor.
        // The constructor already validates its dependencies, so there's no additional state to validate here.
        // This method exists for consistency with the validation pattern used throughout the codebase.

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="DependencyResolutionService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><c>true</c> if the service is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DependencyResolutionService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="DependencyResolutionService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the service is invalid, containing validation error messages.</exception>
    public static void EnsureValid(this DependencyResolutionService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DependencyResolutionService is invalid. Errors: {string.Join(", ", errors)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="Plugin"/> instance for dependency resolution purposes.
    /// </summary>
    /// <param name="plugin">The plugin to validate.</param>
    /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plugin"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(plugin.Name))
        {
            errors.Add("Plugin name cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(plugin.Version))
        {
            errors.Add("Plugin version cannot be null or whitespace.");
        }
        else if (!Version.TryParse(plugin.Version, out _))
        {
            errors.Add("Plugin version must be a valid semantic version string.");
        }

        if (string.IsNullOrWhiteSpace(plugin.AssemblyPath))
        {
            errors.Add("Plugin assembly path cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(plugin.Author))
        {
            errors.Add("Plugin author cannot be null or whitespace.");
        }

        // Validate GUID
        if (plugin.Id == Guid.Empty)
        {
            errors.Add("Plugin ID cannot be empty (Guid.Empty).");
        }

        // Validate dates (should not be default)
        if (plugin.CreatedAt == default)
        {
            errors.Add("Plugin creation timestamp cannot be default (DateTime.MinValue).");
        }

        if (plugin.ModifiedAt == default)
        {
            errors.Add("Plugin modification timestamp cannot be default (DateTime.MinValue).");
        }

        // Validate dependencies
        if (plugin.Dependencies is null)
        {
            errors.Add("Plugin dependencies collection cannot be null.");
        }
        else
        {
            foreach (var dependency in plugin.Dependencies)
            {
                if (dependency is null)
                {
                    errors.Add("Plugin dependency cannot be null.");
                    continue;
                }

                var dependencyErrors = dependency.Validate();
                if (dependencyErrors.Count > 0)
                {
                    errors.AddRange(dependencyErrors.Select(e => $"Dependency error: {e}"));
                }
            }
        }

        // Validate metadata if present
        if (plugin.Metadata is not null)
        {
            var metadataErrors = plugin.Metadata.Validate();
            if (metadataErrors.Count > 0)
            {
                errors.AddRange(metadataErrors.Select(e => $"Metadata error: {e}"));
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="PluginDependency"/> instance.
    /// </summary>
    /// <param name="dependency">The dependency to validate.</param>
    /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dependency"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PluginDependency dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        var errors = new List<string>();

        // Validate GUIDs
        if (dependency.PluginId == Guid.Empty)
        {
            errors.Add("Plugin ID (owner) cannot be empty (Guid.Empty).");
        }

        if (dependency.DependencyPluginId == Guid.Empty)
        {
            errors.Add("Dependency plugin ID cannot be empty (Guid.Empty).");
        }

        // Validate version strings
        if (string.IsNullOrWhiteSpace(dependency.MinimumVersion))
        {
            errors.Add("Minimum version cannot be null or whitespace.");
        }
        else if (!Version.TryParse(dependency.MinimumVersion, out _))
        {
            errors.Add("Minimum version must be a valid semantic version string.");
        }

        if (!string.IsNullOrWhiteSpace(dependency.MaximumVersion) &&
            !Version.TryParse(dependency.MaximumVersion, out _))
        {
            errors.Add("Maximum version must be a valid semantic version string or empty.");
        }

        // Validate dates
        if (dependency.CreatedAt == default)
        {
            errors.Add("Dependency creation timestamp cannot be default (DateTime.MinValue).");
        }

        // Validate type
        if (!Enum.IsDefined(typeof(DependencyType), dependency.Type))
        {
            errors.Add("Dependency type must be a valid enum value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="PluginMetadata"/> instance.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PluginMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var errors = new List<string>();

        // Validate required properties
        if (metadata.PluginId == Guid.Empty)
        {
            errors.Add("Plugin ID cannot be empty (Guid.Empty).");
        }

        if (string.IsNullOrWhiteSpace(metadata.PluginName))
        {
            errors.Add("Plugin name cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(metadata.PluginVersion))
        {
            errors.Add("Plugin version cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(metadata.AssemblyName))
        {
            errors.Add("Assembly name cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(metadata.TargetFramework))
        {
            errors.Add("Target framework cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(metadata.AssemblyVersion))
        {
            errors.Add("Assembly version cannot be null or whitespace.");
        }
        else if (!Version.TryParse(metadata.AssemblyVersion, out _))
        {
            errors.Add("Assembly version must be a valid semantic version string.");
        }

        if (string.IsNullOrWhiteSpace(metadata.Author))
        {
            errors.Add("Author cannot be null or whitespace.");
        }

        // Validate dates
        if (metadata.CreatedAt == default)
        {
            errors.Add("Metadata creation timestamp cannot be default (DateTime.MinValue).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="DependencyNode"/> instance.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DependencyNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var errors = new List<string>();

        if (node.PluginId == Guid.Empty)
        {
            errors.Add("Plugin ID cannot be empty (Guid.Empty).");
        }

        if (string.IsNullOrWhiteSpace(node.PluginName))
        {
            errors.Add("Plugin name cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(node.Version))
        {
            errors.Add("Plugin version cannot be null or whitespace.");
        }
        else if (!Version.TryParse(node.Version, out _))
        {
            errors.Add("Plugin version must be a valid semantic version string.");
        }

        if (node.Level < 0)
        {
            errors.Add("Node level cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="DependencyEdge"/> instance.
    /// </summary>
    /// <param name="edge">The edge to validate.</param>
    /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="edge"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DependencyEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        var errors = new List<string>();

        if (edge.FromPluginId == Guid.Empty)
        {
            errors.Add("Source plugin ID cannot be empty (Guid.Empty).");
        }

        if (edge.ToPluginId == Guid.Empty)
        {
            errors.Add("Target plugin ID cannot be empty (Guid.Empty).");
        }

        if (string.IsNullOrWhiteSpace(edge.VersionConstraint))
        {
            errors.Add("Version constraint cannot be null or whitespace.");
        }
        else
        {
            // Validate version constraint format more thoroughly
            var constraint = edge.VersionConstraint.Trim();
            if (!constraint.StartsWith(">=", StringComparison.Ordinal) &&
                !constraint.Contains("&&", StringComparison.Ordinal))
            {
                errors.Add("Version constraint should start with '>= ' or contain '&&' for range constraints.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="DependencyGraph"/> instance.
    /// </summary>
    /// <param name="graph">The graph to validate.</param>
    /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DependencyGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var errors = new List<string>();

        if (graph.RootPluginId == Guid.Empty)
        {
            errors.Add("Root plugin ID cannot be empty (Guid.Empty).");
        }

        if (graph.Nodes is null)
        {
            errors.Add("Nodes collection cannot be null.");
        }
        else
        {
            foreach (var node in graph.Nodes)
            {
                if (node is null)
                {
                    errors.Add("Node in collection cannot be null.");
                    continue;
                }

                var nodeErrors = node.Validate();
                if (nodeErrors.Count > 0)
                {
                    errors.AddRange(nodeErrors.Select(e => $"Node error: {e}"));
                }
            }
        }

        if (graph.Edges is null)
        {
            errors.Add("Edges collection cannot be null.");
        }
        else
        {
            foreach (var edge in graph.Edges)
            {
                if (edge is null)
                {
                    errors.Add("Edge in collection cannot be null.");
                    continue;
                }

                var edgeErrors = edge.Validate();
                if (edgeErrors.Count > 0)
                {
                    errors.AddRange(edgeErrors.Select(e => $"Edge error: {e}"));
                }
            }
        }

        return errors.AsReadOnly();
    }
}