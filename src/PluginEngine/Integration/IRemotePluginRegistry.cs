// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Integration;

/// <summary>
/// Contract for remote plugin registry operations including search, download, and publish.
/// </summary>
public interface IRemotePluginRegistry
{
    Task<List<PluginInfo>> SearchAsync(string query, int limit = 20);
    Task<PluginInfo?> GetPluginAsync(Guid pluginId);
    Task<List<PluginVersionInfo>> GetVersionsAsync(Guid pluginId);
    Task<string?> DownloadPluginAsync(Guid pluginId, string version, string downloadPath);
    Task<bool> PublishPluginAsync(string filePath, PluginPublishMetadata metadata);
}
