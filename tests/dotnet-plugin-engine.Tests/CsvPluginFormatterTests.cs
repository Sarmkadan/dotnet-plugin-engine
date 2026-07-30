namespace dotnet_plugin_engine.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PluginEngine.Formatters;
using PluginEngine.Domain.Entities;
using Xunit;

public class CsvPluginFormatterTests
{
    private readonly CsvPluginFormatter _formatter = new();

    private static Plugin CreatePlugin(
        Guid? id = null,
        string name = "TestPlugin",
        string version = "1.0.0",
        string status = "Loaded",
        DateTime? modifiedAt = null,
        IEnumerable<PluginDependency>? dependencies = null,
        IEnumerable<PluginCapability>? capabilities = null)
    {
        return new Plugin
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Version = version,
            Status = status,
            ModifiedAt = modifiedAt ?? DateTime.UtcNow,
            Dependencies = dependencies != null ? new List<PluginDependency>(dependencies) : new List<PluginDependency>(),
            Capabilities = capabilities != null ? new List<PluginCapability>(capabilities) : new List<PluginCapability>()
        };
    }

    private static PluginDependency CreateDependency(
        Guid? dependencyId = null,
        string minimumVersion = "1.0.0",
        bool isOptional = false)
    {
        return new PluginDependency
        {
            DependencyPluginId = dependencyId ?? Guid.NewGuid(),
            MinimumVersion = minimumVersion,
            IsOptional = isOptional
        };
    }

    private static PluginCapability CreateCapability(
        string name = "Cap",
        string description = "Capability description",
        string version = "1.0")
    {
        return new PluginCapability
        {
            Name = name,
            Description = description,
            Version = version
        };
    }

    private static PluginHealthInfo CreateHealthInfo(
        Guid? pluginId = null,
        string pluginName = "TestPlugin",
        string status = "Loaded",
        bool isHealthy = true,
        int dependencyCount = 0,
        int capabilityCount = 0,
        int loadTimeMs = 123,
        DateTime? lastAccessedUtc = null)
    {
        return new PluginHealthInfo
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            PluginName = pluginName,
            Status = status,
            IsHealthy = isHealthy,
            DependencyCount = dependencyCount,
            CapabilityCount = capabilityCount,
            LoadTimeMs = loadTimeMs,
            LastAccessedUtc = lastAccessedUtc ?? DateTime.UtcNow
        };
    }

    [Fact]
    public async Task FormatPluginAsync_ReturnsHeaderAndSingleRow()
    {
        // Arrange
        var plugin = CreatePlugin();

        // Act
        var result = await _formatter.FormatPluginAsync(plugin);

        // Assert
        var nl = Environment.NewLine;
        var expectedHeader = "ID,Name,Version,Status,LoadedAtUtc,Dependencies,Capabilities" + nl;
        var expectedRow =
            $"\"{plugin.Id}\",\"{plugin.Name}\",\"{plugin.Version}\",\"{plugin.Status}\",\"{plugin.ModifiedAt:O}\",{plugin.Dependencies.Count},{plugin.Capabilities.Count}" + nl;

        Assert.Equal(expectedHeader + expectedRow, result);
    }

    [Fact]
    public async Task FormatPluginsAsync_EmptyCollection_ReturnsOnlyHeader()
    {
        // Arrange
        var plugins = new List<Plugin>();

        // Act
        var result = await _formatter.FormatPluginsAsync(plugins);

        // Assert
        var nl = Environment.NewLine;
        var expected = "ID,Name,Version,Status,LoadedAtUtc,Dependencies,Capabilities" + nl;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task FormatDetailedReportAsync_IncludesDependenciesAndCapabilitiesSections()
    {
        // Arrange
        var dependency = CreateDependency();
        var capability = CreateCapability();
        var plugin = CreatePlugin(
            dependencies: new[] { dependency },
            capabilities: new[] { capability });

        // Act
        var result = await _formatter.FormatDetailedReportAsync(plugin);

        // Assert
        // Verify the three sections exist and contain the expected CSV lines.
        Assert.Contains("Plugin Information", result);
        Assert.Contains("Dependencies", result);
        Assert.Contains("Capabilities", result);

        var depLine = $"\"{dependency.DependencyPluginId}\",\"{dependency.MinimumVersion}\",\"{dependency.IsOptional}\"";
        var capLine = $"\"{capability.Name}\",\"{capability.Description}\",\"{capability.Version}\"";

        Assert.Contains(depLine, result);
        Assert.Contains(capLine, result);
    }

    [Fact]
    public async Task FormatHealthReportAsync_ProducesCorrectCsvLine()
    {
        // Arrange
        var health = CreateHealthInfo(dependencyCount: 2, capabilityCount: 3, loadTimeMs: 456);

        // Act
        var result = await _formatter.FormatHealthReportAsync(health);

        // Assert
        var nl = Environment.NewLine;
        var expectedHeader = "PluginId,PluginName,Status,IsHealthy,DependencyCount,CapabilityCount,LoadTimeMs,LastAccessedUtc" + nl;
        var expectedLine =
            $"\"{health.PluginId}\"," +
            $"\"{health.PluginName}\"," +
            $"\"{health.Status}\"," +
            $"\"{health.IsHealthy}\"," +
            $"{health.DependencyCount}," +
            $"{health.CapabilityCount}," +
            $"{health.LoadTimeMs}," +
            $"\"{health.LastAccessedUtc:O}\"" + nl;

        Assert.Equal(expectedHeader + expectedLine, result);
    }

    [Fact]
    public async Task FormatPluginsAsync_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<Plugin>? plugins = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _formatter.FormatPluginsAsync(plugins!));
    }
}
