using Microsoft.AspNetCore.Mvc; 
using PluginEngine.Services.Abstractions; 
using PluginEngine.Events;

namespace PluginEngine.Controllers { 
    [ApiController] 
    [Route("[controller]")] 
    public sealed class HealthController : ControllerBase { 
        private readonly IPluginManagerService _pluginManagerService; 
        private readonly IHotReloadService _hotReloadService;
        private readonly IPluginEventPublisher _eventPublisher;

        public HealthController(
            IPluginManagerService pluginManagerService,
            IHotReloadService hotReloadService,
            IPluginEventPublisher eventPublisher) { 
            _pluginManagerService = pluginManagerService; 
            _hotReloadService = hotReloadService;
            _eventPublisher = eventPublisher;
        } 

        [HttpGet] 
        public async Task<IActionResult> GetHealthAsync() { 
            var status = await _pluginManagerService.GetStatusAsync(); 
            return Ok(status); 
        } 

        [HttpGet("ready")] 
        public async Task<IActionResult> GetReadyAsync() { 
            var status = await _pluginManagerService.GetStatusAsync(); 
            if (status.IsInitialized && status.LoadedPlugins > 0) { 
                return Ok(status); 
            } 
            return StatusCode(503); 
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReportAsync()
        {
            var status = await _pluginManagerService.GetStatusAsync();
            var hotReloadStats = await _hotReloadService.GetStatisticsAsync();
            var eventStats = _eventPublisher.GetStatistics();

            var report = new {
                Status = status,
                HotReload = hotReloadStats,
                Events = eventStats,
                Execution = new { Message = "Execution metrics not currently supported" } // Placeholder
            };
            
            return Ok(report);
        }
    } 
}