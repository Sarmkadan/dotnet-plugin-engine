// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Formatters;

/// <summary>
/// Defines the contract for formatting plugin data in various output formats.
/// Supports JSON, CSV, XML, and custom formats through implementations.
/// </summary>
public interface IPluginFormatter
{
    /// <summary>
    /// Gets the format type this formatter handles.
    /// </summary>
    string FormatType { get; }

    /// <summary>
    /// Formats a single plugin for output.
    /// </summary>
    Task<string> FormatPluginAsync(Plugin plugin);

    /// <summary>
    /// Formats a collection of plugins for output.
    /// </summary>
    Task<string> FormatPluginsAsync(IEnumerable<Plugin> plugins);

    /// <summary>
    /// Formats plugin metadata and dependencies as a detailed report.
    /// </summary>
    Task<string> FormatDetailedReportAsync(Plugin plugin);

    /// <summary>
    /// Formats plugin statistics and health information.
    /// </summary>
    Task<string> FormatHealthReportAsync(PluginHealthInfo health);
}

/// <summary>
/// Contains health information about a plugin.
/// </summary>
public class PluginHealthInfo
{
    public required Guid PluginId { get; set; }
    public required string PluginName { get; set; }
    public required string Status { get; set; }
    public required int DependencyCount { get; set; }
    public required int CapabilityCount { get; set; }
    public long LoadTimeMs { get; set; }
    public DateTime LastAccessedUtc { get; set; }
    public bool IsHealthy { get; set; }
    public List<string> Issues { get; set; } = [];
}

/// <summary>
/// Factory for creating formatter instances based on format type.
/// </summary>
public class FormatterFactory
{
    private readonly Dictionary<string, Func<IPluginFormatter>> _formatters;

    public FormatterFactory(
        JsonPluginFormatter json,
        CsvPluginFormatter csv,
        XmlPluginFormatter xml)
    {
        _formatters = new()
        {
            ["json"] = () => json,
            ["csv"] = () => csv,
            ["xml"] = () => xml,
        };
    }

    /// <summary>
    /// Creates a formatter for the specified format type.
    /// </summary>
    public IPluginFormatter? GetFormatter(string formatType)
    {
        var key = formatType.ToLowerInvariant();
        return _formatters.TryGetValue(key, out var factory) ? factory() : null;
    }

    /// <summary>
    /// Gets all supported format types.
    /// </summary>
    public IEnumerable<string> GetSupportedFormats() => _formatters.Keys;
}
