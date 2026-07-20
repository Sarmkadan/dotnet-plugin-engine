using Microsoft.AspNetCore.Mvc;
using PluginEngine.Services.Abstractions;
using PluginEngine.Events;

namespace PluginEngine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class HealthController : ControllerBase
    {
        private readonly IPluginManagerService _pluginManagerService;
        private readonly IHotReloadService _hotReloadService;
        private readonly IPluginEventPublisher _eventPublisher;

        public HealthController(
            IPluginManagerService pluginManagerService,
            IHotReloadService hotReloadService,
            IPluginEventPublisher eventPublisher)
        {
            _pluginManagerService = pluginManagerService;
            _hotReloadService = hotReloadService;
            _eventPublisher = eventPublisher;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealthAsync()
        {
            var status = await _pluginManagerService.GetStatusAsync();
            return Ok(status);
        }

        [HttpGet("ready")]
        public async Task<IActionResult> GetReadyAsync()
        {
            var status = await _pluginManagerService.GetStatusAsync();
            if (status.IsInitialized && status.LoadedPlugins > 0)
            {
                return Ok(status);
            }
            return StatusCode(503);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReportAsync()
        {
            var status = await _pluginManagerService.GetStatusAsync();
            var stats = await _pluginManagerService.GetStatisticsAsync();
            var hotReloadStats = await _hotReloadService.GetStatisticsAsync();
            var eventStats = _eventPublisher.GetStatistics();

            var report = new
            {
                Status = new
                {
                    IsInitialized = status.IsInitialized,
                    InitializedAt = status.InitializedAt,
                    TotalPlugins = status.TotalPlugins,
                    LoadedPlugins = status.LoadedPlugins,
                    ActivePlugins = status.ActivePlugins,
                    FailedPlugins = status.FailedPlugins,
                    LastError = status.LastError
                },
                HotReload = new
                {
                    TotalReloads = hotReloadStats.TotalReloads,
                    SuccessfulReloads = hotReloadStats.SuccessfulReloads,
                    FailedReloads = hotReloadStats.FailedReloads,
                    LastReloadTime = hotReloadStats.LastReloadTime,
                    AverageReloadTime = hotReloadStats.AverageReloadTime,
                    RecentEvents = hotReloadStats.RecentEvents
                },
                Events = new
                {
                    EventsPublished = eventStats.EventsPublished,
                    RegisteredSubscribers = eventStats.RegisteredSubscribers,
                    MonitoredEventTypes = eventStats.MonitoredEventTypes,
                    Timestamp = eventStats.Timestamp
                },
                Execution = new
                {
                    TotalPlugins = stats.TotalPlugins,
                    LoadedPlugins = stats.LoadedPlugins,
                    ActivePlugins = stats.ActivePlugins,
                    FailedPlugins = stats.FailedPlugins,
                    TotalMemoryUsageBytes = stats.TotalMemoryUsageBytes,
                    TotalLoadContexts = stats.TotalLoadContexts,
                    LastOperationTime = stats.LastOperationTime,
                    AverageLoadTimeMs = stats.AverageLoadTimeMs
                }
            };

            return Ok(report);
        }
    }
}
