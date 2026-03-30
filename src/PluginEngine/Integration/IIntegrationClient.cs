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
    public required Guid PluginId { get; set; }
    public required string PluginName { get; set; }
    public required string EventType { get; set; }
    public required DateTime OccurredAtUtc { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = [];
}

/// <summary>
/// Basic plugin information structure for integration.
/// </summary>
public sealed class PluginInfo
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? DownloadUrl { get; set; }
}
