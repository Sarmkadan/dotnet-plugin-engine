#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.BackgroundServices;

/// <summary>
/// Background service that periodically checks the health of loaded plugins.
/// Monitors dependencies, capabilities, and reports issues.
/// </summary>
public sealed class PluginHealthCheckService : BackgroundService
{
    private readonly IPluginManagerService _pluginManager;
    private readonly IDependencyResolutionService _dependencyResolver;
    private readonly IPluginEventPublisher _eventPublisher;
    private readonly ILogger<PluginHealthCheckService> _logger;
    private readonly PluginEngineOptions _options;

    public PluginHealthCheckService(
        IPluginManagerService pluginManager,
        IDependencyResolutionService dependencyResolver,
        IPluginEventPublisher eventPublisher,
        ILogger<PluginHealthCheckService> logger,
        IOptions<PluginEngineOptions> options)
    {
        _pluginManager = pluginManager;
        _dependencyResolver = dependencyResolver;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Plugin health check service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHealthCheckAsync();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Health check service cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task PerformHealthCheckAsync()
    {
        try
        {
            var plugins = await _pluginManager.GetAllPluginsAsync();

            if (plugins.Count == 0)
            {
                _logger.LogDebug("No plugins to check");
                return;
            }

            _logger.LogInformation("Performing health check on {PluginCount} plugins", plugins.Count);

            foreach (var plugin in plugins)
            {
                await CheckPluginHealthAsync(plugin);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing health check");
        }
    }

    private async Task CheckPluginHealthAsync(Plugin plugin)
    {
        try
        {
            var issues = new List<string>();

            // Check dependencies
            var dependenciesValid = await _dependencyResolver.ValidateDependenciesAsync(plugin);
            if (!dependenciesValid)
            {
                issues.Add("Invalid dependencies detected");
            }

            // Check for circular dependencies
            var hasCircular = await _dependencyResolver.HasCircularDependenciesAsync(plugin);
            if (hasCircular)
            {
                issues.Add("Circular dependency detected");
            }

            // Check plugin metadata
            if (string.IsNullOrWhiteSpace(plugin.Metadata?.Description))
            {
                issues.Add("Missing plugin description");
            }

            var isHealthy = issues.Count == 0 && plugin.Status == PluginStatus.Loaded;

            if (!isHealthy)
            {
                _logger.LogWarning(
                    "Plugin health check failed: {PluginName} - Issues: {Issues}",
                    plugin.Name,
                    string.Join(", ", issues));

                // Publish health warning event
                var errorEvent = new PluginErrorEvent
                {
                    PluginId = plugin.Id,
                    ErrorMessage = "Health check failed",
                    ErrorDetails = string.Join(", ", issues),
                    ErrorCode = 1000
                };

                await _eventPublisher.PublishAsync(errorEvent);
            }
            else
            {
                _logger.LogDebug("Plugin health check passed: {PluginName}", plugin.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking plugin health: {PluginName}", plugin.Name);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Plugin health check service stopping");
        await base.StopAsync(cancellationToken);
    }
}
