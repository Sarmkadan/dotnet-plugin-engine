// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Cli;

/// <summary>
/// Main CLI host for the plugin engine. Manages command execution and user interaction.
/// Provides a command-line interface for plugin management operations.
/// </summary>
public class PluginEngineCliHost
{
    private readonly IPluginManagerService _pluginManager;
    private readonly IPluginLoaderService _pluginLoader;
    private readonly IVersioningService _versioningService;
    private readonly CommandParser _commandParser;

    public PluginEngineCliHost(
        IPluginManagerService pluginManager,
        IPluginLoaderService pluginLoader,
        IVersioningService versioningService,
        CommandParser commandParser)
    {
        _pluginManager = pluginManager;
        _pluginLoader = pluginLoader;
        _versioningService = versioningService;
        _commandParser = commandParser;
    }

    /// <summary>
    /// Entry point for CLI execution. Parses command-line arguments and routes to appropriate handler.
    /// </summary>
    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] == "help" || args[0] == "--help" || args[0] == "-h")
            {
                DisplayHelp();
                return 0;
            }

            var (commandType, arguments) = _commandParser.Parse(args);

            return commandType switch
            {
                CommandType.Load => await ExecuteLoadCommand(arguments),
                CommandType.Unload => await ExecuteUnloadCommand(arguments),
                CommandType.List => await ExecuteListCommand(arguments),
                CommandType.Status => await ExecuteStatusCommand(arguments),
                CommandType.Version => await ExecuteVersionCommand(),
                CommandType.Unknown => HandleUnknownCommand(args[0]),
                _ => 1
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> ExecuteLoadCommand(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("path", out var path))
        {
            Console.Error.WriteLine("Error: Missing required --path argument");
            return 1;
        }

        try
        {
            var plugin = await _pluginLoader.LoadPluginAsync(path);
            Console.WriteLine($"✓ Plugin loaded: {plugin.Name} v{plugin.Version}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Load failed: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> ExecuteUnloadCommand(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("id", out var pluginId))
        {
            Console.Error.WriteLine("Error: Missing required --id argument");
            return 1;
        }

        try
        {
            await _pluginManager.UnloadPluginAsync(Guid.Parse(pluginId));
            Console.WriteLine($"✓ Plugin unloaded: {pluginId}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Unload failed: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> ExecuteListCommand(Dictionary<string, string> args)
    {
        try
        {
            var plugins = await _pluginManager.GetAllPluginsAsync();

            if (plugins.Count == 0)
            {
                Console.WriteLine("No plugins loaded.");
                return 0;
            }

            Console.WriteLine("\nLoaded Plugins:");
            Console.WriteLine(new string('-', 60));

            foreach (var plugin in plugins)
            {
                Console.WriteLine($"  Name:     {plugin.Name}");
                Console.WriteLine($"  Version:  {plugin.Version}");
                Console.WriteLine($"  Status:   {plugin.Status}");
                Console.WriteLine(new string('-', 60));
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ List failed: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> ExecuteStatusCommand(Dictionary<string, string> args)
    {
        try
        {
            var health = await _pluginManager.GetHealthAsync();
            Console.WriteLine($"Plugin Engine Status: {health.Status}");
            Console.WriteLine($"Loaded Plugins:      {health.LoadedPluginCount}");
            Console.WriteLine($"Total Capacity:      {health.MaxPlugins}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Status check failed: {ex.Message}");
            return 1;
        }
    }

    private Task<int> ExecuteVersionCommand()
    {
        Console.WriteLine("Plugin Engine CLI v1.0.0");
        return Task.FromResult(0);
    }

    private int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.Error.WriteLine("Use 'help' for available commands");
        return 1;
    }

    private void DisplayHelp()
    {
        Console.WriteLine(@"
Plugin Engine CLI
Usage: plugin-engine <command> [options]

Commands:
  load      Load a plugin
            --path <path>              Path to plugin DLL file

  unload    Unload a loaded plugin
            --id <guid>                Plugin identifier

  list      List all loaded plugins

  status    Show engine status

  version   Show version information

  help      Display this help message

Examples:
  plugin-engine load --path ./plugins/MyPlugin.dll
  plugin-engine unload --id 550e8400-e29b-41d4-a716-446655440000
  plugin-engine list
  plugin-engine status
");
    }
}
