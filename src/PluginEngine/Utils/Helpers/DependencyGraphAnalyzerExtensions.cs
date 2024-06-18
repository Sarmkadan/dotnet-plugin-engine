#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Constants;

namespace PluginEngine.Utils.Helpers;

/// <summary>
/// Extension methods for <see cref="DependencyGraphAnalyzer"/> providing additional utility functions
/// for dependency graph analysis and reporting.
/// </summary>
public static class DependencyGraphAnalyzerExtensions
{
    /// <summary>
    /// Filters the issues list to return only critical issues that require immediate attention.
    /// </summary>
    /// <param name="analyzer">The analyzer instance. Cannot be <see langword="null"/>.</param>
    /// <param name="report">The dependency analysis report. Cannot be <see langword="null"/>.</param>
    /// <returns>List of critical issues.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyzer"/> or <paramref name="report"/> is <see langword="null"/>.</exception>
    public static List<string> GetCriticalIssues(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)
    {
        ArgumentNullException.ThrowIfNull(analyzer);
        ArgumentNullException.ThrowIfNull(report);

        return report.Issues
            .Where(issue => issue.Contains("Circular", StringComparison.Ordinal) ||
                           issue.Contains("High number", StringComparison.Ordinal) ||
                           issue.Contains("More than 50%", StringComparison.Ordinal))
            .ToList();
    }

    /// <summary>
    /// Determines if a plugin has healthy dependency structure based on analysis.
    /// </summary>
    /// <param name="analyzer">The analyzer instance. Cannot be <see langword="null"/>.</param>
    /// <param name="report">The dependency analysis report. Cannot be <see langword="null"/>.</param>
    /// <returns>True if dependencies are healthy; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyzer"/> or <paramref name="report"/> is <see langword="null"/>.</exception>
    public static bool HasHealthyDependencies(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)
    {
        ArgumentNullException.ThrowIfNull(analyzer);
        ArgumentNullException.ThrowIfNull(report);

        var criticalIssues = analyzer.GetCriticalIssues(report);

        return !report.HasCircularDependencies &&
               report.DirectDependencies <= PluginEngineConstants.MaxDirectDependencies &&
               report.Issues.Count == 0 &&
               criticalIssues.Count == 0;
    }

    /// <summary>
    /// Gets a summary string that describes the dependency health of a plugin.
    /// </summary>
    /// <param name="analyzer">The analyzer instance. Cannot be <see langword="null"/>.</param>
    /// <param name="report">The dependency analysis report. Cannot be <see langword="null"/>.</param>
    /// <returns>Human-readable summary of dependency health.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyzer"/> or <paramref name="report"/> is <see langword="null"/>.</exception>
    public static string GetDependencyHealthSummary(this DependencyGraphAnalyzer analyzer, DependencyAnalysisReport report)
    {
        ArgumentNullException.ThrowIfNull(analyzer);
        ArgumentNullException.ThrowIfNull(report);

        var criticalIssues = analyzer.GetCriticalIssues(report);

        return criticalIssues.Count switch
        {
            > 0 => $"CRITICAL: {string.Join(", ", criticalIssues)}",
            _ when report.HasCircularDependencies => "WARNING: Circular dependencies detected",
            _ when report.DirectDependencies > PluginEngineConstants.MaxDirectDependencies => $"WARNING: High direct dependency count ({report.DirectDependencies}) - consider refactoring",
            _ when report.Issues.Count > 0 => $"INFO: {report.Issues.Count} issues found - review recommended",
            _ => "HEALTHY: All dependency checks passed"
        };
    }

    /// <summary>
    /// Generates a dependency graph visualization focused on a specific plugin.
    /// </summary>
    /// <param name="analyzer">The analyzer instance. Cannot be <see langword="null"/>.</param>
    /// <param name="rootPlugin">The root plugin to visualize. Cannot be <see langword="null"/>.</param>
    /// <param name="maxDepth">Maximum depth to traverse in the dependency tree. Must be positive.</param>
    /// <returns>Dependency graph visualization as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyzer"/> or <paramref name="rootPlugin"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxDepth"/> is negative.</exception>
    public static async Task<string> GenerateDependencyGraphAsync(this DependencyGraphAnalyzer analyzer, Plugin rootPlugin, int maxDepth = 3)
    {
        ArgumentNullException.ThrowIfNull(analyzer);
        ArgumentNullException.ThrowIfNull(rootPlugin);
        ArgumentOutOfRangeException.ThrowIfNegative(maxDepth);

        var sb = new StringBuilder();
        var visited = new HashSet<Guid>();

        sb.AppendLine($"Dependency Graph for: {rootPlugin.Name} v{rootPlugin.Version}");
        sb.AppendLine(new string('-', 60));

        await GenerateDependencyNodeAsync(sb, rootPlugin, 0, maxDepth, visited);

        return sb.ToString();
    }

    private static async Task GenerateDependencyNodeAsync(
        StringBuilder sb,
        Plugin plugin,
        int currentDepth,
        int maxDepth,
        HashSet<Guid> visited)
    {
        if (visited.Contains(plugin.Id) || currentDepth > maxDepth)
        {
            return;
        }

        visited.Add(plugin.Id);

        var indent = new string(' ', currentDepth * 2);
        sb.AppendLine($"{indent}├─ {plugin.Name} v{plugin.Version}");

        if (currentDepth < maxDepth)
        {
            const int maxDependenciesToShow = 5;
            foreach (var dep in plugin.Dependencies.Take(maxDependenciesToShow))
            {
                sb.AppendLine($"{indent} └─ → {dep.DependencyPluginId}");
            }

            if (plugin.Dependencies.Count > maxDependenciesToShow)
            {
                sb.AppendLine($"{indent} └─ ... and {plugin.Dependencies.Count - maxDependenciesToShow} more dependencies");
            }
        }

        visited.Remove(plugin.Id);
    }
}