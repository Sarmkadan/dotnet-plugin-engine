#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PluginEngine.Execution;

/// <summary>
/// Interface that plugins can implement to receive lifecycle notifications.
/// Allows plugins to perform setup and teardown logic at specific lifecycle boundaries.
/// </summary>
public interface IPluginLifecycle
{
    /// <summary>
    /// Called before the plugin is fully loaded and initialized.
    /// </summary>
    Task OnBeforeLoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called after the plugin is fully loaded and initialized.
    /// </summary>
    Task OnAfterLoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called before the plugin begins unloading.
    /// </summary>
    Task OnBeforeUnloadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called after the plugin has been unloaded.
    /// </summary>
    Task OnAfterUnloadAsync(CancellationToken cancellationToken = default);
}
