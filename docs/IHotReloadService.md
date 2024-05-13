# IHotReloadService

The `IHotReloadService` interface provides a contract for monitoring and managing the state of hot reloading operations within the `dotnet-plugin-engine`. It exposes statistical data regarding reload attempts, including success rates, timing metrics, and error diagnostics, while also defining the structural schema for individual reload events. This interface serves as the primary source of truth for observing the lifecycle and health of plugin updates without requiring direct interaction with the underlying reload mechanisms.

## API

The following members are exposed by the `IHotReloadService` interface or its associated event structures:

### `TotalReloads`
*   **Type:** `int`
*   **Purpose:** Retrieves the cumulative count of all reload attempts initiated since the service initialization.
*   **Parameters:** None.
*   **Return Value:** An integer representing the total number of attempts.
*   **Throws:** Never.

### `SuccessfulReloads`
*   **Type:** `int`
*   **Purpose:** Retrieves the count of reload attempts that completed successfully without errors.
*   **Parameters:** None.
*   **Return Value:** An integer representing the number of successful operations.
*   **Throws:** Never.

### `FailedReloads`
*   **Type:** `int`
*   **Purpose:** Retrieves the count of reload attempts that resulted in a failure.
*   **Parameters:** None.
*   **Return Value:** An integer representing the number of failed operations.
*   **Throws:** Never.

### `LastReloadTime`
*   **Type:** `DateTime?`
*   **Purpose:** Indicates the timestamp of the most recent reload attempt, regardless of success or failure. Returns `null` if no attempts have been made.
*   **Parameters:** None.
*   **Return Value:** A nullable `DateTime` structure.
*   **Throws:** Never.

### `AverageReloadTime`
*   **Type:** `TimeSpan`
*   **Purpose:** Provides the arithmetic mean duration of all completed reload operations.
*   **Parameters:** None.
*   **Return Value:** A `TimeSpan` representing the average execution time.
*   **Throws:** Never.

### `RecentEvents`
*   **Type:** `List<HotReloadEvent>`
*   **Purpose:** Returns a collection of the most recent hot reload events, containing detailed diagnostics for each operation.
*   **Parameters:** None.
*   **Return Value:** A list of `HotReloadEvent` objects.
*   **Throws:** Never. Note: The list contents may be modified by the service; consumers should treat the list as read-only or create a copy if long-term storage is required.

### `SupportsHotReload`
*   **Type:** `bool`
*   **Purpose:** Determines whether the current plugin or environment configuration supports hot reloading capabilities.
*   **Parameters:** None.
*   **Return Value:** `true` if hot reloading is supported; otherwise, `false`.
*   **Throws:** Never.

### `ReloadCount`
*   **Type:** `int`
*   **Purpose:** An alternative counter representing the number of reloads performed, typically scoped to a specific plugin instance or session context.
*   **Parameters:** None.
*   **Return Value:** An integer count.
*   **Throws:** Never.

### `LastError`
*   **Type:** `string?`
*   **Purpose:** Contains the error message associated with the most recent failed reload attempt. Returns `null` if the last attempt was successful or if no attempts have occurred.
*   **Parameters:** None.
*   **Return Value:** A string describing the error, or `null`.
*   **Throws:** Never.

### HotReloadEvent Members
The `RecentEvents` list contains objects of type `HotReloadEvent`, which expose the following properties:

#### `PluginId` (Event)
*   **Type:** `Guid`
*   **Purpose:** Uniquely identifies the plugin associated with this specific reload event.
*   **Parameters:** None.
*   **Return Value:** The GUID of the plugin.
*   **Throws:** Never.

#### `Timestamp`
*   **Type:** `DateTime`
*   **Purpose:** Records the exact time the reload event occurred.
*   **Parameters:** None.
*   **Return Value:** The `DateTime` of the event.
*   **Throws:** Never.

#### `Success`
*   **Type:** `bool`
*   **Purpose:** Indicates whether this specific reload event completed successfully.
*   **Parameters:** None.
*   **Return Value:** `true` for success, `false` for failure.
*   **Throws:** Never.

#### `ErrorMessage`
*   **Type:** `string?`
*   **Purpose:** Provides the detailed exception message or error description if the event failed.
*   **Parameters:** None.
*   **Return Value:** The error string, or `null` if successful.
*   **Throws:** Never.

#### `Duration`
*   **Type:** `TimeSpan`
*   **Purpose:** Measures the time elapsed during this specific reload operation.
*   **Parameters:** None.
*   **Return Value:** The `TimeSpan` duration.
*   **Throws:** Never.

#### `PluginId` (Service Context)
*   **Type:** `Guid`
*   **Purpose:** When exposed directly on the service context, this identifies the primary plugin instance being monitored by this specific service implementation.
*   **Parameters:** None.
*   **Return Value:** The GUID of the plugin.
*   **Throws:** Never.

## Usage

### Example 1: Monitoring Reload Health
The following example demonstrates how to check the overall health of the hot reload system by analyzing success rates and retrieving the last error message if a failure occurred.

```csharp
public void CheckReloadHealth(IHotReloadService service)
{
    if (!service.SupportsHotReload)
    {
        Console.WriteLine("Hot reloading is not supported in this environment.");
        return;
    }

    Console.WriteLine($"Total Attempts: {service.TotalReloads}");
    Console.WriteLine($"Success Rate: {(service.TotalReloads > 0 ? (double)service.SuccessfulReloads / service.TotalReloads * 100 : 0):F2}%");
    Console.WriteLine($"Average Duration: {service.AverageReloadTime.TotalMilliseconds:F2}ms");

    if (service.FailedReloads > 0 && service.LastError != null)
    {
        Console.WriteLine($"Last Failure Reason: {service.LastError}");
        Console.WriteLine($"Last Failure Time: {service.LastReloadTime}");
    }
}
```

### Example 2: Auditing Recent Events
This example iterates through the `RecentEvents` list to audit the last five operations, extracting detailed timing and error information for logging purposes.

```csharp
public void AuditRecentEvents(IHotReloadService service)
{
    var events = service.RecentEvents;
    if (events == null || events.Count == 0)
    {
        Console.WriteLine("No recent events recorded.");
        return;
    }

    // Take the last 5 events
    var recent = events.TakeLast(5).ToList();

    foreach (var evt in recent)
    {
        string status = evt.Success ? "SUCCESS" : "FAILED";
        Console.WriteLine($"[{evt.Timestamp:HH:mm:ss}] Plugin {evt.PluginId}: {status}");
        
        if (!evt.Success)
        {
            Console.WriteLine($"  Error: {evt.ErrorMessage}");
        }
        
        Console.WriteLine($"  Duration: {evt.Duration.TotalMilliseconds}ms");
    }
}
```

## Notes

*   **Thread Safety:** The properties exposed by `IHotReloadService` are intended for read-only consumption by external callers. However, as the underlying service updates counters (`TotalReloads`, `SuccessfulReloads`, etc.) and appends to `RecentEvents` concurrently during reload operations, consumers should expect values to change between reads. The `RecentEvents` list itself may be thread-safe for reading, but enumerating it while the service is actively adding new events may result in a collection modification exception depending on the concrete implementation; creating a snapshot (e.g., `.ToList()`) is recommended before iteration.
*   **Data Consistency:** The `AverageReloadTime` is calculated based on completed operations. If `TotalReloads` is zero, the behavior of `AverageReloadTime` depends on the implementation, but typically returns `TimeSpan.Zero`.
*   **Duplicate Identifiers:** The interface definition includes `PluginId` in multiple contexts (both as a direct property and within `HotReloadEvent`). The direct property likely refers to the service's primary subject, whereas the event property allows the service to track multiple plugins if aggregated, or simply redundantly tags the event for clarity.
*   **Nullable Handling:** Properties such as `LastReloadTime`, `ErrorMessage`, and `LastError` are nullable. Consumers must perform null checks before accessing members of these values to prevent `NullReferenceException`, particularly when the service has just started and no reloads have occurred yet.
*   **Event Retention:** The `RecentEvents` list likely has a capped size to prevent memory leaks in long-running applications. Older events may be automatically pruned as new ones are added.
