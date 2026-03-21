// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginEngine.Configuration;
using PluginEngine.Events;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Examples;

/// <summary>
/// AdvancedScenarios demonstrates complex plugin engine usage patterns.
/// Includes event handling, plugin composition, and custom integration.
/// </summary>
public class AdvancedScenarios
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Advanced Plugin Engine Scenarios ===\n");

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });

        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
            options.EnableHotReload = true;
            options.EnableLogging = true;
            options.MaxConcurrentPluginLoads = Environment.ProcessorCount;
        });

        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();

        await engine.InitializeAsync();

        // Scenario 1: Event-driven plugin lifecycle
        Console.WriteLine("Scenario 1: Event-Driven Plugin Lifecycle\n");
        await EventDrivenScenario(serviceProvider);

        // Scenario 2: Plugin composition and chaining
        Console.WriteLine("\n\nScenario 2: Plugin Composition\n");
        await PluginCompositionScenario(serviceProvider);

        // Scenario 3: Dynamic plugin discovery and loading
        Console.WriteLine("\n\nScenario 3: Dynamic Discovery\n");
        await DynamicDiscoveryScenario(serviceProvider);

        // Scenario 4: Plugin lifecycle management
        Console.WriteLine("\n\nScenario 4: Lifecycle Management\n");
        await LifecycleManagementScenario(serviceProvider);

        // Scenario 5: Performance monitoring and optimization
        Console.WriteLine("\n\nScenario 5: Performance Monitoring\n");
        await PerformanceMonitoringScenario(serviceProvider);

        await engine.ShutdownAsync();
        Console.WriteLine("\n✓ Advanced scenarios complete");
    }

    private static async Task EventDrivenScenario(IServiceProvider services)
    {
        Console.WriteLine("Setting up event subscriptions...\n");

        var subscriber = services.GetRequiredService<PluginEventSubscriber>();
        var manager = services.GetRequiredService<IPluginManagerService>();
        var engine = services.GetRequiredService<PluginEngine.PluginEngine>();

        // Subscribe to plugin loaded events
        subscriber.Subscribe<PluginLoadedEvent>(async @event =>
        {
            Console.WriteLine($"✓ Event: Plugin loaded");
            Console.WriteLine($"  Name: {@event.Plugin.Name}");
            Console.WriteLine($"  Version: {@event.Plugin.Version}");

            // Perform initialization based on plugin capabilities
            foreach (var capability in @event.Plugin.Capabilities)
            {
                Console.WriteLine($"  Capability: {capability.Name}");
            }

            await Task.CompletedTask;
        });

        // Subscribe to reload events
        subscriber.Subscribe<PluginReloadedEvent>(async @event =>
        {
            Console.WriteLine($"↻ Event: Plugin reloaded");
            Console.WriteLine($"  Name: {@event.Plugin.Name}");
            Console.WriteLine($"  Previous Version: {@event.PreviousVersion}");
            Console.WriteLine($"  New Version: {@event.Plugin.Version}");

            await Task.CompletedTask;
        });

        // Load plugins to trigger events
        await engine.LoadAllPluginsAsync();
    }

    private static async Task PluginCompositionScenario(IServiceProvider services)
    {
        Console.WriteLine("Demonstrating plugin composition patterns...\n");

        var manager = services.GetRequiredService<IPluginManagerService>();
        var resolver = services.GetRequiredService<IDependencyResolutionService>();

        var plugins = await manager.GetAllLoadedPluginsAsync();

        if (plugins.Count < 2)
        {
            Console.WriteLine("⚠ Need at least 2 plugins for composition example");
            return;
        }

        // Create a composition chain
        var chain = new List<string>();
        foreach (var plugin in plugins.Take(2))
        {
            chain.Add(plugin.Name);

            // Validate dependencies
            var valid = await resolver.ValidateDependenciesAsync(plugin);
            Console.WriteLine(
                $"Plugin: {plugin.Name} - Dependencies valid: {valid}"
            );
        }

        Console.WriteLine($"\n✓ Composition chain: {string.Join(" → ", chain)}");
    }

    private static async Task DynamicDiscoveryScenario(IServiceProvider services)
    {
        Console.WriteLine("Scanning for plugins and analyzing...\n");

        var manager = services.GetRequiredService<IPluginManagerService>();
        var versionService = services.GetRequiredService<IVersioningService>();

        var plugins = await manager.GetAllLoadedPluginsAsync();

        // Categorize plugins by version
        var versionGroups = plugins
            .GroupBy(p => versionService.ParseVersion(p.Version).Major)
            .OrderByDescending(g => g.Key);

        Console.WriteLine("Plugins by major version:\n");
        foreach (var group in versionGroups)
        {
            Console.WriteLine($"v{group.Key}.x:");
            foreach (var plugin in group)
            {
                Console.WriteLine($"  - {plugin.Name} ({plugin.Version})");
            }
            Console.WriteLine();
        }

        // Find plugins with no dependencies
        var standalone = plugins.Where(p => p.Dependencies.Count == 0).ToList();
        Console.WriteLine($"Standalone plugins (no dependencies): {standalone.Count}");
        foreach (var plugin in standalone)
        {
            Console.WriteLine($"  - {plugin.Name}");
        }

        // Find plugins with most dependencies
        var mostDependencies = plugins
            .OrderByDescending(p => p.Dependencies.Count)
            .FirstOrDefault();

        if (mostDependencies != null)
        {
            Console.WriteLine(
                $"\nMost dependent plugin: {mostDependencies.Name} " +
                $"({mostDependencies.Dependencies.Count} dependencies)"
            );
        }
    }

    private static async Task LifecycleManagementScenario(IServiceProvider services)
    {
        Console.WriteLine("Managing plugin lifecycle...\n");

        var manager = services.GetRequiredService<IPluginManagerService>();
        var plugins = await manager.GetAllLoadedPluginsAsync();

        if (plugins.Count == 0)
        {
            Console.WriteLine("⚠ No plugins loaded");
            return;
        }

        var plugin = plugins.First();
        Console.WriteLine($"Managing lifecycle of: {plugin.Name}\n");

        // Enable plugin
        try
        {
            await manager.EnablePluginAsync(plugin.Id);
            Console.WriteLine($"✓ Plugin enabled: {plugin.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Enable failed: {ex.Message}");
        }

        // Execute plugin
        try
        {
            var context = new PluginExecutionContext
            {
                Parameters = new Dictionary<string, object>
                {
                    { "test", "parameter" }
                }
            };

            var result = await manager.ExecutePluginAsync(plugin.Id, context);

            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ Plugin executed successfully");
                Console.WriteLine($"  Result: {result.Value}");
            }
            else
            {
                Console.WriteLine($"❌ Plugin execution failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Execution error: {ex.Message}");
        }

        // Disable plugin
        try
        {
            await manager.DisablePluginAsync(plugin.Id);
            Console.WriteLine($"✓ Plugin disabled: {plugin.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Disable failed: {ex.Message}");
        }
    }

    private static async Task PerformanceMonitoringScenario(IServiceProvider services)
    {
        Console.WriteLine("Monitoring engine performance...\n");

        var engine = services.GetRequiredService<PluginEngine.PluginEngine>();
        var hotReloader = services.GetRequiredService<IHotReloadService>();

        // Get engine statistics
        var stats = await engine.GetStatisticsAsync();
        Console.WriteLine("Engine Statistics:");
        Console.WriteLine($"  Total plugins loaded: {stats.TotalPluginsLoaded}");
        Console.WriteLine($"  Failed loads: {stats.FailedLoadCount}");
        Console.WriteLine($"  Average load time: {stats.AveragePluginLoadTimeMs}ms");

        // Get health information
        var health = await engine.GetHealthInfoAsync();
        Console.WriteLine("\nEngine Health:");
        Console.WriteLine($"  Status: {(health.IsHealthy ? "✓ Healthy" : "⚠ Degraded")}");
        Console.WriteLine($"  Loaded: {health.LoadedPluginsCount}");
        Console.WriteLine($"  Failed: {health.FailedPluginsCount}");

        // Get hot reload statistics
        var reloadStats = await hotReloader.GetStatisticsAsync();
        Console.WriteLine("\nHot Reload Statistics:");
        Console.WriteLine($"  Monitoring active: {reloadStats.IsMonitoring}");
        Console.WriteLine($"  Total reloads: {reloadStats.TotalReloads}");
        Console.WriteLine($"  Failed reloads: {reloadStats.FailedReloadCount}");
        Console.WriteLine($"  Average reload time: {reloadStats.AverageReloadTimeMs}ms");

        // Performance recommendations
        Console.WriteLine("\nPerformance Recommendations:");
        if (stats.AveragePluginLoadTimeMs > 500)
        {
            Console.WriteLine("  ⚠ High plugin load time detected");
            Console.WriteLine("    → Consider reducing plugin size or complexity");
        }

        if (health.FailedPluginsCount > 0)
        {
            Console.WriteLine("  ⚠ Failed plugins detected");
            Console.WriteLine("    → Review error logs and fix dependency issues");
        }

        if (reloadStats.FailedReloadCount > 0)
        {
            Console.WriteLine("  ⚠ Failed hot reloads detected");
            Console.WriteLine("    → Check file permissions and plugin locks");
        }

        Console.WriteLine("  ✓ All metrics within normal range");
    }
}
