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
/// HotReloadExample demonstrates automatic plugin reloading when files change.
/// This is useful for development and zero-downtime production updates.
/// </summary>
public class HotReloadExample
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Hot Reload Example ===\n");
        Console.WriteLine("This example monitors the plugins directory for changes.");
        Console.WriteLine("Modify or replace plugin DLL files to see hot reload in action.\n");

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });

        // Configure with hot reload enabled
        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
            options.EnableHotReload = true;
            options.HotReloadCheckIntervalMs = 3000; // Check every 3 seconds
            options.EnableLogging = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();

        await engine.InitializeAsync();
        await engine.LoadAllPluginsAsync();

        // Get the hot reload service
        var hotReloader = serviceProvider.GetRequiredService<IHotReloadService>();

        // Register callbacks for when plugins reload
        var manager = serviceProvider.GetRequiredService<IPluginManagerService>();
        var plugins = await manager.GetAllLoadedPluginsAsync();

        foreach (var plugin in plugins)
        {
            await hotReloader.RegisterHotReloadCallback(plugin.Id, async reloadedPlugin =>
            {
                Console.WriteLine($"\n🔄 Plugin reloaded: {reloadedPlugin.Name} v{reloadedPlugin.Version}");
                Console.WriteLine($"   Reloaded at: {DateTime.Now:HH:mm:ss}");

                // Perform any necessary re-initialization here
                await Task.CompletedTask;
            });
        }

        // Start monitoring for changes
        Console.WriteLine("📁 Starting hot reload monitoring...\n");
        await hotReloader.StartHotReloadMonitoringAsync();

        Console.WriteLine("✓ Monitoring started. Press Ctrl+C to exit.\n");

        // Keep the application running
        try
        {
            while (true)
            {
                await Task.Delay(5000);

                // Display periodic statistics
                var stats = await hotReloader.GetStatisticsAsync();
                Console.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss}] Monitoring active: {stats.IsMonitoring}, " +
                    $"Total reloads: {stats.TotalReloads}, " +
                    $"Avg time: {stats.AverageReloadTimeMs}ms"
                );
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n\n⏹ Shutting down...");

            // Stop monitoring
            await hotReloader.StopHotReloadMonitoringAsync();

            // Display final statistics
            var finalStats = await hotReloader.GetStatisticsAsync();
            Console.WriteLine($"\n=== Final Statistics ===");
            Console.WriteLine($"Total reloads: {finalStats.TotalReloads}");
            Console.WriteLine($"Average reload time: {finalStats.AverageReloadTimeMs}ms");
            Console.WriteLine($"Failed reloads: {finalStats.FailedReloadCount}");

            // Shutdown engine
            await engine.ShutdownAsync();
            Console.WriteLine("✓ Shutdown complete");
        }
    }
}
