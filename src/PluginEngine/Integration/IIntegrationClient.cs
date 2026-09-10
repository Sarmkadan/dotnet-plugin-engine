#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Defines the contract for external plugin integration and remote operations.
/// Enables plugins to communicate with external systems and registries.
/// </summary>
public interface IIntegrationClient
{
    /// <summary>
    /// Gets the integration provider name.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the integration is available and configured.
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Sends a notification about a plugin event.
    /// </summary>
    Task SendNotificationAsync(string eventType, PluginNotification notification);

    /// <summary>
    /// Retrieves plugin information from an external source.
    /// </summary>
    Task<PluginInfo?> GetPluginInfoAsync(Guid pluginId);
}

/// <summary>
/// Represents a plugin notification sent to external systems.
/// </summary>
public sealed class PluginNotification
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public required Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin.
    /// </summary>
    public required string PluginName { get; set; }

    /// <summary>
    /// Gets or sets the type of the event that triggered the notification.
    /// </summary>
    public required string EventType { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when the event occurred.
    /// </summary>
    public required DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Gets or sets additional metadata associated with the notification.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];
}

/// <summary>
/// Basic plugin information structure for integration.
/// </summary>
public sealed class PluginInfo
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the author of the plugin.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the download URL for the plugin.
    /// </summary>
    public string? DownloadUrl { get; set; }
}