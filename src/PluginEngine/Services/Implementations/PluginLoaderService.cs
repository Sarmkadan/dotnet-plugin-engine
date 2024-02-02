#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Runtime.Loader;
using PluginEngine.Domain.Entities;
using PluginEngine.Exceptions;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Service implementation for loading and unloading plugins.
/// </summary>
public sealed class PluginLoaderService : IPluginLoaderService
{
    private sealed class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginAssemblyLoadContext(string name, string pluginPath) : base(name, isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }
            return IntPtr.Zero;
        }
    }

    private readonly Dictionary<Guid, (Plugin Plugin, AssemblyLoadContext Context)> _loadedPlugins = new();
    private readonly object _lockObject = new object();
    private readonly IServiceProvider? _serviceProvider;

    public PluginLoaderService(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Loads a plugin from the specified assembly path.
    /// </summary>
    public async Task<Plugin> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
            throw new ArgumentException("Assembly path cannot be empty.", nameof(assemblyPath));

        if (!File.Exists(assemblyPath))
            throw new PluginLoadException($"Assembly file not found: {assemblyPath}", "ASSEMBLY_NOT_FOUND", assemblyPath, PluginLoadStage.AssemblyResolution);

        try
        {
            return await Task.Run(() =>
            {
                var fullPath = Path.GetFullPath(assemblyPath);
                var context = new PluginAssemblyLoadContext($"PluginContext_{Guid.NewGuid()}", fullPath);
                var assembly = context.LoadFromAssemblyPath(fullPath);

                var assemblyName = assembly.GetName();
                var assemblyNameStr = assemblyName.Name ?? "Unknown";
                var versionStr = assemblyName.Version?.ToString() ?? "1.0.0";
                var plugin = new Plugin
                {
                    Id = Guid.NewGuid(),
                    Name = assemblyNameStr,
                    AssemblyPath = assemblyPath,
                    LoadContextId = context.Name ?? "Unknown",
                    Status = PluginStatus.Loaded,
                    Version = versionStr
                };

                lock (_lockObject)
                {
                    _loadedPlugins[plugin.Id] = (plugin, context);
                }

                return plugin;
            }, cancellationToken);
        }
        catch (Exception ex) when (!(ex is PluginException))
        {
            throw new PluginLoadException($"Failed to load plugin: {ex.Message}", "ASSEMBLY_NOT_FOUND", assemblyPath, PluginLoadStage.AssemblyResolution, ex);
        }
    }

    /// <summary>
    /// Unloads a previously loaded plugin.
    /// </summary>
    public async Task<bool> UnloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            lock (_lockObject)
            {
                if (!_loadedPlugins.TryGetValue(pluginId, out var data))
                    return false;

                var (plugin, context) = data;
                plugin.Status = PluginStatus.Unloading;

                try
                {
                    context.Unload();

                    // Hotfix: Explicitly dispose the AssemblyLoadContext to prevent memory leaks
                    // The context.Unload() marks the context for unloading but doesn't immediately
                    // release all resources. Disposing ensures proper cleanup.
                    var contextField = context.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (contextField?.GetValue(context) is bool disposed && !disposed)
                    {
                        context.Dispose();
                    }

                    if (_serviceProvider != null)
                    {
                        // Clean up event subscribers
                        var publisher = _serviceProvider.GetService(typeof(PluginEngine.Events.IPluginEventPublisher)) as PluginEngine.Events.IPluginEventPublisher;
                        publisher?.RemoveSubscribersForContext(context);

                        var subscriber = _serviceProvider.GetService(typeof(PluginEngine.Events.IPluginEventSubscriber)) as PluginEngine.Events.IPluginEventSubscriber;
                        subscriber?.RemoveSubscribersForContext(context);

                        // Clean up hot reload callbacks
                        var hotReloader = _serviceProvider.GetService(typeof(IHotReloadService)) as IHotReloadService;
                        hotReloader?.RemoveCallbacksForContext(context);
                    }

                    _loadedPlugins.Remove(pluginId);
                    plugin.Status = PluginStatus.Unloaded;
                    return true;
                }
                catch
                {
                    plugin.Status = PluginStatus.Failed;
                    return false;
                }
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Gets a loaded plugin by ID.
    /// </summary>
    public async Task<Plugin?> GetLoadedPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            lock (_lockObject)
            {
                return _loadedPlugins.TryGetValue(pluginId, out var data) ? data.Plugin : null;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Gets all currently loaded plugins.
    /// </summary>
    public async Task<IEnumerable<Plugin>> GetAllLoadedPluginsAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            lock (_lockObject)
            {
                return _loadedPlugins.Values.Select(d => d.Plugin).ToList();
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Checks if a plugin is loaded.
    /// </summary>
    public async Task<bool> IsPluginLoadedAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            lock (_lockObject)
            {
                return _loadedPlugins.ContainsKey(pluginId);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Loads all plugins from a directory.
    /// </summary>
    public async Task<IEnumerable<Plugin>> LoadPluginsFromDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var loadedPlugins = new List<Plugin>();
        var dllFiles = Directory.GetFiles(directoryPath, "*.dll");

        foreach (var dllPath in dllFiles)
        {
            try
            {
                var plugin = await LoadPluginAsync(dllPath, cancellationToken);
                loadedPlugins.Add(plugin);
            }
            catch (Exception ex)
            {
                // Log but continue loading other plugins
                Console.WriteLine($"Failed to load plugin {dllPath}: {ex.Message}");
            }
        }

        return loadedPlugins;
    }

    /// <summary>
    /// Reloads a plugin.
    /// </summary>
    public async Task<Plugin> ReloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        Plugin? existingPlugin = await GetLoadedPluginAsync(pluginId, cancellationToken);
        if (existingPlugin is null)
            throw new PluginException($"Plugin {pluginId} is not loaded.", "PLUGIN_NOT_FOUND");

        var assemblyPath = existingPlugin.AssemblyPath;
        await UnloadPluginAsync(pluginId, cancellationToken);

        return await LoadPluginAsync(assemblyPath, cancellationToken);
    }
}
