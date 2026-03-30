#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Helpers;

/// <summary>
/// Analyzes plugin dependency graphs for visualization and debugging.
/// Provides tools for understanding complex dependency relationships.
/// </summary>
public sealed class DependencyGraphAnalyzer
{
    private readonly IDependencyResolutionService _dependencyResolver;
    private readonly ILogger<DependencyGraphAnalyzer> _logger;

    public DependencyGraphAnalyzer(
        IDependencyResolutionService dependencyResolver,
        ILogger<DependencyGraphAnalyzer> logger)
    {
        _dependencyResolver = dependencyResolver;
        _logger = logger;
    }

    /// <summary>
    /// Generates a visual text representation of dependency graph.
    /// </summary>
    public async Task<string> GenerateGraphVisualizationAsync(Plugin rootPlugin)
    {
        try
        {
            var graph = await _dependencyResolver.GetDependencyGraphAsync(rootPlugin.Id);
            var sb = new StringBuilder();

            sb.AppendLine($"Dependency Graph for: {rootPlugin.Name}");
            sb.AppendLine(new string('=', 50));

            await VisualizeNodeAsync(sb, rootPlugin, 0, new HashSet<Guid>());

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dependency graph visualization");
            return string.Empty;
        }
    }

    private async Task VisualizeNodeAsync(
        StringBuilder sb,
        Plugin plugin,
        int depth,
        HashSet<Guid> visited)
    {
        if (visited.Contains(plugin.Id))
        {
            sb.AppendLine($"{GetIndent(depth)}├─ {plugin.Name} (circular reference)");
            return;
        }

        visited.Add(plugin.Id);

        sb.AppendLine($"{GetIndent(depth)}├─ {plugin.Name} v{plugin.Version}");

        foreach (var dep in plugin.Dependencies)
        {
            if (depth < 5) // Limit depth to prevent infinite recursion
            {
                sb.AppendLine($"{GetIndent(depth + 1)}└─ Requires: {dep.DependencyPluginId}");
            }
        }

        visited.Remove(plugin.Id);
    }

    /// <summary>
    /// Analyzes dependency complexity and identifies issues.
    /// </summary>
    public async Task<DependencyAnalysisReport> AnalyzeAsync(Plugin plugin)
    {
        try
        {
            var dependencies = await _dependencyResolver.ResolveDependenciesAsync(plugin);
            var hasCircular = await _dependencyResolver.HasCircularDependenciesAsync(plugin);

            var report = new DependencyAnalysisReport
            {
                PluginName = plugin.Name,
                DirectDependencies = plugin.Dependencies.Count,
                TotalDependencies = dependencies.Count(),
                HasCircularDependencies = hasCircular,
                AnalyzedAtUtc = DateTime.UtcNow
            };

            // Calculate complexity score
            report.ComplexityScore = CalculateComplexityScore(plugin, dependencies.Count());

            // Identify potential issues
            if (plugin.Dependencies.Count > 20)
            {
                report.Issues.Add("High number of direct dependencies (>20)");
            }

            if (hasCircular)
            {
                report.Issues.Add("Circular dependencies detected");
            }

            var unmandatoryDeps = plugin.Dependencies.Count(d => d.IsOptional);
            if (unmandatoryDeps > plugin.Dependencies.Count * 0.5)
            {
                report.Issues.Add("More than 50% of dependencies are optional");
            }

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing dependency graph");
            return new DependencyAnalysisReport { PluginName = plugin.Name };
        }
    }

    /// <summary>
    /// Finds all plugins that depend on a given plugin.
    /// </summary>
    public async Task<List<Guid>> FindDependentsAsync(IEnumerable<Plugin> allPlugins, Guid targetPluginId)
    {
        var dependents = new List<Guid>();

        foreach (var plugin in allPlugins)
        {
            if (plugin.Dependencies.Any(d => d.DependencyPluginId == targetPluginId))
            {
                dependents.Add(plugin.Id);
            }
        }

        return await Task.FromResult(dependents);
    }

    /// <summary>
    /// Calculates a complexity score for a plugin based on dependencies.
    /// </summary>
    private static int CalculateComplexityScore(Plugin plugin, int totalDependencies)
    {
        var score = 0;

        // Direct dependency score
        score += plugin.Dependencies.Count * 10;

        // Total dependency score
        score += Math.Min(totalDependencies * 2, 100);

        // Circular dependency penalty
        // Would need additional logic here

        // Optional dependency factor
        var optionalCount = plugin.Dependencies.Count(d => d.IsOptional);
        score += optionalCount * 5;

        return Math.Min(score, 100); // Cap at 100
    }

    private static string GetIndent(int depth) => new(' ', depth * 2);
}

/// <summary>
/// Report from dependency graph analysis.
/// </summary>
public sealed class DependencyAnalysisReport
{
    public required string PluginName { get; set; }
    public int DirectDependencies { get; set; }
    public int TotalDependencies { get; set; }
    public bool HasCircularDependencies { get; set; }
    public int ComplexityScore { get; set; }
    public DateTime AnalyzedAtUtc { get; set; }
    public List<string> Issues { get; set; } = [];

    public string GetComplexityLevel() => ComplexityScore switch
    {
        < 20 => "Simple",
        < 50 => "Moderate",
        < 75 => "Complex",
        _ => "Very Complex"
    };
}
