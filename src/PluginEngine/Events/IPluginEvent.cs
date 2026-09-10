#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Events;

/// <summary>
/// Base interface for all plugin events in the system.
/// Supports event sourcing and event-driven architecture.
/// </summary>
public interface IPluginEvent
{
    /// <summary>
    /// Gets the event ID for tracking.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the time the event occurred.
    /// </summary>
    DateTime OccurredAtUtc { get; }

    /// <summary>
    /// Gets the plugin associated with the event.
    /// </summary>
    Guid PluginId { get; }

    /// <summary>
    /// Gets the event type name.
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Handler interface for processing specific event types.
/// </summary>
public interface IPluginEventHandler<T> where T : IPluginEvent
{
    /// <summary>
    /// Handles an event.
    /// </summary>
    Task HandleAsync(T @event);
}

/// <summary>
/// Publisher for raising plugin events throughout the system.
/// </summary>
public interface IPluginEventPublisher
{
    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    Task PublishAsync<T>(T @event) where T : IPluginEvent;

    /// <summary>
    /// Gets publisher statistics for monitoring.
    /// </summary>
    EventPublisherStatistics GetStatistics();

    /// <summary>
    /// Removes all subscribers belonging to the specified AssemblyLoadContext.
    /// </summary>
    void RemoveSubscribersForContext(System.Runtime.Loader.AssemblyLoadContext context);
}

/// <summary>
/// Subscriber for registering interest in specific event types.
/// </summary>
public interface IPluginEventSubscriber
{
    /// <summary>
    /// Subscribes to an event type with a handler callback.
    /// </summary>
    void Subscribe<T>(Func<T, Task> handler) where T : IPluginEvent;

    /// <summary>
    /// Unsubscribes a handler from an event type.
    /// </summary>
    void Unsubscribe<T>(Func<T, Task> handler) where T : IPluginEvent;
}

/// <summary>
/// Base class for plugin events with common properties.
/// </summary>
public abstract class PluginEventBase : IPluginEvent
{
    /// <summary>
    /// Gets the event ID for tracking.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();
    /// <summary>
    /// Gets the time the event occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    /// <summary>
    /// Gets the plugin associated with the event.
    /// </summary>
    public required Guid PluginId { get; set; }
    public abstract string EventType { get; }
}

/// <summary>
/// Event raised when a plugin is loaded.
/// </summary>
public sealed class PluginLoadedEvent : PluginEventBase
{
    /// <summary>
    /// Gets the event type name.
    /// </summary>
    public override string EventType => "PluginLoaded";
    /// <summary>
    /// Gets or sets the name of the plugin.
    /// </summary>
    public required string PluginName { get; set; }
    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public required string Version { get; set; }
    /// <summary>
    /// Gets or sets the load time in milliseconds.
    /// </summary>
    public long LoadTimeMs { get; set; }
}

/// <summary>
/// Event raised when a plugin is unloaded.
/// </summary>
public sealed class PluginUnloadedEvent : PluginEventBase
{
    /// <summary>
    /// Gets the event type name.
    /// </summary>
    public override string EventType => "PluginUnloaded";
    /// <summary>
    /// Gets or sets the name of the plugin.
    /// </summary>
    public required string PluginName { get; set; }
    /// <summary>
    /// Gets or sets the reason for unloading the plugin.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Event raised when a plugin is updated.
/// </summary>
public sealed class PluginUpdatedEvent : PluginEventBase
{
    /// <summary>
    /// Gets the event type name.
    /// </summary>
    public override string EventType => "PluginUpdated";
    /// <summary>
    /// Gets or sets the previous version of the plugin.
    /// </summary>
    public required string PreviousVersion { get; set; }
    /// <summary>
    /// Gets or sets the new version of the plugin.
    /// </summary>
    public required string NewVersion { get; set; }
    /// <summary>
    /// Gets or sets a summary of the changes made.
    /// </summary>
    public required string ChangesSummary { get; set; }
}

/// <summary>
/// Event raised when a plugin encounters an error.
/// </summary>
public sealed class PluginErrorEvent : PluginEventBase
{
    /// <summary>
    /// Gets the event type name.
    /// </summary>
    public override string EventType => "PluginError";
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public required string ErrorMessage { get; set; }
    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    public string? ErrorDetails { get; set; }
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public int ErrorCode { get; set; }
}

/// <summary>
/// Event raised when dependencies are resolved.
/// </summary>
public sealed class DependenciesResolvedEvent : PluginEventBase
{
    /// <summary>
    /// Gets the event type name.
    /// </summary>
    public override string EventType => "DependenciesResolved";
    /// <summary>
    /// Gets or sets the list of resolved dependency IDs.
    /// </summary>
    public List<Guid> ResolvedDependencies { get; set; } = [];
    /// <summary>
    /// Gets or sets the resolution time in milliseconds.
    /// </summary>
    public long ResolutionTimeMs { get; set; }
}
