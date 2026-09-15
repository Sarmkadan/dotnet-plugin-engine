# HotReloadModels

Data models representing hot reload statistics, events, and per-plugin status, produced by `HotReloadService` and exposed through `IHotReloadService`.

## HotReloadStatistics

Represents aggregate hot reload statistics.

| Property | Type | Description |
| --- | --- | --- |
| `TotalReloads` | `int` | The total number of reloads. |
| `SuccessfulReloads` | `int` | The number of successful reloads. |
| `FailedReloads` | `int` | The number of failed reloads. |
| `LastReloadTime` | `DateTime?` | The time of the last reload. |
| `AverageReloadTime` | `TimeSpan` | The average reload time. |
| `RecentEvents` | `List<HotReloadEvent>` | The list of recent hot reload events. |

## HotReloadEvent

Represents a single hot reload event.

| Property | Type | Description |
| --- | --- | --- |
| `PluginId` | `Guid` | The plugin identifier. |
| `Timestamp` | `DateTime` | The timestamp of the event. |
| `Success` | `bool` | A value indicating whether the reload was successful. |
| `ErrorMessage` | `string?` | The error message if the reload failed. |
| `Duration` | `TimeSpan` | The duration of the reload operation. |

## HotReloadStatus

Represents the hot reload status of a plugin.

| Property | Type | Description |
| --- | --- | --- |
| `PluginId` | `Guid` | The plugin identifier. |
| `SupportsHotReload` | `bool` | A value indicating whether the plugin supports hot reload. |
| `LastReloadTime` | `DateTime?` | The time of the last reload. |
| `ReloadCount` | `int` | The number of times the plugin has been reloaded. |
| `LastError` | `string?` | The last error message encountered during reload. |

## How HotReloadService populates them

`HotReloadService` maintains an in-memory list of recent events and a concurrent map of per-plugin statuses.

- **Event retention**: `RecordHotReloadEvent` appends each event to `_recentEvents` and trims the oldest entry once the list exceeds `MaxRecentEvents` (100). Only the most recent 100 events are ever retained.
- **Statistics**: `GetStatisticsAsync` derives `TotalReloads`, `SuccessfulReloads`, and `FailedReloads` by counting over `_recentEvents`; `LastReloadTime` is the timestamp of the most recent event; `AverageReloadTime` is the mean duration across all retained events (only set when at least one event exists). `RecentEvents` is the last `RecentEventsInStatistics` (10) events.
- **Status updates**: `UpdateHotReloadStatus` upserts a `HotReloadStatus` per plugin. On first reload it creates a new entry with `SupportsHotReload = true`, `ReloadCount = 1`, and the current UTC time; on subsequent reloads it increments `ReloadCount` and refreshes `LastReloadTime`. `LastError` is set on failure and only overwritten by later failures, never cleared on success.
- **Event recording**: Both success and failure paths in `HotReloadPluginAsync` record a `HotReloadEvent` (with `Duration` measured via a `Stopwatch`) and update the plugin's status before returning.

## Usage