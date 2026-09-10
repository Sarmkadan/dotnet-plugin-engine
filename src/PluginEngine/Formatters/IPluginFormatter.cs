#nullable enable
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
public sealed class PluginHealthInfo
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public required Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin.
    /// </summary>
    public required string PluginName { get; set; }

    /// <summary>
    /// Gets or sets the status of the plugin.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the number of dependencies.
    /// </summary>
    public required int DependencyCount { get; set; }

    /// <summary>
    /// Gets or sets the number of capabilities.
    /// </summary>
    public required int CapabilityCount { get; set; }

    /// <summary>
    /// Gets or sets the load time in milliseconds.
    /// </summary>
    public long LoadTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the last accessed time in UTC.
    /// </summary>
    public DateTime LastAccessedUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the list of issues.
    /// </summary>
    public List<string> Issues { get; set; } = [];
}

/// <summary>
/// Factory for creating formatter instances based on format type.
/// </summary>
public sealed class FormatterFactory
{
    private readonly Dictionary<string, Func<IPluginFormatter>> _formatters;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatterFactory"/> class.
    /// </summary>
    /// <param name="json">The JSON formatter.</param>
    /// <param name="csv">The CSV formatter.</param>
    /// <param name="xml">The XML formatter.</param>
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
