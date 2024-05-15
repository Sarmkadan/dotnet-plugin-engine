# WebhookConfiguration

The `WebhookConfiguration` type holds the settings that control how the plugin engine receives, validates, and processes incoming webhook requests. It enables toggling the webhook endpoint, defining security secrets, limiting payload size, configuring retry behavior, and filtering which event types are handled.

## API

### Enabled  
**Type:** `bool`  
**Purpose:** Gets or sets whether the webhook endpoint is active. When `false`, incoming requests are ignored regardless of other settings.  
**Parameters:** None.  
**Return Value:** Current enabled state.  
**Exceptions:** None.

### Secret  
**Type:** `string?`  
**Purpose:** Gets or sets the optional secret used to verify the `X-Signature` header of incoming webhook payloads. If `null` or empty, signature verification is skipped.  
**Parameters:** None.  
**Return Value:** The secret string or `null`.  
**Exceptions:** None.

### MaxPayloadSizeBytes  
**Type:** `int`  
**Purpose:** Gets or sets the maximum allowed size (in bytes) of a webhook request body. Requests exceeding this limit are rejected with HTTP 413.  
**Parameters:** None.  
**Return Value:** The configured limit.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if set to a value less than 1.

### EndpointPath  
**Type:** `string`  
**Purpose:** Gets or sets the relative URL path at which the webhook endpoint is hosted (e.g., `/api/webhooks/github`). Must be an absolute path starting with `/`.  
**Parameters:** None.  
**Return Value:** The configured path.  
**Exceptions:** Throws `ArgumentException` if the value is `null`, empty, or does not start with `/`.

### ProcessingTimeoutMs  
**Type:** `int`  
**Purpose:** Gets or sets the maximum time (in milliseconds) the engine will spend processing a single webhook request before aborting and returning HTTP 504.  
**Parameters:** None.  
**Return Value:** The timeout value.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if set to a value less than 1.

### RetryPolicy  
**Type:** `WebhookRetryPolicy`  
**Purpose:** Gets or sets the policy that determines how failed webhook deliveries are retried. The policy encapsulates retry count, delay strategy, and limits.  
**Parameters:** None.  
**Return Value:** The current retry policy instance.  
**Exceptions:** None.

### EventFilters  
**Type:** `List<string>`  
**Purpose:** Gets the list of event type strings that the webhook will accept. An empty list indicates that all events are accepted. The list is mutable; items can be added or removed after construction.  
**Parameters:** None.  
**Return Value:** The mutable list of filters.  
**Exceptions:** None.

### EnableDetailedLogging  
**Type:** `bool`  
**Purpose:** Gets or sets whether detailed diagnostic logging (including payloads and headers) is performed for each webhook request. Sensitive data such as the `Secret` is never logged.  
**Parameters:** None.  
**Return Value:** Current logging flag.  
**Exceptions:** None.

### IsValid  
**Type:** `bool` (read‑only)  
**Purpose:** Indicates whether the current configuration is syntactically and semantically correct. Returns `false` if `EndpointPath` is invalid, `MaxPayloadSizeBytes` or `ProcessingTimeoutMs` are non‑positive, or if any required fields are missing.  
**Parameters:** None.  
**Return Value:** `true` if the configuration can be used to start the webhook listener; otherwise `false`.  
**Exceptions:** None.

### MaxRetries  
**Type:** `int`  
**Purpose:** Gets or sets the maximum number of retry attempts for a failed webhook delivery. Used by the default retry policy when `RetryPolicy` is not explicitly set.  
**Parameters:** None.  
**Return Value:** The retry limit.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if set to a negative value.

### InitialDelayMs  
**Type:** `int`  
**Purpose:** Gets or sets the initial delay (in milliseconds) before the first retry attempt.  
**Parameters:** None.  
**Return Value:** The initial delay.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if set to a negative value.

### BackoffMultiplier  
**Type:** `double`  
**Purpose:** Gets or sets the factor by which the delay is multiplied after each unsuccessful retry attempt. A value of `1` results in constant delay; values > 1 produce exponential backoff.  
**Parameters:** None.  
**Return Value:** The multiplier.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if set to a value less than or equal to 0.

### MaxDelayMs  
**Type:** `int`  
**Purpose:** Gets or sets the upper bound (in milliseconds) for the delay between retry attempts. The calculated delay is capped at this value.  
**Parameters:** None.  
**Return Value:** The maximum delay.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if set to a value less than 0.

### GetDelayForAttempt  
**Signature:** `int GetDelayForAttempt(int attemptNumber)`  
**Purpose:** Calculates the delay (in milliseconds) to apply before the specified retry attempt based on `InitialDelayMs`, `BackoffMultiplier`, and `MaxDelayMs`. The `attemptNumber` is 1‑based (1 for the first retry).  
**Parameters:**  
- `attemptNumber`: The retry attempt for which the delay is required. Must be ≥ 1.  
**Return Value:** The delay in milliseconds to wait before performing the attempt.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if `attemptNumber` is less than 1.

### AddWebhookSupport  
**Signature:** `public static IServiceCollection AddWebhookSupport(this IServiceCollection services, Action<WebhookConfiguration>? configure = null)`  
**Purpose:** Extension method that registers the webhook middleware and a singleton `WebhookConfiguration` instance with the DI container. Optionally accepts a configuration delegate to set properties after registration.  
**Parameters:**  
- `services`: The service collection to modify.  
- `configure`: Optional delegate to further customize the configuration instance.  
**Return Value:** The same `IServiceCollection` instance for chaining.  
**Exceptions:** Throws `ArgumentNullException` if `services` is `null`. The method does not validate the configuration; validation occurs when the webhook middleware starts.

### WithEventFilters  
**Signature:** `public static WebhookConfiguration WithEventFilters(params string[] eventTypes)`  
**Purpose:** Factory method that creates a new `WebhookConfiguration` instance with the specified event types added to its `EventFilters` list. All other properties receive default values.  
**Parameters:**  
- `eventTypes`: One or more event type strings to filter on. Passing zero arguments results in an empty filter list (accept all).  
**Return Value:** A newly initialized `WebhookConfiguration` object.  
**Exceptions:** Throws `ArgumentNullException` if any element in `eventTypes` is `null`.

### WithAllEvents  
**Signature:** `public static WebhookConfiguration WithAllEvents()`  
**Purpose:** Factory method that creates a new `WebhookConfiguration` instance whose `EventFilters` list is empty, indicating that all incoming event types should be processed. Other properties are set to sensible defaults.  
**Parameters:** None.  
**Return Value:** A newly initialized `WebhookConfiguration` object.  
**Exceptions:** None.

### WithAggressiveRetry  
**Signature:** `public static WebhookConfiguration WithAggressiveRetry()`  
**Purpose:** Factory method that creates a new `WebhookConfiguration` instance with a retry policy tuned for rapid retries: low `InitialDelayMs`, high `BackoffMultiplier`, and limited `MaxDelayMs`. Other properties receive default values.  
**Parameters:** None.  
**Return Value:** A newly initialized `WebhookConfiguration` object.  
**Exceptions:** None.

## Usage

### Basic setup in an ASP.NET Core application
```csharp
using Microsoft.Extensions.DependencyInjection;
using DotnetPluginEngine.Webhooks;

var builder = WebApplication.CreateBuilder(args);

// Register webhook support with a custom configuration
builder.Services.AddWebhookSupport(cfg =>
{
    cfg.EndpointPath = "/api/webhooks";
    cfg.Secret = "super‑secret‑key";
    cfg.MaxPayloadSizeBytes = 1024 * 1024; // 1 MiB
    cfg.EnableDetailedLogging = true;
    cfg.WithEventFilters("issue.opened", "issue.closed");
});

var app = builder.Build();
app.UseWebhookMiddleware(); // assumes extension method added by AddWebhookSupport
app.Run();
```

### Using factory methods for ad‑hoc configuration
```csharp
using DotnetPluginEngine.Webhooks;

// Create a configuration that accepts all events and uses aggressive retry logic
var config = WebhookConfiguration.WithAllEvents()
                                 .WithAggressiveRetry();

// Adjust a few settings after creation
config.EndpointPath = "/hooks/github";
config.MaxRetries = 5;
config.InitialDelayMs = 100;
config.BackoffMultiplier = 2.0;
config.MaxDelayMs = 5000;

if (!config.IsValid)
{
    throw new InvalidOperationException("Webhook configuration is invalid.");
}

// Pass the configuration to a custom hosted service or middleware
var host = new WebhookHost(config);
host.Start();
```

## Notes

- **Mutability:** Instance properties such as `Enabled`, `Secret`, `MaxPayloadSizeBytes`, `ProcessingTimeoutMs`, `RetryPolicy`, `EventFilters`, `EnableDetailedLogging`, `MaxRetries`, `InitialDelayMs`, `BackoffMultiplier`, and `MaxDelayMs` are mutable after construction. Changing them while the webhook middleware is actively processing requests can lead to undefined behavior; configuration should be finalized before the middleware starts.
- **Validation:** The `IsValid` property performs only basic sanity checks (non‑null/empty path, positive size and timeout values). It does **not** verify that the `Secret` is cryptographically strong or that the `EndpointPath` does not clash with other routes. Users must ensure these concerns are addressed separately.
- **Thread safety:** Reading properties is thread‑safe. However, concurrent writes to the same instance without external synchronization can cause race conditions. The typical pattern is to configure a single instance during application startup and treat it as immutable thereafter.
- **Static factory methods:** `WithEventFilters`, `WithAllEvents`, and `WithAggressiveRetry` each return a **new** `WebhookConfiguration` instance; they do not modify any existing object. This makes them safe to use in fluent chains or LINQ expressions without side effects.
- **GetDelayForAttempt:** The method assumes a 1‑based `attemptNumber`. Supplying zero or a negative value throws `ArgumentOutOfRangeException`. The returned delay respects `MaxDelayMs`; if the exponential calculation exceeds this limit, the limit is returned.
- **AddWebhookSupport:** This extension method can be called multiple times on the same `IServiceCollection`; each call registers another singleton `WebhookConfiguration`, with the last registration overriding previous ones. To avoid unexpected overrides, call it once during application startup. The method itself does not start listening for webhooks; that responsibility lies with the middleware registered internally.
