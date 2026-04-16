#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;

namespace PluginEngine.Services.Abstractions;

/// <summary>
/// Service for zero-downtime plugin hot-swapping: replaces a running plugin with a
/// new assembly version, preserving host-application availability throughout the swap.
/// Supports rollback to the previous assembly if the new version fails to load.
/// </summary>
public interface IHotSwapService
{
    /// <summary>
    /// Swaps the assembly of a running plugin with the one at <paramref name="newAssemblyPath"/>.
    /// The old plugin is unloaded, the new assembly is loaded in a fresh
    /// <see cref="System.Runtime.Loader.AssemblyLoadContext"/>, and all registered
    /// post-swap callbacks are invoked.  On failure the previous assembly is reloaded
    /// automatically (best-effort rollback).
    /// </summary>
    /// <param name="pluginId">Identifier of the plugin to swap.</param>
    /// <param name="newAssemblyPath">File-system path of the replacement assembly.</param>
    Task<PluginOperationResult> SwapPluginAsync(
        Guid pluginId, string newAssemblyPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the most recent swap for the specified plugin, reloading the assembly
    /// that was active immediately before the last successful swap.
    /// </summary>
    /// <param name="pluginId">Identifier of the plugin to roll back.</param>
    Task<PluginOperationResult> RollbackSwapAsync(
        Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the most recent <see cref="SwapRecord"/> for <paramref name="pluginId"/>,
    /// or <c>null</c> if the plugin has never been swapped.
    /// </summary>
    Task<PluginOperationResult<SwapRecord?>> GetLastSwapRecordAsync(
        Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full swap history for <paramref name="pluginId"/>, ordered oldest-first.
    /// </summary>
    Task<PluginOperationResult<List<SwapRecord>>> GetSwapHistoryAsync(
        Guid pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a callback to be invoked after every successful swap of
    /// <paramref name="pluginId"/>.  Only one callback is stored per plugin;
    /// re-registering replaces the previous callback.
    /// </summary>
    void RegisterPostSwapCallback(Guid pluginId, Func<Plugin, Task> callback);

    /// <summary>
    /// Removes the post-swap callback registered for <paramref name="pluginId"/>.
    /// </summary>
    void UnregisterPostSwapCallback(Guid pluginId);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="plugin"/> can be hot-swapped:
    /// it must be in a loaded/active state and its assembly file must be accessible.
    /// </summary>
    bool CanSwap(Plugin plugin);
}

/// <summary>
/// Immutable record of a single hot-swap operation.
/// </summary>
public sealed class SwapRecord
{
    /// <summary>Gets the plugin that was swapped.</summary>
    public Guid PluginId { get; init; }

    /// <summary>Gets the assembly path that was active before the swap.</summary>
    public string PreviousAssemblyPath { get; init; } = string.Empty;

    /// <summary>Gets the assembly path loaded by this swap.</summary>
    public string NewAssemblyPath { get; init; } = string.Empty;

    /// <summary>Gets whether the swap completed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the UTC time when the swap was attempted.</summary>
    public DateTime SwappedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets how long the swap took end-to-end.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Gets the error message if the swap failed; otherwise <c>null</c>.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets whether this swap has been rolled back.</summary>
    public bool RolledBack { get; set; }
}
