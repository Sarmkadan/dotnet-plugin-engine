#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Orchestrates plugin lifecycle transitions (load, activate, deactivate, unload),
/// invoking <see cref="IPluginLifecycle"/> hooks on the plugin when implemented,
/// and maintaining consistent state in the <see cref="PluginRegistry"/>.
/// </summary>
public sealed class LifecycleManager
{
    private readonly ILogger<LifecycleManager> _logger;
    private readonly PluginRegistry _registry;
    private readonly PluginLoader _loader;
    private readonly SandboxRunner _sandbox;

    /// <summary>
    /// Initializes a new instance of the <see cref="LifecycleManager"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="registry">The plugin registry.</param>
    /// <param name="loader">The low-level assembly loader.</param>
    /// <param name="sandbox">The sandbox runner for executing plugin hooks safely.</param>
    public LifecycleManager(
        ILogger<LifecycleManager> logger,
        PluginRegistry registry,
        PluginLoader loader,
        SandboxRunner sandbox)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(sandbox);
        _logger = logger;
        _registry = registry;
        _loader = loader;
        _sandbox = sandbox;
    }

    /// <summary>
    /// Transitions a plugin through the full load sequence: loads the assembly,
    /// invokes <see cref="IPluginLifecycle.OnBeforeLoadAsync"/> and
    /// <see cref="IPluginLifecycle.OnAfterLoadAsync"/>, and registers it.
    /// </summary>
    /// <param name="plugin">The plugin entity to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the plugin was loaded successfully.</returns>
    public async Task<bool> LoadPluginAsync(Plugin plugin, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _logger.LogInformation("Loading plugin {Name} ({Id}).", plugin.Name, plugin.Id);

        plugin.Status = PluginStatus.Loading;
        var lifecycle = FindLifecycleHook(plugin.Id);

        if (lifecycle is not null)
        {
            var hookResult = await _sandbox.ExecuteAsync(plugin.Id,
                ct => lifecycle.OnBeforeLoadAsync(ct), cancellationToken);
            if (hookResult.Outcome != SandboxOutcome.Success)
            {
                plugin.Status = PluginStatus.Failed;
                _logger.LogError("OnBeforeLoad hook failed for {Name}.", plugin.Name);
                return false;
            }
        }

        try
        {
            _loader.LoadAssembly(plugin.Id, plugin.AssemblyPath);
        }
        catch (PluginLoadException ex)
        {
            plugin.Status = PluginStatus.Failed;
            _logger.LogError(ex, "Assembly load failed for {Name}.", plugin.Name);
            return false;
        }

        if (lifecycle is not null)
            await _sandbox.ExecuteAsync(plugin.Id, ct => lifecycle.OnAfterLoadAsync(ct), cancellationToken);

        plugin.Status = PluginStatus.Loaded;
        plugin.ModifiedAt = DateTime.UtcNow;
        _registry.Register(plugin);
        _logger.LogInformation("Plugin {Name} loaded.", plugin.Name);
        return true;
    }

    /// <summary>
    /// Activates a loaded plugin, moving it to <see cref="PluginStatus.Active"/>.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if activation succeeded.</returns>
    public Task<bool> ActivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var plugin = _registry.GetById(pluginId);
        if (plugin is null)
        {
            _logger.LogWarning("Cannot activate unknown plugin {Id}.", pluginId);
            return Task.FromResult(false);
        }

        if (plugin.Status != PluginStatus.Loaded && plugin.Status != PluginStatus.Inactive)
        {
            _logger.LogWarning("Plugin {Name} is in state {Status} and cannot be activated.", plugin.Name, plugin.Status);
            return Task.FromResult(false);
        }

        plugin.Status = PluginStatus.Active;
        plugin.ModifiedAt = DateTime.UtcNow;
        _logger.LogInformation("Plugin {Name} activated.", plugin.Name);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Deactivates an active plugin, moving it to <see cref="PluginStatus.Inactive"/>.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if deactivation succeeded.</returns>
    public Task<bool> DeactivatePluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var plugin = _registry.GetById(pluginId);
        if (plugin is null || plugin.Status != PluginStatus.Active)
        {
            _logger.LogWarning("Cannot deactivate plugin {Id}: not active or not found.", pluginId);
            return Task.FromResult(false);
        }

        plugin.Status = PluginStatus.Inactive;
        plugin.ModifiedAt = DateTime.UtcNow;
        _logger.LogInformation("Plugin {Name} deactivated.", plugin.Name);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Unloads a plugin: invokes lifecycle hooks, unloads the assembly context,
    /// and removes the plugin from the registry.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the plugin was unloaded successfully.</returns>
    public async Task<bool> UnloadPluginAsync(Guid pluginId, CancellationToken cancellationToken = default)
    {
        var plugin = _registry.GetById(pluginId);
        if (plugin is null)
        {
            _logger.LogWarning("Cannot unload unknown plugin {Id}.", pluginId);
            return false;
        }

        _logger.LogInformation("Unloading plugin {Name}.", plugin.Name);
        plugin.Status = PluginStatus.Unloading;

        var lifecycle = FindLifecycleHook(pluginId);
        if (lifecycle is not null)
        {
            await _sandbox.ExecuteAsync(pluginId, ct => lifecycle.OnBeforeUnloadAsync(ct), cancellationToken);
        }

        _loader.UnloadAssembly(pluginId);

        if (lifecycle is not null)
        {
            await _sandbox.ExecuteAsync(pluginId, ct => lifecycle.OnAfterUnloadAsync(ct), cancellationToken);
        }

        plugin.Status = PluginStatus.Unloaded;
        _registry.Unregister(pluginId);
        _logger.LogInformation("Plugin {Name} unloaded.", plugin.Name);
        return true;
    }

    private IPluginLifecycle? FindLifecycleHook(Guid pluginId)
    {
        var assembly = _loader.GetLoadedAssembly(pluginId);
        if (assembly is null) return null;

        var lifecycleType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IPluginLifecycle).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        if (lifecycleType is null) return null;

        try
        {
            return Activator.CreateInstance(lifecycleType) as IPluginLifecycle;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not instantiate IPluginLifecycle from plugin {Id}.", pluginId);
            return null;
        }
    }
}
