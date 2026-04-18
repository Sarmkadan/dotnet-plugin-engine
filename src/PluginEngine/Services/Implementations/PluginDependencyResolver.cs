#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Default implementation of <see cref="IPluginDependencyResolver"/>.
/// Uses Kahn's algorithm for topological ordering, and scans all dependency
/// declarations for incompatible version constraints to surface conflicts early.
/// </summary>
public sealed class PluginDependencyResolver : IPluginDependencyResolver
{
    private readonly IPluginLoaderService _loader;
    private readonly ILogger<PluginDependencyResolver> _logger;

    /// <summary>Initialises a new instance of <see cref="PluginDependencyResolver"/>.</summary>
    public PluginDependencyResolver(
        IPluginLoaderService loader,
        ILogger<PluginDependencyResolver> logger)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<PluginOperationResult<List<Plugin>>> GetInstallOrderAsync(
        IEnumerable<Plugin> plugins, CancellationToken cancellationToken = default)
    {
        var pluginList = plugins?.ToList() ?? throw new ArgumentNullException(nameof(plugins));

        try
        {
            var ordered = TopologicalSort(pluginList);
            _logger.LogDebug("Topological sort produced {Count} step(s) for {Input} plugin(s)",
                ordered.Count, pluginList.Count);

            return Task.FromResult(
                PluginOperationResult<List<Plugin>>.CreateSuccess(ordered,
                    $"Install order computed: {ordered.Count} plugin(s)."));
        }
        catch (DependencyResolutionException ex)
        {
            _logger.LogError(ex, "Circular dependency detected during topological sort");
            return Task.FromResult(
                PluginOperationResult<List<Plugin>>.CreateFailure(ex.Message, 1002));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute install order");
            return Task.FromResult(PluginOperationResult<List<Plugin>>.FromException(ex));
        }
    }

    /// <inheritdoc />
    public Task<PluginOperationResult<List<DependencyConflict>>> FindConflictsAsync(
        IEnumerable<Plugin> plugins, CancellationToken cancellationToken = default)
    {
        var pluginList = plugins?.ToList() ?? throw new ArgumentNullException(nameof(plugins));

        try
        {
            var conflicts = DetectConflicts(pluginList);
            _logger.LogDebug("Conflict scan found {Count} conflict(s) in {Total} plugin(s)",
                conflicts.Count, pluginList.Count);

            return Task.FromResult(
                PluginOperationResult<List<DependencyConflict>>.CreateSuccess(conflicts,
                    conflicts.Count == 0
                        ? "No conflicts detected."
                        : $"{conflicts.Count} conflict(s) detected."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect dependency conflicts");
            return Task.FromResult(PluginOperationResult<List<DependencyConflict>>.FromException(ex));
        }
    }

    /// <inheritdoc />
    public async Task<PluginOperationResult<DependencyResolutionPlan>> BuildResolutionPlanAsync(
        Guid pluginId, CancellationToken cancellationToken = default)
    {
        try
        {
            var root = await _loader.GetLoadedPluginAsync(pluginId, cancellationToken);
            if (root is null)
                return PluginOperationResult<DependencyResolutionPlan>.CreateFailure(
                    $"Plugin {pluginId} is not loaded.", 404);

            var allPlugins = (await _loader.GetAllLoadedPluginsAsync(cancellationToken)).ToList();
            var pluginMap  = allPlugins.ToDictionary(p => p.Id);

            // Collect full transitive closure rooted at this plugin
            var transitive = CollectTransitive(root, pluginMap, new HashSet<Guid>());
            transitive.Insert(0, root);

            var conflicts = DetectConflicts(transitive);
            var orderedSteps = TopologicalSort(transitive);

            int order = 1;
            var steps = orderedSteps.Select(p =>
            {
                // Determine the action for this plugin in the plan
                var action = p.Id == root.Id
                    ? ResolutionAction.Install
                    : ResolutionAction.AlreadySatisfied;

                // Check if any dependency of root declares this plugin but the version is off
                var dep = root.Dependencies.FirstOrDefault(d => d.DependencyPluginId == p.Id);
                if (dep is not null && !dep.IsSatisfiedBy(p.Version))
                    action = ResolutionAction.Upgrade;

                if (conflicts.Any(c => c.DependencyPluginId == p.Id))
                    action = ResolutionAction.ManualResolutionRequired;

                return new ResolutionStep
                {
                    Order      = order++,
                    PluginId   = p.Id,
                    PluginName = p.Name,
                    Version    = p.Version,
                    Action     = action,
                    IsOptional = dep?.IsOptional ?? false
                };
            }).ToList();

            var plan = new DependencyResolutionPlan
            {
                RootPluginId    = pluginId,
                Steps           = steps,
                Conflicts       = conflicts,
                GeneratedAtUtc  = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Resolution plan for plugin {PluginId}: {Steps} step(s), {Conflicts} conflict(s)",
                pluginId, steps.Count, conflicts.Count);

            return PluginOperationResult<DependencyResolutionPlan>.CreateSuccess(plan,
                plan.IsExecutable
                    ? "Resolution plan is ready to execute."
                    : $"Resolution plan contains {conflicts.Count} conflict(s) requiring manual resolution.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build resolution plan for plugin {PluginId}", pluginId);
            return PluginOperationResult<DependencyResolutionPlan>.FromException(ex);
        }
    }

    // ── Topological sort (Kahn's algorithm) ───────────────────────────────────

    private static List<Plugin> TopologicalSort(List<Plugin> plugins)
    {
        var idSet    = new HashSet<Guid>(plugins.Select(p => p.Id));
        var inDegree = plugins.ToDictionary(p => p.Id, _ => 0);
        var adjacency = new Dictionary<Guid, List<Guid>>();

        foreach (var p in plugins)
            adjacency[p.Id] = [];

        foreach (var plugin in plugins)
        {
            foreach (var dep in plugin.Dependencies)
            {
                if (!idSet.Contains(dep.DependencyPluginId))
                    continue; // dependency outside the provided set – skip

                // dep must come before plugin → dep → plugin edge
                adjacency[dep.DependencyPluginId].Add(plugin.Id);
                inDegree[plugin.Id]++;
            }
        }

        var queue  = new Queue<Guid>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var result = new List<Plugin>();
        var map    = plugins.ToDictionary(p => p.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(map[current]);

            foreach (var neighbour in adjacency[current])
            {
                if (--inDegree[neighbour] == 0)
                    queue.Enqueue(neighbour);
            }
        }

        if (result.Count != plugins.Count)
            throw new DependencyResolutionException(
                "Circular dependency detected: could not produce a valid install order.",
                DependencyResolutionReason.CircularDependency);

        return result;
    }

    // ── Conflict detection ────────────────────────────────────────────────────

    private static List<DependencyConflict> DetectConflicts(List<Plugin> plugins)
    {
        // Group all dependency declarations by the shared dependency plugin id
        var requirementGroups = new Dictionary<Guid, List<(Plugin Requirer, PluginDependency Dep)>>();

        foreach (var plugin in plugins)
        {
            foreach (var dep in plugin.Dependencies)
            {
                if (!requirementGroups.TryGetValue(dep.DependencyPluginId, out var list))
                {
                    list = [];
                    requirementGroups[dep.DependencyPluginId] = list;
                }
                list.Add((plugin, dep));
            }
        }

        var conflicts = new List<DependencyConflict>();

        foreach (var (depId, requirements) in requirementGroups)
        {
            if (requirements.Count < 2)
                continue; // Nothing to conflict with

            // Check if all requirements can be satisfied simultaneously by at least one version
            // We detect conflicts by checking min/max bounds overlap.
            var conflict = FindConflictInRequirements(depId, requirements);
            if (conflict is not null)
                conflicts.Add(conflict);
        }

        return conflicts;
    }

    private static DependencyConflict? FindConflictInRequirements(
        Guid depId, List<(Plugin Requirer, PluginDependency Dep)> requirements)
    {
        Version? highestMin  = null;
        Version? lowestMax   = null;

        foreach (var (_, dep) in requirements)
        {
            if (Version.TryParse(dep.MinimumVersion, out var min))
                highestMin = highestMin is null || min > highestMin ? min : highestMin;

            if (!string.IsNullOrWhiteSpace(dep.MaximumVersion) &&
                Version.TryParse(dep.MaximumVersion, out var max))
                lowestMax = lowestMax is null || max < lowestMax ? max : lowestMax;
        }

        // Conflict: the tightest lower bound exceeds the tightest upper bound
        if (lowestMax is not null && highestMin is not null && highestMin > lowestMax)
        {
            return new DependencyConflict
            {
                DependencyPluginId  = depId,
                DependencyName      = requirements[0].Dep.DependencyPluginId.ToString(),
                ConflictingRequirements = requirements.Select(r => new ConflictingRequirement
                {
                    RequiringPluginId   = r.Requirer.Id,
                    RequiringPluginName = r.Requirer.Name,
                    VersionConstraint   = r.Dep.GetVersionConstraint()
                }).ToList(),
                Description = $"Incompatible constraints: minimum required is {highestMin} " +
                              $"but upper bound is {lowestMax}."
            };
        }

        return null;
    }

    // ── Transitive closure ────────────────────────────────────────────────────

    private static List<Plugin> CollectTransitive(
        Plugin root, Dictionary<Guid, Plugin> pluginMap, HashSet<Guid> visited)
    {
        var result = new List<Plugin>();
        foreach (var dep in root.Dependencies)
        {
            if (!visited.Add(dep.DependencyPluginId))
                continue;

            if (!pluginMap.TryGetValue(dep.DependencyPluginId, out var depPlugin))
                continue;

            result.AddRange(CollectTransitive(depPlugin, pluginMap, visited));
            result.Add(depPlugin);
        }
        return result;
    }
}
