#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginEngine.Configuration;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Examples;

/// <summary>
/// AdvancedUsage demonstrates configuration, error handling, and hot reload.
/// </summary>
public sealed class AdvancedUsage
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        
        // 1. Advanced Configuration
        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = "./plugins";
            options.EnableHotReload = true;
            options.EnableLogging = true;
            options.OperationTimeoutMs = 60000;
        });

        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();
        await engine.InitializeAsync();

        // 2. Error Handling
        try
        {
            await engine.LoadAllPluginsAsync();
        }
        catch (PluginLoadException ex)
        {
            Console.WriteLine($"Error loading plugins: {ex.Message}");
        }

        // 3. Hot Reload
        var hotReloader = serviceProvider.GetRequiredService<IHotReloadService>();
        await hotReloader.StartHotReloadMonitoringAsync();
        
        await hotReloader.RegisterHotReloadCallback("plugin-id", async plugin =>
        {
            Console.WriteLine($"Plugin {plugin.Name} reloaded!");
        });
    }
}
