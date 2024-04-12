#nullable enable
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Examples;

/// <summary>
/// BasicUsage demonstrates the minimal setup required to load and list plugins.
/// </summary>
public sealed class BasicUsage
{
    public static async Task Main(string[] args)
    {
        // 1. Setup DI
        var services = new ServiceCollection();
        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
        });

        var serviceProvider = services.BuildServiceProvider();

        // 2. Initialize
        var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();
        await engine.InitializeAsync();

        // 3. Load and use
        await engine.LoadAllPluginsAsync();
        
        var manager = serviceProvider.GetRequiredService<IPluginManagerService>();
        var plugins = await manager.GetAllLoadedPluginsAsync();

        foreach (var plugin in plugins)
        {
            Console.WriteLine($"Loaded: {plugin.Name} v{plugin.Version}");
        }
    }
}
