// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginEngine.Configuration;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Examples;

/// <summary>
/// BasicPluginHost demonstrates the simplest way to use dotnet-plugin-engine.
/// This example loads all plugins from a directory and displays their information.
/// </summary>
public class BasicPluginHost
{
    public static async Task Main(string[] args)
    {
        // Configure services
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });

        // Add the plugin engine with basic configuration
        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
            options.EnableHotReload = false; // Keep simple for this example
            options.EnableLogging = true;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Initialize the engine
        var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();
        await engine.InitializeAsync();

        Console.WriteLine("=== Basic Plugin Host ===\n");

        // Load all plugins
        Console.WriteLine("Loading plugins...");
        var loadedCount = await engine.LoadAllPluginsAsync();
        Console.WriteLine($"✓ Loaded {loadedCount} plugins\n");

        // Get and display plugin information
        var manager = serviceProvider.GetRequiredService<IPluginManagerService>();
        var plugins = await manager.GetAllLoadedPluginsAsync();

        if (plugins.Count == 0)
        {
            Console.WriteLine("No plugins loaded. Create plugin DLLs in the 'plugins' directory.");
            return;
        }

        foreach (var plugin in plugins)
        {
            Console.WriteLine($"📦 {plugin.Name} v{plugin.Version}");
            Console.WriteLine($"   ID: {plugin.Id}");

            if (plugin.Metadata?.Description != null)
            {
                Console.WriteLine($"   Description: {plugin.Metadata.Description}");
            }

            if (plugin.Metadata?.Author != null)
            {
                Console.WriteLine($"   Author: {plugin.Metadata.Author}");
            }

            if (plugin.Dependencies.Count > 0)
            {
                Console.WriteLine("   Dependencies:");
                foreach (var dep in plugin.Dependencies)
                {
                    Console.WriteLine($"     - {dep.Name} {dep.VersionConstraint}");
                }
            }

            if (plugin.Capabilities.Count > 0)
            {
                Console.WriteLine("   Capabilities:");
                foreach (var cap in plugin.Capabilities)
                {
                    Console.WriteLine($"     - {cap.Name} v{cap.Version}");
                }
            }

            Console.WriteLine();
        }

        // Display engine health information
        var health = await engine.GetHealthInfoAsync();
        Console.WriteLine("=== Engine Health ===");
        Console.WriteLine($"Status: {(health.IsHealthy ? "✓ Healthy" : "⚠ Degraded")}");
        Console.WriteLine($"Loaded: {health.LoadedPluginsCount}");
        Console.WriteLine($"Failed: {health.FailedPluginsCount}");
        Console.WriteLine($"Avg Load Time: {health.AveragePluginLoadTimeMs}ms");

        // Graceful shutdown
        await engine.ShutdownAsync();
        Console.WriteLine("\n✓ Shutdown complete");
    }
}
