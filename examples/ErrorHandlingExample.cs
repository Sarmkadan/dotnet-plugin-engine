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
/// ErrorHandlingExample demonstrates comprehensive error handling and recovery strategies.
/// Shows how to handle different exception types and implement recovery logic.
/// </summary>
public class ErrorHandlingExample
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Error Handling Example ===\n");

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug); // Enable debug for detailed errors
        });

        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
            options.EnableLogging = true;
            options.MinimumLogLevel = LogLevel.Debug;
        });

        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();
        var loader = serviceProvider.GetRequiredService<IPluginLoaderService>();
        var manager = serviceProvider.GetRequiredService<IPluginManagerService>();

        await engine.InitializeAsync();

        // Example 1: Handle plugin loading errors
        Console.WriteLine("1️⃣  Loading with error handling:\n");
        await LoadPluginWithErrorHandling(loader, "invalid-plugin.dll");
        await LoadPluginWithErrorHandling(loader, "missing-dependency.dll");

        // Example 2: Handle dependency resolution errors
        Console.WriteLine("\n2️⃣  Dependency resolution errors:\n");
        await engine.LoadAllPluginsAsync();
        await HandleDependencyErrors(manager);

        // Example 3: Handle version constraint violations
        Console.WriteLine("\n3️⃣  Version mismatch handling:\n");
        await HandleVersionMismatch(manager);

        // Example 4: Implement recovery strategies
        Console.WriteLine("\n4️⃣  Recovery strategies:\n");
        await ImplementRecoveryStrategy(loader, manager);

        await engine.ShutdownAsync();
        Console.WriteLine("\n✓ Example complete");
    }

    private static async Task LoadPluginWithErrorHandling(IPluginLoaderService loader, string pluginPath)
    {
        try
        {
            Console.WriteLine($"Loading: {pluginPath}");
            var plugin = await loader.LoadPluginAsync(pluginPath);
            Console.WriteLine($"✓ Loaded: {plugin.Name}\n");
        }
        catch (PluginLoadException ex)
        {
            Console.WriteLine($"❌ Load failed at stage: {ex.LoadStage}");
            Console.WriteLine($"   Error: {ex.Message}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Details: {ex.InnerException.Message}");
            }

            // Implement recovery based on load stage
            switch (ex.LoadStage)
            {
                case PluginLoadException.PluginLoadStage.Discovery:
                    Console.WriteLine("   → Action: Verify plugin file path and name\n");
                    break;
                case PluginLoadException.PluginLoadStage.AssemblyLoad:
                    Console.WriteLine("   → Action: Check .NET framework compatibility\n");
                    break;
                case PluginLoadException.PluginLoadStage.MetadataExtraction:
                    Console.WriteLine("   → Action: Verify plugin assembly metadata\n");
                    break;
                case PluginLoadException.PluginLoadStage.ALCCreation:
                    Console.WriteLine("   → Action: Check system resources (memory, file handles)\n");
                    break;
            }
        }
        catch (PluginException ex)
        {
            Console.WriteLine($"❌ Plugin error: {ex.Message}");
            Console.WriteLine($"   Error code: {ex.ErrorCode}\n");
        }
    }

    private static async Task HandleDependencyErrors(IPluginManagerService manager)
    {
        var plugins = await manager.GetAllLoadedPluginsAsync();

        foreach (var plugin in plugins)
        {
            try
            {
                // Attempt to enable plugin (validates dependencies)
                await manager.EnablePluginAsync(plugin.Id);
            }
            catch (DependencyResolutionException ex)
            {
                Console.WriteLine($"❌ {plugin.Name} has unresolved dependencies:");

                foreach (var unresolved in ex.UnresolvedDependencies)
                {
                    Console.WriteLine($"   - Missing: {unresolved}");
                }

                foreach (var violation in ex.VersionViolations)
                {
                    Console.WriteLine($"   - Version violation: {violation.PluginName} " +
                        $"({violation.Required} vs {violation.Available})");
                }

                // Recovery: Attempt to load missing dependencies from registry
                Console.WriteLine("   → Action: Download missing dependencies from registry");
                Console.WriteLine("   → Then retry enabling plugin\n");
            }
        }
    }

    private static async Task HandleVersionMismatch(IPluginManagerService manager)
    {
        var plugins = await manager.GetAllLoadedPluginsAsync();

        foreach (var plugin in plugins)
        {
            if (plugin.Version.StartsWith("0."))
            {
                Console.WriteLine($"⚠ Pre-release: {plugin.Name} v{plugin.Version}");
                Console.WriteLine("  Note: Pre-release versions may have breaking changes\n");
            }
        }

        // Example version handling
        Console.WriteLine("Version compatibility strategies:");
        Console.WriteLine("  1. Strict: Require exact version matches");
        Console.WriteLine("  2. Compatible: Allow patch-level changes (same major.minor)");
        Console.WriteLine("  3. Lenient: Allow any version meeting basic constraints");
        Console.WriteLine();
    }

    private static async Task ImplementRecoveryStrategy(
        IPluginLoaderService loader,
        IPluginManagerService manager)
    {
        Console.WriteLine("Recovery strategies for different scenarios:\n");

        Console.WriteLine("Strategy 1: Automatic Retry");
        Console.WriteLine("  - Retry loading with exponential backoff");
        Console.WriteLine("  - Useful for transient failures (resource exhaustion, locks)");
        const int maxRetries = 3;
        var delay = 100;

        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                Console.WriteLine($"  Attempt {i}/{maxRetries}...");
                await Task.Delay(delay);
                // Simulated retry
                delay *= 2; // Exponential backoff
            }
            catch (Exception ex)
            {
                if (i == maxRetries)
                {
                    Console.WriteLine($"  Failed after {maxRetries} attempts: {ex.Message}");
                }
            }
        }

        Console.WriteLine("\nStrategy 2: Fallback to Default");
        Console.WriteLine("  - Use default/mock plugin if load fails");
        Console.WriteLine("  - Allows application to continue with degraded functionality");

        Console.WriteLine("\nStrategy 3: Disable Problematic Plugin");
        Console.WriteLine("  - Mark plugin as disabled");
        Console.WriteLine("  - Log error for later investigation");
        Console.WriteLine("  - Continue loading other plugins");

        Console.WriteLine("\nStrategy 4: Circuit Breaker");
        Console.WriteLine("  - Track failure count");
        Console.WriteLine("  - Open circuit after threshold");
        Console.WriteLine("  - Prevent cascading failures");

        Console.WriteLine("\nStrategy 5: Rollback to Previous Version");
        Console.WriteLine("  - Keep backup of previous working version");
        Console.WriteLine("  - Restore on load failure");
        Console.WriteLine("  - Maintain service continuity");

        await Task.CompletedTask;
    }
}
