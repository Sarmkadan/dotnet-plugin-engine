// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.BackgroundServices;

/// <summary>
/// Background service that monitors plugin file system changes.
/// Detects plugin installations, updates, and deletions for automatic hot reload.
/// </summary>
public class BackgroundPluginMonitor : BackgroundService
{
    private readonly IPluginManagerService _pluginManager;
    private readonly IHotReloadService _hotReloadService;
    private readonly ILogger<BackgroundPluginMonitor> _logger;
    private readonly PluginEngineOptions _options;
    private FileSystemWatcher? _watcher;

    public BackgroundPluginMonitor(
        IPluginManagerService pluginManager,
        IHotReloadService hotReloadService,
        ILogger<BackgroundPluginMonitor> logger,
        IOptions<PluginEngineOptions> options)
    {
        _pluginManager = pluginManager;
        _hotReloadService = hotReloadService;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableHotReload)
        {
            _logger.LogInformation("Plugin monitoring disabled");
            return;
        }

        _logger.LogInformation("Plugin monitor service starting");

        try
        {
            InitializeFileSystemWatcher();
            await _hotReloadService.StartHotReloadMonitoringAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Plugin monitor service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in plugin monitor service");
        }
        finally
        {
            _watcher?.Dispose();
        }
    }

    private void InitializeFileSystemWatcher()
    {
        try
        {
            if (!Directory.Exists(_options.PluginDirectory))
            {
                _logger.LogWarning("Plugin directory not found: {Path}", _options.PluginDirectory);
                return;
            }

            _watcher = new FileSystemWatcher(_options.PluginDirectory)
            {
                Filter = "*.dll",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnPluginFileCreated;
            _watcher.Changed += OnPluginFileChanged;
            _watcher.Deleted += OnPluginFileDeleted;

            _logger.LogInformation("File system watcher initialized for: {Path}", _options.PluginDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize file system watcher");
        }
    }

    private void OnPluginFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Plugin file created: {FileName}", e.Name);

        // Wait for file to be fully written
        Task.Delay(1000).ContinueWith(async _ =>
        {
            try
            {
                var plugin = await _pluginManager.LoadPluginAsync(e.FullPath);
                _logger.LogInformation("Auto-loaded plugin: {PluginName} v{Version}", plugin.Name, plugin.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-load plugin: {FilePath}", e.FullPath);
            }
        });
    }

    private void OnPluginFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Plugin file changed: {FileName}", e.Name);

        // Trigger hot reload for the changed plugin
        Task.Delay(500).ContinueWith(async _ =>
        {
            try
            {
                // In production, would match file to plugin ID and reload
                _logger.LogDebug("Change detected for plugin file: {FileName}", e.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling plugin file change: {FileName}", e.Name);
            }
        });
    }

    private void OnPluginFileDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Plugin file deleted: {FileName}", e.Name);

        // Could automatically unload the plugin if desired
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Plugin monitor service stopping");
        await base.StopAsync(cancellationToken);
    }
}
