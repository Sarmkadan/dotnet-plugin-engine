#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Cli;

/// <summary>
/// Main CLI host for the plugin engine. Manages command execution and user interaction.
/// Provides a command-line interface for plugin management operations.
/// </summary>
public sealed class PluginEngineCliHost
{
    private readonly IPluginManagerService _pluginManager;
    private readonly IPluginLoaderService _pluginLoader;
    private readonly IVersioningService _versioningService;
    private readonly CommandParser _commandParser;
    private readonly IPluginMarketplaceService _marketplace;
    private readonly IHotSwapService _hotSwap;
    private readonly IPluginDependencyResolver _dependencyResolver;

    private const int SeparatorWidth = 60;
    private const string DefaultMarketplaceLimit = "20";
    private const int MaxMarketplaceLimit = 100;
    private const string HelpFlag = "help";
    private const string HelpFlagLong = "--help";
    private const string HelpFlagShort = "-h";
    private const string ArgPath = "path";
    private const string ArgAction = "action";
    private const string ArgQuery = "query";
    private const string ArgLimit = "limit";

    public PluginEngineCliHost(
        IPluginManagerService pluginManager,
        IPluginLoaderService pluginLoader,
        IVersioningService versioningService,
        CommandParser commandParser,
        IPluginMarketplaceService marketplace,
        IHotSwapService hotSwap,
        IPluginDependencyResolver dependencyResolver)
    {
        _pluginManager      = pluginManager;
        _pluginLoader       = pluginLoader;
        _versioningService  = versioningService;
        _commandParser      = commandParser;
        _marketplace        = marketplace;
        _hotSwap            = hotSwap;
        _dependencyResolver = dependencyResolver;
    }

    /// <summary>
    /// Entry point for CLI execution. Parses command-line arguments and routes to appropriate handler.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The exit code.</returns>
    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] == HelpFlag || args[0] == HelpFlagLong || args[0] == HelpFlagShort)
            {
                DisplayHelp();
                return 0;
            }

            var (commandType, arguments) = _commandParser.Parse(args);

            return commandType switch
            {
                CommandType.Load        => await ExecuteLoadCommand(arguments),
                CommandType.Unload      => await ExecuteUnloadCommand(arguments),
                CommandType.List        => await ExecuteListCommand(arguments),
                CommandType.Status      => await ExecuteStatusCommand(arguments),
                CommandType.Version     => await ExecuteVersionCommand(),
                CommandType.Marketplace => await ExecuteMarketplaceCommand(arguments),
                CommandType.HotSwap     => await ExecuteHotSwapCommand(arguments),
                CommandType.Resolve     => await ExecuteResolveCommand(arguments),
                CommandType.Unknown     => HandleUnknownCommand(args[0]),
                _ => 1
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Executes the load command to load a plugin from the specified path.
    /// </summary>
    /// <param name="args">The command arguments containing the plugin path.</param>
    /// <returns>The exit code indicating success or failure.</returns>
    private async Task<int> ExecuteLoadCommand(Dictionary<string, string> args)
    {
        if (!args.TryGetValue(ArgPath, out var path))
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

    /// <summary>
    /// Executes the unload command to unload a plugin by its identifier.
    /// </summary>
    /// <param name="args">The command arguments containing the plugin ID.</param>
    /// <returns>The exit code indicating success or failure.</returns>
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

    /// <summary>
    /// Executes the list command to display all loaded plugins.
    /// </summary>
    /// <param name="args">The command arguments (unused).</param>
    /// <returns>The exit code indicating success or failure.</returns>
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
            Console.WriteLine(new string('-', SeparatorWidth));

            foreach (var plugin in plugins)
            {
                Console.WriteLine($"  Name:     {plugin.Name}");
                Console.WriteLine($"  Version:  {plugin.Version}");
                Console.WriteLine($"  Status:   {plugin.Status}");
                Console.WriteLine(new string('-', SeparatorWidth));
            }

            return 0;
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ List failed: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Executes the status command to display the plugin engine status.
    /// </summary>
    /// <param name="args">The command arguments (unused).</param>
    /// <returns>The exit code indicating success or failure.</returns>
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

    /// <summary>
    /// Executes the version command to display the version information.
    /// </summary>
    /// <returns>The exit code.</returns>
    private Task<int> ExecuteVersionCommand()
    {
        Console.WriteLine("Plugin Engine CLI v1.0.0");
        return Task.FromResult(0);
    }

    /// <summary>
    /// Executes the marketplace command to interact with the plugin marketplace.
    /// </summary>
    /// <param name="args">The command arguments containing action and parameters.</param>
    /// <returns>The exit code indicating success or failure.</returns>
    private async Task<int> ExecuteMarketplaceCommand(Dictionary<string, string> args)
    {
        // Sub-commands: search, info, install, browse, trending, featured
        if (!args.TryGetValue(ArgAction, out var action))
            action = args.ContainsKey(ArgQuery) ? "search" : "trending";

        try
        {
            switch (action.ToLowerInvariant())
            {
                case "search":
                {
                    args.TryGetValue(ArgQuery, out var query);
                    int.TryParse(args.GetValueOrDefault(ArgLimit, DefaultMarketplaceLimit), out var limit);
                    var result = await _marketplace.SearchAsync(new MarketplaceSearchFilter
                    {
                        Query    = query,
                        PageSize = Math.Clamp(limit, 1, MaxMarketplaceLimit)
                    });
                    if (!result.Success) { Console.Error.WriteLine($"✗ {result.Message}"); return 1; }
                    PrintMarketplaceEntries(result.Data ?? []);
                    return 0;
                }

                case "info":
                {
                    if (!args.TryGetValue("id", out var idStr) || !Guid.TryParse(idStr, out var pluginId))
                    { Console.Error.WriteLine("Error: --id <guid> is required for 'info'"); return 1; }
                    var result = await _marketplace.GetEntryAsync(pluginId);
                    if (!result.Success) { Console.Error.WriteLine($"✗ {result.Message}"); return 1; }
                    PrintEntryDetail(result.Data!);
                    return 0;
                }

                case "install":
                {
                    if (!args.TryGetValue("id", out var idStr) || !Guid.TryParse(idStr, out var pluginId))
                    { Console.Error.WriteLine("Error: --id <guid> is required for 'install'"); return 1; }
                    args.TryGetValue("version", out var version);
                    args.TryGetValue("target", out var target);
                    target ??= "./plugins";
                    version ??= "latest";
                    var result = await _marketplace.InstallAsync(pluginId, version, target);
                    if (!result.Success) { Console.Error.WriteLine($"✗ {result.Message}"); return 1; }
                    Console.WriteLine($"✓ {result.Message}");
                    return 0;
                }

                default:
                    Console.Error.WriteLine($"Unknown marketplace action: {action}");
                    Console.Error.WriteLine("Valid actions: search, info, install");
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Marketplace command failed: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Executes the hot-swap command to replace a running plugin with a new assembly.
    /// </summary>
    /// <param name="args">The command arguments containing the plugin ID and new assembly path.</param>
    /// <returns>The exit code indicating success or failure.</returns>
    private async Task<int> ExecuteHotSwapCommand(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("id", out var idStr) || !Guid.TryParse(idStr, out var pluginId))
        { Console.Error.WriteLine("Error: --id <guid> is required"); return 1; }

        if (!args.TryGetValue(ArgPath, out var newPath))
        { Console.Error.WriteLine("Error: --path <assembly-path> is required"); return 1; }

        try
        {
            var result = await _hotSwap.SwapPluginAsync(pluginId, newPath);
            if (result.Success)
            {
                Console.WriteLine($"✓ {result.Message}");
                return 0;
            }
            Console.Error.WriteLine($"✗ Hot-swap failed: {result.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Hot-swap error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Executes the resolve command to show the dependency resolution plan for a plugin.
    /// </summary>
    /// <param name="args">The command arguments containing the plugin ID.</param>
    /// <returns>The exit code indicating success or failure.</returns>
    private async Task<int> ExecuteResolveCommand(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("id", out var idStr) || !Guid.TryParse(idStr, out var pluginId))
        { Console.Error.WriteLine("Error: --id <guid> is required"); return 1; }

        try
        {
            var result = await _dependencyResolver.BuildResolutionPlanAsync(pluginId);
            if (!result.Success) { Console.Error.WriteLine($"✗ {result.Message}"); return 1; }

            var plan = result.Data!;
            Console.WriteLine($"\nResolution Plan for plugin {pluginId}");
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"  Executable : {plan.IsExecutable}");
            Console.WriteLine($"  Steps      : {plan.Steps.Count}");
            Console.WriteLine($"  Conflicts  : {plan.Conflicts.Count}");
            Console.WriteLine();

            foreach (var step in plan.Steps)
            {
                var optional = step.IsOptional ? " (optional)" : string.Empty;
                Console.WriteLine($"  {step.Order,2}. [{step.Action,-24}] {step.PluginName} v{step.Version}{optional}");
            }

            if (plan.Conflicts.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Conflicts:");
                foreach (var conflict in plan.Conflicts)
                    Console.WriteLine($"  ⚠ {conflict.Description}");
            }

            Console.WriteLine();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Resolve error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Prints a list of marketplace entries to the console in a formatted table.
    /// </summary>
    /// <param name="entries">The list of marketplace entries to print.</param>
    private static void PrintMarketplaceEntries(List<MarketplaceEntry> entries)
    {
        if (entries.Count == 0) { Console.WriteLine("No plugins found."); return; }
        Console.WriteLine($"\n{"Name",-30} {"Version",-12} {"Author",-20} {"Downloads",10}");
        Console.WriteLine(new string('-', 76));
        foreach (var e in entries)
            Console.WriteLine($"{e.Name,-30} {e.LatestVersion,-12} {e.Author,-20} {e.Downloads,10:N0}");
        Console.WriteLine();
    }

    private static void PrintEntryDetail(MarketplaceEntry e)
    {
        Console.WriteLine($"\n  Name        : {e.Name}");
        Console.WriteLine($"  Version     : {e.LatestVersion}");
        Console.WriteLine($"  Author      : {e.Author}");
        Console.WriteLine($"  Description : {e.Description}");
        Console.WriteLine($"  Rating      : {e.Rating:F1} / 5.0");
        Console.WriteLine($"  Downloads   : {e.Downloads:N0}");
        Console.WriteLine($"  Verified    : {e.IsVerified}");
        Console.WriteLine($"  License     : {e.LicenseType}");
        if (e.AvailableVersions.Count > 0)
        {
            Console.WriteLine($"  Versions    : {string.Join(", ", e.AvailableVersions.Select(v => v.Version))}");
        }
        Console.WriteLine();
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

  marketplace  Browse and install plugins from the marketplace
            --action <action>          Action: search | info | install  (default: trending)
            --query <text>             Search query (for 'search')
            --id <guid>                Plugin identifier (for 'info' / 'install')
            --version <ver>            Version to install  (default: latest)
            --target <dir>             Installation directory  (default: ./plugins)
            --limit <n>                Max results (default: 20)

  swap      Hot-swap a running plugin with a new assembly (zero downtime)
            --id <guid>                Plugin to replace
            --path <path>              Replacement assembly path

  resolve   Show the full dependency resolution plan for a plugin
            --id <guid>                Plugin identifier

  help      Display this help message

Examples:
  plugin-engine load --path ./plugins/MyPlugin.dll
  plugin-engine unload --id 550e8400-e29b-41d4-a716-446655440000
  plugin-engine list
  plugin-engine status
  plugin-engine marketplace --action search --query logging --limit 10
  plugin-engine marketplace --action info --id <guid>
  plugin-engine marketplace --action install --id <guid> --version 2.1.0 --target ./plugins
  plugin-engine swap --id <guid> --path ./plugins/MyPlugin.v2.dll
  plugin-engine resolve --id <guid>
");
    }
}
