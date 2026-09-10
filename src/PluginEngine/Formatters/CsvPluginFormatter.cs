#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Formatters;

/// <summary>
/// Formats plugin data as CSV for spreadsheet applications and data analysis.
/// Supports both simple summary and detailed export formats.
/// </summary>
public sealed class CsvPluginFormatter : IPluginFormatter
{
    /// <summary>
    /// Gets the formatter type.
    /// </summary>
    public string FormatType => "csv";

    /// <summary>
    /// Formats the specified plugin as a CSV string.
    /// </summary>
    /// <param name="plugin">The plugin to format.</param>
    /// <returns>A CSV string representing the plugin.</returns>
    public Task<string> FormatPluginAsync(Plugin plugin)
    {
        var sb = new StringBuilder();
        WriteHeaders(sb);
        WritePlugin(sb, plugin);
        return Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Formats the specified plugins as a CSV string.
    /// </summary>
    /// <param name="plugins">The plugins to format.</param>
    /// <returns>A CSV string representing the plugins.</returns>
    public Task<string> FormatPluginsAsync(IEnumerable<Plugin> plugins)
    {
        var sb = new StringBuilder();
        WriteHeaders(sb);

        foreach (var plugin in plugins)
        {
            WritePlugin(sb, plugin);
        }

        return Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Formats the specified plugin's detailed report as a CSV string.
    /// </summary>
    /// <param name="plugin">The plugin to format.</param>
    /// <returns>A CSV string representing the plugin's detailed report.</returns>
    public Task<string> FormatDetailedReportAsync(Plugin plugin)
    {
        var sb = new StringBuilder();

        // Plugin info
        sb.AppendLine("Plugin Information");
        sb.AppendLine("ID,Name,Version,Status,LoadedAtUtc");
        sb.AppendLine($"\"{EscapeCsv(plugin.Id.ToString())}\",\"{EscapeCsv(plugin.Name)}\",\"{plugin.Version}\",\"{plugin.Status}\",\"{plugin.ModifiedAt:O}\"");

        // Dependencies
        sb.AppendLine();
        sb.AppendLine("Dependencies");
        sb.AppendLine("DependencyId,RequiredVersion,IsOptional");

        foreach (var dep in plugin.Dependencies)
        {
            sb.AppendLine($"\"{EscapeCsv(dep.DependencyPluginId.ToString())}\",\"{dep.MinimumVersion}\",\"{dep.IsOptional}\"");
        }

        // Capabilities
        sb.AppendLine();
        sb.AppendLine("Capabilities");
        sb.AppendLine("Name,Description,Version");

        foreach (var cap in plugin.Capabilities)
        {
            sb.AppendLine($"\"{EscapeCsv(cap.Name)}\",\"{EscapeCsv(cap.Description)}\",\"{cap.Version}\"");
        }

        return Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Formats the specified plugin health report as a CSV string.
    /// </summary>
    /// <param name="health">The plugin health information to format.</param>
    /// <returns>A CSV string representing the plugin health report.</returns>
    public Task<string> FormatHealthReportAsync(PluginHealthInfo health)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PluginId,PluginName,Status,IsHealthy,DependencyCount,CapabilityCount,LoadTimeMs,LastAccessedUtc");
        sb.AppendLine(
            $"\"{EscapeCsv(health.PluginId.ToString())}\"," +
            $"\"{EscapeCsv(health.PluginName)}\"," +
            $"\"{health.Status}\"," +
            $"\"{health.IsHealthy}\"," +
            $"{health.DependencyCount}," +
            $"{health.CapabilityCount}," +
            $"{health.LoadTimeMs}," +
            $"\"{health.LastAccessedUtc:O}\"");

        return Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Writes the CSV headers to the specified string builder.
    /// </summary>
    /// <param name="sb">The string builder to write headers to.</param>
    private static void WriteHeaders(StringBuilder sb)
    {
        sb.AppendLine("ID,Name,Version,Status,LoadedAtUtc,Dependencies,Capabilities");
    }

    /// <summary>
    /// Writes the specified plugin to the CSV string builder.
    /// </summary>
    /// <param name="sb">The string builder to write the plugin to.</param>
    /// <param name="plugin">The plugin to write.</param>
    private static void WritePlugin(StringBuilder sb, Plugin plugin)
    {
        sb.AppendLine(
            $"\"{EscapeCsv(plugin.Id.ToString())}\"," +
            $"\"{EscapeCsv(plugin.Name)}\"," +
            $"\"{plugin.Version}\"," +
            $"\"{plugin.Status}\"," +
            $"\"{plugin.ModifiedAt:O}\"," +
            $"{plugin.Dependencies.Count}," +
            $"{plugin.Capabilities.Count}");
    }

    /// <summary>
    /// Escapes a string for CSV output by doubling quotes and wrapping in quotes if necessary.
    /// </summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>The escaped string suitable for CSV.</returns>
    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return value.Replace("\"", "\"\"");
        }

        return value;
    }
}
