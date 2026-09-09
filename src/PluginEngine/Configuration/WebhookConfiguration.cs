#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Configuration;

/// <summary>
/// Configuration for webhook support in the plugin engine.
/// Manages incoming webhooks from external systems and registries.
/// </summary>
public sealed class WebhookConfiguration
{
    /// <summary>
    /// Default maximum payload size in bytes (1MB).
    /// </summary>
    public const int DefaultMaxPayloadSizeBytes = 1048576;

    /// <summary>
    /// Default endpoint path for webhooks.
    /// </summary>
    public const string DefaultEndpointPath = "/webhooks/plugins";

    /// <summary>
    /// Default processing timeout in milliseconds (30 seconds).
    /// </summary>
    public const int DefaultProcessingTimeoutMs = 30000;

    /// <summary>
    /// Minimum allowed payload size in bytes (1KB).
    /// </summary>
    public const int MinPayloadSizeBytes = 1024;

    /// <summary>
    /// Enables webhook support.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Secret key for webhook signature verification.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Maximum allowed payload size in bytes.
    /// </summary>
    public int MaxPayloadSizeBytes { get; set; } = DefaultMaxPayloadSizeBytes;

    /// <summary>
    /// HTTP endpoint path for receiving webhooks.
    /// </summary>
    public string EndpointPath { get; set; } = DefaultEndpointPath;

    /// <summary>
    /// Timeout for webhook processing in milliseconds.
    /// </summary>
    public int ProcessingTimeoutMs { get; set; } = DefaultProcessingTimeoutMs;

    /// <summary>
    /// Retry policy for failed webhook processing.
    /// </summary>
    public WebhookRetryPolicy RetryPolicy { get; set; } = new();

    /// <summary>
    /// Webhook event filters (process only specific event types).
    /// </summary>
    public List<string> EventFilters { get; set; } = [];

    /// <summary>
    /// Enables detailed webhook logging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Validates the webhook configuration for correctness.
    /// </summary>
    public bool IsValid()
    {
        if (!Enabled)
            return true;

        if (string.IsNullOrWhiteSpace(Secret))
        {
            throw new InvalidOperationException("Webhook secret is required when webhooks are enabled");
        }

        if (MaxPayloadSizeBytes < MinPayloadSizeBytes)
        {
            throw new InvalidOperationException("MaxPayloadSizeBytes must be at least 1KB");
        }

        return true;
    }
}

/// <summary>
/// Retry policy configuration for webhook processing failures.
/// </summary>
public sealed class WebhookRetryPolicy
{
    /// <summary>
    /// Default maximum number of retry attempts.
    /// </summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>
    /// Default initial delay in milliseconds before first retry.
    /// </summary>
    public const int DefaultInitialDelayMs = 1000;

    /// <summary>
    /// Default multiplier for exponential backoff.
    /// </summary>
    public const double DefaultBackoffMultiplier = 2.0;

    /// <summary>
    /// Default maximum delay in milliseconds between retries.
    /// </summary>
    public const int DefaultMaxDelayMs = 60000;

    /// <summary>
    /// Aggressive retry policy maximum number of retry attempts.
    /// </summary>
    public const int AggressiveMaxRetries = 5;

    /// <summary>
    /// Aggressive retry policy initial delay in milliseconds before first retry.
    /// </summary>
    public const int AggressiveInitialDelayMs = 500;

    /// <summary>
    /// Aggressive retry policy multiplier for exponential backoff.
    /// </summary>
    public const double AggressiveBackoffMultiplier = 1.5;

    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = DefaultMaxRetries;

    /// <summary>
    /// Initial delay in milliseconds before first retry.
    /// </summary>
    public int InitialDelayMs { get; set; } = DefaultInitialDelayMs;

    /// <summary>
    /// Multiplier for exponential backoff.
    /// </summary>
    public double BackoffMultiplier { get; set; } = DefaultBackoffMultiplier;

    /// <summary>
    /// Maximum delay in milliseconds between retries.
    /// </summary>
    public int MaxDelayMs { get; set; } = DefaultMaxDelayMs;

    /// <summary>
    /// Calculates delay for the specified retry attempt.
    /// </summary>
    public int GetDelayForAttempt(int attemptNumber)
    {
        var delay = (int)(InitialDelayMs * Math.Pow(BackoffMultiplier, attemptNumber - 1));
        return Math.Min(delay, MaxDelayMs);
    }
}

/// <summary>
/// Extension methods for webhook configuration.
/// </summary>
public static class WebhookConfigurationExtensions
{
    /// <summary>
    /// Adds webhook configuration to services.
    /// </summary>
    public static IServiceCollection AddWebhookSupport(
        this IServiceCollection services,
        Action<WebhookConfiguration>? configure = null)
    {
        var config = new WebhookConfiguration();
        configure?.Invoke(config);

        if (config.IsValid())
        {
            services.AddSingleton(config);
        }

        return services;
    }

    /// <summary>
    /// Configures webhook events to process.
    /// </summary>
    public static WebhookConfiguration WithEventFilters(
        this WebhookConfiguration config,
        params string[] eventTypes)
    {
        config.EventFilters.Clear();
        config.EventFilters.AddRange(eventTypes);
        return config;
    }

    /// <summary>
    /// Enables all standard plugin webhook events.
    /// </summary>
    public static WebhookConfiguration WithAllEvents(this WebhookConfiguration config)
    {
        return config.WithEventFilters(
            "plugin.created",
            "plugin.updated",
            "plugin.deleted",
            "plugin.security_patch");
    }

    /// <summary>
    /// Configures aggressive retry policy for reliability.
    /// </summary>
    public static WebhookConfiguration WithAggressiveRetry(this WebhookConfiguration config)
    {
        config.RetryPolicy.MaxRetries = WebhookRetryPolicy.AggressiveMaxRetries;
        config.RetryPolicy.InitialDelayMs = WebhookRetryPolicy.AggressiveInitialDelayMs;
        config.RetryPolicy.BackoffMultiplier = WebhookRetryPolicy.AggressiveBackoffMultiplier;
        return config;
    }
}
