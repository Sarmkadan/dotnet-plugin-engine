#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.Loader;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Handles low-level assembly loading into isolated <see cref="AssemblyLoadContext"/> instances,
/// providing assembly resolution and safe unloading with collectibility support.
/// </summary>
public sealed class PluginLoader : IDisposable
{
    private readonly ILogger<PluginLoader> _logger;
    private readonly IOptions<PluginEngineOptions> _options;
    private readonly ConcurrentDictionary<Guid, AssemblyLoadContext> _loadContexts = new();
    private readonly ConcurrentDictionary<Guid, Assembly> _loadedAssemblies = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoader"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="options">Plugin engine configuration options.</param>
    public PluginLoader(ILogger<PluginLoader> logger, IOptions<PluginEngineOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Loads an assembly from <paramref name="assemblyPath"/> into a new collectible
    /// <see cref="AssemblyLoadContext"/> and returns the primary assembly.
    /// </summary>
    /// <param name="pluginId">Unique identifier to associate with this load context.</param>
    /// <param name="assemblyPath">Absolute path to the plugin assembly DLL.</param>
    /// <returns>The loaded <see cref="Assembly"/>.</returns>
    /// <exception cref="PluginLoadException">Thrown when the assembly cannot be loaded.</exception>
    public Assembly LoadAssembly(Guid pluginId, string assemblyPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

        if (_loadContexts.ContainsKey(pluginId))
        {
            _logger.LogWarning("Plugin {PluginId} already has a load context; unload it first.", pluginId);
            throw new PluginLoadException($"Plugin {pluginId} is already loaded.");
        }

        var fullPath = Path.GetFullPath(assemblyPath);
        if (!File.Exists(fullPath))
            throw new PluginLoadException($"Assembly not found: {fullPath}");

        _logger.LogInformation("Loading assembly for plugin {PluginId} from {Path}.", pluginId, fullPath);

        var context = new AssemblyLoadContext($"Plugin-{pluginId}", isCollectible: true);
        try
        {
            var assembly = context.LoadFromAssemblyPath(fullPath);
            _loadContexts[pluginId] = context;
            _loadedAssemblies[pluginId] = assembly;

            _logger.LogInformation("Loaded assembly {AssemblyName} for plugin {PluginId}.", assembly.FullName, pluginId);
            return assembly;
        }
        catch (Exception ex) when (ex is not PluginLoadException)
        {
            context.Unload();
            _logger.LogError(ex, "Failed to load assembly for plugin {PluginId}.", pluginId);
            throw new PluginLoadException($"Failed to load assembly from {fullPath}.", ex);
        }
    }

    /// <summary>
    /// Unloads the <see cref="AssemblyLoadContext"/> associated with the given plugin,
    /// allowing the runtime to collect the assemblies when no references remain.
    /// </summary>
    /// <param name="pluginId">The plugin whose load context should be unloaded.</param>
    /// <returns><c>true</c> if the context was found and unloaded; otherwise <c>false</c>.</returns>
    public bool UnloadAssembly(Guid pluginId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_loadContexts.TryRemove(pluginId, out var context))
        {
            _logger.LogWarning("No load context found for plugin {PluginId}.", pluginId);
            return false;
        }

        _loadedAssemblies.TryRemove(pluginId, out _);
        context.Unload();
        _logger.LogInformation("Unloaded context for plugin {PluginId}.", pluginId);
        return true;
    }

    /// <summary>
    /// Returns the assembly loaded for <paramref name="pluginId"/>, or <c>null</c> if none is loaded.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The <see cref="Assembly"/> if loaded; otherwise <c>null</c>.</returns>
    public Assembly? GetLoadedAssembly(Guid pluginId)
    {
        _loadedAssemblies.TryGetValue(pluginId, out var assembly);
        return assembly;
    }

    /// <summary>
    /// Scans a directory for <c>*.dll</c> files and returns their paths as potential plugin assemblies.
    /// Does not load them.
    /// </summary>
    /// <param name="directoryPath">The directory to scan.</param>
    /// <returns>An array of absolute paths to DLL files found.</returns>
    public string[] DiscoverAssemblies(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var dir = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(dir))
        {
            _logger.LogWarning("Plugin directory does not exist: {Dir}.", dir);
            return [];
        }

        var files = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);
        _logger.LogInformation("Discovered {Count} assemblies in {Dir}.", files.Length, dir);
        return files;
    }

    /// <summary>
    /// Returns the number of currently active load contexts.
    /// </summary>
    public int ActiveContextCount => _loadContexts.Count;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (id, context) in _loadContexts)
        {
            try
            {
                context.Unload();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unloading context for plugin {PluginId} during dispose.", id);
            }
        }

        _loadContexts.Clear();
        _loadedAssemblies.Clear();
    }
}
