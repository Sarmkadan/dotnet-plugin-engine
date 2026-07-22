#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Reflection;
using System.Runtime.Loader;
using PluginEngine.Constants;
using PluginEngine.Domain.Entities;
using PluginEngine.Events;
using PluginEngine.Exceptions;
using PluginEngine.Execution;
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

private readonly Dictionary<Guid, (Plugin Plugin, AssemblyLoadContext Context, List<IPluginLifecycle> Lifecycles)> _loadedPlugins = new();
private readonly object _lockObject = new object();
private readonly IServiceProvider? _serviceProvider;
private readonly ILogger<PluginLoaderService>? _logger;

/// <summary>
/// Initializes a new instance of the <see cref="PluginLoaderService"/> class.
/// </summary>
/// <param name="serviceProvider">Optional provider used to resolve a logger and the versioning service.</param>
public PluginLoaderService(IServiceProvider? serviceProvider = null)
{
_serviceProvider = serviceProvider;
_logger = serviceProvider?.GetService(typeof(ILogger<PluginLoaderService>)) as ILogger<PluginLoaderService>;
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
// Create context and load assembly outside Task.Run to keep async lifecycle invocation clean,
// but for safety we keep it inside or we await them inside Task.Run.
var result = await Task.Run(async () =>
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
Status = PluginStatus.Loading,
Version = versionStr
};

var directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
var pluginJsonPath = Path.Combine(directory, "plugin.json");

if (!File.Exists(pluginJsonPath))
{
pluginJsonPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(fullPath) + PluginEngineConstants.MetadataFileExtension);
}

if (File.Exists(pluginJsonPath))
{
try
{
var json = File.ReadAllText(pluginJsonPath);
plugin.Metadata = System.Text.Json.JsonSerializer.Deserialize<PluginMetadata>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (plugin.Metadata != null && !string.IsNullOrWhiteSpace(plugin.Metadata.EngineVersionConstraint))
{
var versioningService = _serviceProvider?.GetService(typeof(IVersioningService)) as IVersioningService;
if (versioningService != null)
{
var hostVersion = typeof(PluginLoaderService).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
if (!versioningService.IsSatisfiedBy(plugin.Metadata.EngineVersionConstraint, hostVersion))
{
context.Unload();
throw new PluginIncompatibleException(plugin.Name, plugin.Metadata.EngineVersionConstraint, hostVersion);
}
}
}
}
catch (System.Text.Json.JsonException ex)
{
_logger?.LogWarning(ex, "Invalid plugin metadata JSON, ignoring: {MetadataPath}", pluginJsonPath);
}
}

var lifecycles = new List<global::PluginEngine.Execution.IPluginLifecycle>();
foreach (var type in assembly.GetTypes())
{
if (typeof(global::PluginEngine.Execution.IPluginLifecycle).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
{
if (Activator.CreateInstance(type) is global::PluginEngine.Execution.IPluginLifecycle instance)
{
lifecycles.Add(instance);
}
}
}

foreach (var lifecycle in lifecycles)
{
await lifecycle.OnBeforeLoadAsync(cancellationToken);
}

plugin.Status = PluginStatus.Loaded;

lock (_lockObject)
{
_loadedPlugins[plugin.Id] = (plugin, context, lifecycles);
}

foreach (var lifecycle in lifecycles)
{
await lifecycle.OnAfterLoadAsync(cancellationToken);
}

return plugin;
}, cancellationToken);

return result;
}
catch (Exception ex) when (!(ex is PluginException))
{
throw new PluginLoadException($"Failed to load plugin: {ex.Message}", "ASSEMBLY_NOT_FOUND", assemblyPath, PluginLoadStage.AssemblyResolution, ex);
}
}

/// <summary>
/// Unloads a previously loaded plugin.
/// <para>This method ensures that the plugin's AssemblyLoadContext can be properly unloaded by:
/// <list type="bullet">
/// <item>Clearing all lifecycle instances that hold references to plugin types</item>
/// <item>Removing the plugin entry from the loaded plugins dictionary</item>
/// <item>Disposing any hot reload callbacks associated with the context</item>
/// <item>Unloading the AssemblyLoadContext to allow memory reclamation</item>
/// </list></para>
/// <para><strong>Invariant:</strong> After this method completes, no strong references to plugin types
/// or the AssemblyLoadContext should remain, allowing the context to be collected by the garbage collector.</para>
/// </summary>
/// <param name="pluginId">The unique ID of the plugin to unload.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>A task representing the asynchronous operation, containing <c>true</c> if successful, otherwise <c>false</c>.</returns>
public async Task<bool> UnloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
{
var dataOptional = await Task.Run(() =>
{
lock (_lockObject)
{
return _loadedPlugins.TryGetValue(pluginId, out var data) ? (Plugin: data.Plugin, Context: data.Context, Lifecycles: data.Lifecycles) : ((Plugin, AssemblyLoadContext, List<global::PluginEngine.Execution.IPluginLifecycle>)?)(null);
}
});

if (dataOptional == null)
return false;

var (plugin, context, lifecycles) = dataOptional.Value;
plugin.Status = PluginStatus.Unloading;

try
{
foreach (var lifecycle in lifecycles)
{
await lifecycle.OnBeforeUnloadAsync(cancellationToken);
}

// Remove from dictionary first to prevent any new references to lifecycle instances
await Task.Run(() =>
{
lock (_lockObject)
{
_loadedPlugins.Remove(pluginId);
}

// Clear lifecycle references to remove strong references to plugin types
lifecycles.Clear();

// Dispose hot reload callbacks before unloading context
if (_serviceProvider != null)
{
var hotReloader = _serviceProvider.GetService(typeof(IHotReloadService)) as IHotReloadService;
hotReloader?.RemoveCallbacksForContext(context);
}

// Unload the AssemblyLoadContext to allow memory reclamation
// Note: This must be done after clearing lifecycle references and removing from dictionary
context.Unload();

GC.Collect();
GC.WaitForPendingFinalizers();
});

foreach (var lifecycle in lifecycles)
{
await lifecycle.OnAfterUnloadAsync(cancellationToken);
}

plugin.Status = PluginStatus.Unloaded;
return true;
}
catch
{
plugin.Status = PluginStatus.Failed;
return false;
}
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

private async Task<Plugin> CreatePluginFromAssemblyAsync(string fullPath, string assemblyPath)
{
var assemblyName = AssemblyName.GetAssemblyName(fullPath);
var assemblyNameStr = assemblyName.Name ?? "Unknown";
var versionStr = assemblyName.Version?.ToString() ?? "1.0.0";

var plugin = new Plugin
{
Id = Guid.NewGuid(),
Name = assemblyNameStr,
AssemblyPath = assemblyPath,
LoadContextId = PluginEngineConstants.LoadContextPrefix + Guid.NewGuid(),
Status = PluginStatus.Loading,
Version = versionStr
};

await LoadPluginMetadataAsync(plugin, fullPath);

return plugin;
}

private async Task LoadPluginMetadataAsync(Plugin plugin, string fullPath)
{
var directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
var pluginJsonPath = Path.Combine(directory, "plugin.json");

if (!File.Exists(pluginJsonPath))
{
pluginJsonPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(fullPath) + PluginEngineConstants.MetadataFileExtension);
}

if (File.Exists(pluginJsonPath))
{
try
{
var json = await File.ReadAllTextAsync(pluginJsonPath);
plugin.Metadata = System.Text.Json.JsonSerializer.Deserialize<PluginMetadata>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (plugin.Metadata != null && !string.IsNullOrWhiteSpace(plugin.Metadata.EngineVersionConstraint))
{
await ValidateEngineVersionConstraintAsync(plugin, pluginJsonPath);
}
}
catch (System.Text.Json.JsonException ex)
{
_logger?.LogWarning(ex, "Invalid plugin metadata JSON, ignoring: {MetadataPath}", pluginJsonPath);
}
}
}

private async Task ValidateEngineVersionConstraintAsync(Plugin plugin, string pluginJsonPath)
{
var versioningService = _serviceProvider?.GetService(typeof(IVersioningService)) as IVersioningService;
if (versioningService != null)
{
var hostVersion = typeof(PluginLoaderService).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
if (!versioningService.IsSatisfiedBy(plugin.Metadata!.EngineVersionConstraint, hostVersion))
{
throw new PluginIncompatibleException(plugin.Name, plugin.Metadata.EngineVersionConstraint, hostVersion);
}
}
}

private async Task<List<IPluginLifecycle>> DiscoverPluginLifecyclesAsync(Assembly assembly)
{
var lifecycles = new List<IPluginLifecycle>();

foreach (var type in assembly.GetTypes())
{
if (typeof(IPluginLifecycle).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
{
if (Activator.CreateInstance(type) is IPluginLifecycle instance)
{
lifecycles.Add(instance);
}
}
}

return lifecycles;
}

private async Task InvokeLifecycleMethodsAsync(List<IPluginLifecycle> lifecycles, Func<IPluginLifecycle, Task> lifecycleAction)
{
foreach (var lifecycle in lifecycles)
{
await lifecycleAction(lifecycle);
}
}
}
