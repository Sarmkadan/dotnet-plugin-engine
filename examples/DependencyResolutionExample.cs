// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginEngine.Configuration;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Examples;

/// <summary>
/// DependencyResolutionExample demonstrates sophisticated dependency analysis.
/// Includes transitive resolution, validation, and circular dependency detection.
/// </summary>
public class DependencyResolutionExample
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dependency Resolution Example ===\n");

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });

        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
            options.EnableDependencyCaching = true;
            options.DependencyCacheTTLMs = 300000;
            options.EnableCircularDependencyDetection = true;
            options.EnableLogging = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();

        await engine.InitializeAsync();
        var loadedCount = await engine.LoadAllPluginsAsync();
        Console.WriteLine($"Loaded {loadedCount} plugins\n");

        var resolver = serviceProvider.GetRequiredService<IDependencyResolutionService>();
        var manager = serviceProvider.GetRequiredService<IPluginManagerService>();
        var plugins = await manager.GetAllLoadedPluginsAsync();

        foreach (var plugin in plugins)
        {
            Console.WriteLine($"📦 {plugin.Name}");

            // Check for circular dependencies
            var hasCircular = await resolver.HasCircularDependenciesAsync(plugin);
            if (hasCircular)
            {
                Console.WriteLine("   ⚠ WARNING: Circular dependency detected!");
            }

            // Validate dependencies
            var isValid = await resolver.ValidateDependenciesAsync(plugin);
            if (!isValid)
            {
                Console.WriteLine("   ❌ Missing required dependencies");
            }
            else
            {
                Console.WriteLine("   ✓ Dependencies satisfied");
            }

            // Resolve transitive dependencies
            try
            {
                var dependencies = await resolver.ResolveDependenciesAsync(plugin);
                Console.WriteLine($"   Direct dependencies: {plugin.Dependencies.Count}");
                Console.WriteLine($"   Total (transitive): {dependencies.Count}");

                foreach (var dep in dependencies)
                {
                    Console.WriteLine($"     - {dep.Name}: {dep.VersionConstraint}");
                }
            }
            catch (DependencyResolutionException ex)
            {
                Console.WriteLine("   ❌ Resolution failed:");
                foreach (var unresolved in ex.UnresolvedDependencies)
                {
                    Console.WriteLine($"     - Missing: {unresolved}");
                }
            }

            // Get dependency graph visualization
            try
            {
                var graph = await resolver.GetDependencyGraphAsync(plugin.Id);
                Console.WriteLine("   Dependency tree:");
                PrintDependencyTree(graph, plugin.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Could not build dependency tree: {ex.Message}");
            }

            Console.WriteLine();
        }

        // Example: Find all plugins with unresolved dependencies
        Console.WriteLine("=== Dependency Health Check ===\n");
        var unhealthyPlugins = new List<(string name, List<string> missing)>();

        foreach (var plugin in plugins)
        {
            try
            {
                var valid = await resolver.ValidateDependenciesAsync(plugin);
                if (!valid)
                {
                    var deps = await resolver.ResolveDependenciesAsync(plugin);
                    var missing = plugin.Dependencies
                        .Where(d => !deps.Any(r => r.Name == d.Name))
                        .Select(d => d.Name)
                        .ToList();

                    unhealthyPlugins.Add((plugin.Name, missing));
                }
            }
            catch
            {
                unhealthyPlugins.Add((plugin.Name, new List<string> { "Unknown" }));
            }
        }

        if (unhealthyPlugins.Count == 0)
        {
            Console.WriteLine("✓ All plugins have satisfied dependencies");
        }
        else
        {
            Console.WriteLine($"⚠ {unhealthyPlugins.Count} plugins have unresolved dependencies:");
            foreach (var (name, missing) in unhealthyPlugins)
            {
                Console.WriteLine($"  {name}:");
                foreach (var dep in missing)
                {
                    Console.WriteLine($"    - {dep}");
                }
            }
        }

        await engine.ShutdownAsync();
    }

    private static void PrintDependencyTree(
        object graph,
        string pluginId,
        int depth = 0,
        HashSet<string>? visited = null)
    {
        visited ??= new HashSet<string>();

        if (visited.Contains(pluginId))
        {
            Console.WriteLine(new string(' ', (depth + 2) * 2) + "...(circular)");
            return;
        }

        visited.Add(pluginId);

        // This is a simplified example. In real usage, you'd navigate the graph object structure
        var indent = new string(' ', (depth + 2) * 2);
        Console.WriteLine($"{indent}- {pluginId}");
    }
}
