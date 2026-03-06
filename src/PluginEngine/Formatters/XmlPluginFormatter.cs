#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Formatters;

/// <summary>
/// Formats plugin data as XML for configuration files and system integration.
/// Provides structured output suitable for parsing and transformation.
/// </summary>
public sealed class XmlPluginFormatter : IPluginFormatter
{
    public string FormatType => "xml";

    public Task<string> FormatPluginAsync(Plugin plugin)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("Plugin");
        WritePluginElement(xmlWriter, plugin);
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return Task.FromResult(stringWriter.ToString());
    }

    public Task<string> FormatPluginsAsync(IEnumerable<Plugin> plugins)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("Plugins");

        foreach (var plugin in plugins)
        {
            xmlWriter.WriteStartElement("Plugin");
            WritePluginElement(xmlWriter, plugin);
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return Task.FromResult(stringWriter.ToString());
    }

    public Task<string> FormatDetailedReportAsync(Plugin plugin)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("PluginReport");

        // Plugin info
        xmlWriter.WriteStartElement("PluginInfo");
        xmlWriter.WriteElementString("Id", plugin.Id.ToString());
        xmlWriter.WriteElementString("Name", plugin.Name);
        xmlWriter.WriteElementString("Version", plugin.Version);
        xmlWriter.WriteElementString("Status", plugin.Status.ToString());
        xmlWriter.WriteElementString("LoadedAt", plugin.ModifiedAt.ToString("O"));
        xmlWriter.WriteEndElement();

        // Metadata
        if (plugin.Metadata is not null)
        {
            xmlWriter.WriteStartElement("Metadata");
            xmlWriter.WriteElementString("Description", plugin.Metadata.Description);
            if (!string.IsNullOrEmpty(plugin.Metadata.Author))
                xmlWriter.WriteElementString("Author", plugin.Metadata.Author);
            if (!string.IsNullOrEmpty(plugin.Metadata.Company))
                xmlWriter.WriteElementString("Company", plugin.Metadata.Company);
            xmlWriter.WriteEndElement();
        }

        // Dependencies
        xmlWriter.WriteStartElement("Dependencies");
        foreach (var dep in plugin.Dependencies)
        {
            xmlWriter.WriteStartElement("Dependency");
            xmlWriter.WriteElementString("Id", dep.DependencyPluginId.ToString());
            xmlWriter.WriteElementString("RequiredVersion", dep.MinimumVersion);
            xmlWriter.WriteElementString("IsOptional", dep.IsOptional.ToString());
            xmlWriter.WriteEndElement();
        }
        xmlWriter.WriteEndElement();

        // Capabilities
        xmlWriter.WriteStartElement("Capabilities");
        foreach (var cap in plugin.Capabilities)
        {
            xmlWriter.WriteStartElement("Capability");
            xmlWriter.WriteElementString("Name", cap.Name);
            xmlWriter.WriteElementString("Description", cap.Description);
            xmlWriter.WriteElementString("Version", cap.Version);
            xmlWriter.WriteEndElement();
        }
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return Task.FromResult(stringWriter.ToString());
    }

    public Task<string> FormatHealthReportAsync(PluginHealthInfo health)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("HealthReport");

        xmlWriter.WriteStartElement("Plugin");
        xmlWriter.WriteElementString("Id", health.PluginId.ToString());
        xmlWriter.WriteElementString("Name", health.PluginName);
        xmlWriter.WriteElementString("Status", health.Status);
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Health");
        xmlWriter.WriteElementString("IsHealthy", health.IsHealthy.ToString());
        xmlWriter.WriteElementString("DependencyCount", health.DependencyCount.ToString());
        xmlWriter.WriteElementString("CapabilityCount", health.CapabilityCount.ToString());
        xmlWriter.WriteElementString("LoadTimeMs", health.LoadTimeMs.ToString());
        xmlWriter.WriteElementString("LastAccessed", health.LastAccessedUtc.ToString("O"));
        xmlWriter.WriteEndElement();

        if (health.Issues.Count > 0)
        {
            xmlWriter.WriteStartElement("Issues");
            foreach (var issue in health.Issues)
            {
                xmlWriter.WriteElementString("Issue", issue);
            }
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return Task.FromResult(stringWriter.ToString());
    }

    private static void WritePluginElement(XmlTextWriter xmlWriter, Plugin plugin)
    {
        xmlWriter.WriteElementString("Id", plugin.Id.ToString());
        xmlWriter.WriteElementString("Name", plugin.Name);
        xmlWriter.WriteElementString("Version", plugin.Version);
        xmlWriter.WriteElementString("Status", plugin.Status.ToString());
        xmlWriter.WriteElementString("LoadedAt", plugin.ModifiedAt.ToString("O"));
        xmlWriter.WriteElementString("DependencyCount", plugin.Dependencies.Count.ToString());
        xmlWriter.WriteElementString("CapabilityCount", plugin.Capabilities.Count.ToString());
    }
}
