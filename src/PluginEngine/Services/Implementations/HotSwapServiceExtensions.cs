#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using PluginEngine.Domain.Entities;
using PluginEngine.Results;

namespace PluginEngine.Services.Implementations;

/// <summary>
/// Extension methods for <see cref="HotSwapService"/> that provide convenient
/// operations for plugin hot-swapping scenarios.
/// </summary>
/// <remarks>
/// All methods delegate to the corresponding instance methods on <see cref="HotSwapService"/>.
/// These extensions provide a fluent API for common hot-swap operations.
/// </remarks>
public static class HotSwapServiceExtensions
{
    /// <summary>
    /// Checks if a plugin can be swapped based on its current status.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="plugin">The plugin to check. Can be null.</param>
    /// <returns>True if the plugin can be swapped; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static bool CanSwap(this HotSwapService service, Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.CanSwap(plugin);
    }

    /// <summary>
    /// Gets the last swap record for a plugin, or null if no swaps have occurred.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the operation result with the last swap record or null if none exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<PluginOperationResult<SwapRecord?>> GetLastSwapRecordAsync(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.GetLastSwapRecordAsync(pluginId);
    }

    /// <summary>
    /// Gets the swap history for a plugin.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the operation result with the list of swap records.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<PluginOperationResult<List<SwapRecord>>> GetSwapHistoryAsync(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.GetSwapHistoryAsync(pluginId);
    }

    /// <summary>
    /// Registers a callback to be invoked after every successful swap of a plugin.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="callback">The callback to register. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="callback"/> is null.</exception>
    public static void RegisterPostSwapCallback(
        this HotSwapService service,
        Guid pluginId,
        Func<Plugin, Task> callback)
    {
        ArgumentNullException.ThrowIfNull(service);
        service.RegisterPostSwapCallback(pluginId, callback);
    }

    /// <summary>
    /// Unregisters the post-swap callback for a plugin.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static void UnregisterPostSwapCallback(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        service.UnregisterPostSwapCallback(pluginId);
    }

    /// <summary>
    /// Rolls back the most recent swap for a plugin.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the operation result indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<PluginOperationResult> RollbackSwapAsync(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.RollbackSwapAsync(pluginId);
    }

    /// <summary>
    /// Swaps the assembly of a running plugin with a new assembly.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="newAssemblyPath">The file-system path of the replacement assembly. Cannot be null or whitespace.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the operation result indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="newAssemblyPath"/> is null or whitespace.</exception>
    public static async Task<PluginOperationResult> SwapPluginAsync(
        this HotSwapService service,
        Guid pluginId,
        string newAssemblyPath)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.SwapPluginAsync(pluginId, newAssemblyPath);
    }

    /// <summary>
    /// Swaps the assembly of a running plugin with a new assembly, with cancellation support.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="newAssemblyPath">The file-system path of the replacement assembly. Cannot be null or whitespace.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the operation result indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="newAssemblyPath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="newAssemblyPath"/> is null or whitespace.</exception>
    public static async Task<PluginOperationResult> SwapPluginAsync(
        this HotSwapService service,
        Guid pluginId,
        string newAssemblyPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.SwapPluginAsync(pluginId, newAssemblyPath, cancellationToken);
    }

    /// <summary>
    /// Checks if any swap records exist for a plugin.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// true if swap history exists; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<bool> HasSwapHistoryAsync(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        var result = await service.GetLastSwapRecordAsync(pluginId);
        return result.Success && result.Data != null;
    }

    /// <summary>
    /// Gets the count of swap records for a plugin.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the count of swap records.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<int> GetSwapHistoryCountAsync(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        var result = await service.GetSwapHistoryAsync(pluginId);
        return result.Success ? result.Data?.Count ?? 0 : 0;
    }

    /// <summary>
    /// Gets the most recent successful swap record for a plugin.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the operation result with the most recent successful swap record or null if none exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<PluginOperationResult<SwapRecord?>> GetLastSuccessfulSwapAsync(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        var result = await service.GetSwapHistoryAsync(pluginId);

        if (!result.Success || result.Data == null)
        {
            return PluginOperationResult<SwapRecord?>.CreateFailure(
                "No swap history found.",
                result.ErrorCode ?? 404,
                result.ErrorDetails);
        }

        var lastSuccessful = result.Data.LastOrDefault(r => r.Success && !r.RolledBack);
        return PluginOperationResult<SwapRecord?>.CreateSuccess(
            lastSuccessful,
            lastSuccessful == null ? "No successful swap found." : "Last successful swap retrieved.");
    }

    /// <summary>
    /// Checks if the most recent swap for a plugin was successful.
    /// </summary>
    /// <param name="service">The hot swap service instance. Cannot be null.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// true if the most recent swap was successful; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<bool> IsLastSwapSuccessfulAsync(
        this HotSwapService service,
        Guid pluginId)
    {
        ArgumentNullException.ThrowIfNull(service);
        var result = await service.GetLastSwapRecordAsync(pluginId);
        return result.Success && result.Data != null && result.Data.Success && !result.Data.RolledBack;
    }
}