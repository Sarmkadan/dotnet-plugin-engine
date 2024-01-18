// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Configuration;

/// <summary>
/// Configuration for webhook support in the plugin engine.
/// Manages incoming webhooks from external systems and registries.
/// </summary>
public class WebhookConfiguration
{
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
    public int MaxPayloadSizeBytes { get; set; } = 1048576; // 1MB

    /// <summary>
    /// HTTP endpoint path for receiving webhooks.
    /// </summary>
    public string EndpointPath { get; set; } = "/webhooks/plugins";

    /// <summary>
    /// Timeout for webhook processing in milliseconds.
    /// </summary>
    public int ProcessingTimeoutMs { get; set; } = 30000; // 30 seconds

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

        if (MaxPayloadSizeBytes < 1024)
        {
            throw new InvalidOperationException("MaxPayloadSizeBytes must be at least 1KB");
        }

        return true;
    }
}

/// <summary>
/// Retry policy configuration for webhook processing failures.
/// </summary>
public class WebhookRetryPolicy
{
    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial delay in milliseconds before first retry.
    /// </summary>
    public int InitialDelayMs { get; set; } = 1000;

    /// <summary>
    /// Multiplier for exponential backoff.
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Maximum delay in milliseconds between retries.
    /// </summary>
    public int MaxDelayMs { get; set; } = 60000;

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
        config.RetryPolicy.MaxRetries = 5;
        config.RetryPolicy.InitialDelayMs = 500;
        config.RetryPolicy.BackoffMultiplier = 1.5;
        return config;
    }
}
