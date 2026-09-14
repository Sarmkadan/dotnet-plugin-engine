namespace dotnet_plugin_engine.Tests;

using System;
using System.Threading.Tasks;
using Xunit;
using PluginEngine.Formatters;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

/// <summary>
/// Tests for the XmlPluginFormatter class.
/// </summary>
public class XmlPluginFormatterTests
{
    [Fact]
    /// <summary>Verifies that FormatPluginAsync returns a valid XML string.</summary>
    public async Task FormatPluginAsync_ReturnsXmlString()
    {
        // Arrange
        var plugin = new Plugin { Id = Guid.NewGuid(), Name = "Test Plugin", Version = "1.0.0" };

        // Act
        var result = await new XmlPluginFormatter().FormatPluginAsync(plugin);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<Plugin>", result);
        Assert.Contains(plugin.Id.ToString(), result);
        Assert.Contains(plugin.Name, result);
        Assert.Contains(plugin.Version, result);
    }

    [Fact]
    /// <summary>Verifies that FormatPluginsAsync returns a valid XML string for multiple plugins.</summary>
    public async Task FormatPluginsAsync_ReturnsXmlString()
    {
        // Arrange
        var plugin1 = new Plugin { Id = Guid.NewGuid(), Name = "Test Plugin 1", Version = "1.0.0" };
        var plugin2 = new Plugin { Id = Guid.NewGuid(), Name = "Test Plugin 2", Version = "2.0.0" };

        // Act
        var result = await new XmlPluginFormatter().FormatPluginsAsync(new[] { plugin1, plugin2 });

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<Plugins>", result);
        Assert.Contains(plugin1.Id.ToString(), result);
        Assert.Contains(plugin1.Name, result);
        Assert.Contains(plugin1.Version, result);
        Assert.Contains(plugin2.Id.ToString(), result);
        Assert.Contains(plugin2.Name, result);
        Assert.Contains(plugin2.Version, result);
    }

    [Fact]
    /// <summary>Verifies that FormatDetailedReportAsync returns a valid XML string with plugin details.</summary>
    public async Task FormatDetailedReportAsync_ReturnsXmlString()
    {
        // Arrange
        var plugin = new Plugin { Id = Guid.NewGuid(), Name = "Test Plugin", Version = "1.0.0" };
        var metadata = new PluginMetadata { Description = "Test Plugin Description" };
        var dependency = new PluginDependency { DependencyPluginId = Guid.NewGuid(), MinimumVersion = "1.0.0" };
        var capability = new PluginCapability { Name = "Test Capability", Description = "Test Capability Description", Version = "1.0.0" };

        plugin.Metadata = metadata;
        plugin.Dependencies.Add(dependency);
        plugin.Capabilities.Add(capability);

        // Act
        var result = await new XmlPluginFormatter().FormatDetailedReportAsync(plugin);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<PluginReport>", result);
        Assert.Contains(plugin.Id.ToString(), result);
        Assert.Contains(plugin.Name, result);
        Assert.Contains(plugin.Version, result);
        Assert.Contains(metadata.Description, result);
        Assert.Contains(dependency.DependencyPluginId.ToString(), result);
        Assert.Contains(dependency.MinimumVersion, result);
        Assert.Contains(capability.Name, result);
        Assert.Contains(capability.Description, result);
        Assert.Contains(capability.Version, result);
    }

    [Fact]
    /// <summary>Verifies that FormatHealthReportAsync returns a valid XML string.</summary>
    public async Task FormatHealthReportAsync_ReturnsXmlString()
    {
        // Arrange
        var pluginHealthInfo = new PluginHealthInfo { PluginId = Guid.NewGuid(), PluginName = "Test Plugin", Status = "Healthy" };

        // Act
        var result = await new XmlPluginFormatter().FormatHealthReportAsync(pluginHealthInfo);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<HealthReport>", result);
        Assert.Contains(pluginHealthInfo.PluginId.ToString(), result);
        Assert.Contains(pluginHealthInfo.PluginName, result);
        Assert.Contains(pluginHealthInfo.Status, result);
    }

    [Fact]
    /// <summary>Verifies that FormatPluginAsync throws ArgumentNullException when plugin is null.</summary>
    public async Task FormatPluginAsync_ThrowsArgumentNullException_WhenPluginIsNull()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => new XmlPluginFormatter().FormatPluginAsync(null));
    }

    [Fact]
    /// <summary>Verifies that FormatPluginsAsync throws ArgumentNullException when plugins is null.</summary>
    public async Task FormatPluginsAsync_ThrowsArgumentNullException_WhenPluginsIsNull()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => new XmlPluginFormatter().FormatPluginsAsync(null));
    }

    [Fact]
    /// <summary>Verifies that FormatDetailedReportAsync throws ArgumentNullException when plugin is null.</summary>
    public async Task FormatDetailedReportAsync_ThrowsArgumentNullException_WhenPluginIsNull()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => new XmlPluginFormatter().FormatDetailedReportAsync(null));
    }

    [Fact]
    public async Task FormatHealthReportAsync_ThrowsArgumentNullException_WhenPluginHealthInfoIsNull()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => new XmlPluginFormatter().FormatHealthReportAsync(null));
    }
}
