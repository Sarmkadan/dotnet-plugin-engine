# Plugin Event Types

This document provides an overview of the core event types used by the Plugin Engine, the base event class, and the interfaces for handling and subscribing to events.  
The documentation follows the same style as the existing `docs/StringExtensions.md`.

---

## Core Event Interface

| Property | Type | Description |
|----------|------|-------------|
| `EventId` | `Guid` | Unique identifier for the event instance. |
| `OccurredAtUtc` | `DateTime` | UTC timestamp when the event was created. |
| `PluginId` | `Guid` | Identifier of the plugin that raised the event. |
| `EventType` | `string` | Human‑readable name of the event type. |

All concrete events inherit from `PluginEventBase`, which implements the above properties.

---

## Plugin Event Types

| Event | Properties | Description |
|-------|------------|-------------|
| **`PluginLoadedEvent`** | `PluginName` (`string`), `Version` (`string`), `LoadTimeMs` (`long`) | Raised when a plugin is successfully loaded. |
| **`PluginUnloadedEvent`** | `PluginName` (`string`), `Reason` (`string?`) | Raised when a plugin is unloaded. |
| **`PluginUpdatedEvent`** | `PreviousVersion` (`string`), `NewVersion` (`string`), `ChangesSummary` (`string`) | Raised when a plugin is updated to a new version. |
| **`PluginErrorEvent`** | `ErrorMessage` (`string`), `ErrorDetails` (`string?`), `ErrorCode` (`int`) | Raised when an error occurs during plugin operation. |
| **`DependenciesResolvedEvent`** | `ResolvedDependencies` (`List<Guid>`), `ResolutionTimeMs` (`long`) | Raised after dependency resolution completes. |

---

## Plugin Event Type Enum

The engine exposes a `PluginEventType` enum that maps to the string names used in `EventType`.  
It is defined as follows:

