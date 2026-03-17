// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Formatters;

/// <summary>
/// Formats plugin data as CSV for spreadsheet applications and data analysis.
/// Supports both simple summary and detailed export formats.
/// </summary>
public class CsvPluginFormatter : IPluginFormatter
{
    public string FormatType => "csv";

    public Task<string> FormatPluginAsync(Plugin plugin)
    {
        var sb = new StringBuilder();
        WriteHeaders(sb);
        WritePlugin(sb, plugin);
        return Task.FromResult(sb.ToString());
    }

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

    private static void WriteHeaders(StringBuilder sb)
    {
        sb.AppendLine("ID,Name,Version,Status,LoadedAtUtc,Dependencies,Capabilities");
    }

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

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return value.Replace("\"", "\"\"");
        }

        return value;
    }
}
