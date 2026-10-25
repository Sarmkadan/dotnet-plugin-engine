#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Formatters;

/// <summary>
/// Formats plugin data as JSON for API responses and file storage.
/// Provides compact and detailed output modes for flexibility.
/// </summary>
public sealed class JsonPluginFormatter : IPluginFormatter
{
    /// <summary>
    /// Gets the formatter type.
    /// </summary>
    public string FormatType => "json";

    /// <summary>
    /// Formats the specified plugin as a JSON string.
    /// </summary>
    /// <param name="plugin">The plugin to format.</param>
    /// <returns>A JSON string representing the plugin.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plugin"/> is null.</exception>
    public Task<string> FormatPluginAsync(Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var output = new
        {
            id = plugin.Id,
            name = plugin.Name,
            version = plugin.Version,
            status = plugin.Status.ToString(),
            loadedAtUtc = plugin.ModifiedAt,
            dependencyCount = plugin.Dependencies.Count,
            capabilityCount = plugin.Capabilities.Count
        };

        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(output, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    /// <summary>
    /// Formats the specified plugins as a JSON string.
    /// </summary>
    /// <param name="plugins">The plugins to format.</param>
    /// <returns>A JSON string representing the plugins.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plugins"/> is null.</exception>
    public Task<string> FormatPluginsAsync(IEnumerable<Plugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var items = plugins.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            version = p.Version,
            status = p.Status.ToString(),
            loadedAtUtc = p.ModifiedAt
        }).ToList();

        var output = new
        {
            plugins = items,
            count = items.Count,
            generatedAtUtc = DateTime.UtcNow
        };

        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(output, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    /// <summary>
    /// Formats the specified plugin's detailed report as a JSON string.
    /// </summary>
    /// <param name="plugin">The plugin to format.</param>
    /// <returns>A JSON string representing the plugin's detailed report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plugin"/> is null.</exception>
    public Task<string> FormatDetailedReportAsync(Plugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var output = new
        {
            plugin = new
            {
                id = plugin.Id,
                name = plugin.Name,
                version = plugin.Version,
                status = plugin.Status.ToString(),
                loadedAtUtc = plugin.ModifiedAt
            },
            metadata = plugin.Metadata is not null ? new
            {
                description = plugin.Metadata.Description,
                author = plugin.Metadata.Author,
                company = plugin.Metadata.Company
            } : null,
            dependencies = plugin.Dependencies.Select(d => new
            {
                id = d.DependencyPluginId,
                requiredVersion = d.MinimumVersion,
                isOptional = d.IsOptional
            }).ToList(),
            capabilities = plugin.Capabilities.Select(c => new
            {
                name = c.Name,
                description = c.Description,
                version = c.Version
            }).ToList()
        };

        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(output, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    /// <summary>
    /// Formats the specified plugin health report as a JSON string.
    /// </summary>
    /// <param name="health">The plugin health information to format.</param>
    /// <returns>A JSON string representing the plugin health report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="health"/> is null.</exception>
    public Task<string> FormatHealthReportAsync(PluginHealthInfo health)
    {
        ArgumentNullException.ThrowIfNull(health);

        var output = new
        {
            plugin = new
            {
                id = health.PluginId,
                name = health.PluginName,
                status = health.Status
            },
            health = new
            {
                isHealthy = health.IsHealthy,
                dependencyCount = health.DependencyCount,
                capabilityCount = health.CapabilityCount,
                loadTimeMs = health.LoadTimeMs,
                lastAccessedUtc = health.LastAccessedUtc
            },
            issues = health.Issues,
            reportGeneratedAtUtc = DateTime.UtcNow
        };

        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(output, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}
