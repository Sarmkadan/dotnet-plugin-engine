#nullable enable
using FluentAssertions;
using PluginEngine.Domain.Entities;
using PluginEngine.Formatters;
using System.Text.Json;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Tests for the JsonPluginFormatter class.
/// </summary>
public sealed class JsonPluginFormatterTests
{
    private readonly JsonPluginFormatter _sut = new();

    /// <summary>
    /// Creates a new plugin with the specified name and version.
    /// </summary>
    /// <param name="name">The name of the plugin.</param>
    /// <param name="version">The version of the plugin.</param>
    /// <returns>A new plugin instance.</returns>
    private static Plugin MakePlugin(string name = "TestPlugin", string version = "1.0.0")
    {
        return new Plugin
        {
            Id = Guid.NewGuid(),
            Name = name,
            Version = version,
            AssemblyPath = $"/plugins/{name}.dll",
            Status = PluginStatus.Active
        };
    }

    // ── FormatType ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the FormatType method returns "json".
    /// </summary>
    [Fact]
    public void FormatType_IsJson()
    {
        _sut.FormatType.Should().Be("json");
    }

    // ── FormatPluginAsync ───────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the FormatPluginAsync method produces valid JSON.
    /// </summary>
    [Fact]
    public async Task FormatPluginAsync_ProducesValidJson()
    {
        var plugin = MakePlugin();

        var json = await _sut.FormatPluginAsync(plugin);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the FormatPluginAsync method includes the plugin name in the JSON output.
    /// </summary>
    [Fact]
    public async Task FormatPluginAsync_ContainsPluginName()
    {
        var plugin = MakePlugin("AuthPlugin");

        var json = await _sut.FormatPluginAsync(plugin);

        json.Should().Contain("AuthPlugin");
    }

    /// <summary>
    /// Verifies that the FormatPluginAsync method includes the plugin version in the JSON output.
    /// </summary>
    [Fact]
    public async Task FormatPluginAsync_ContainsVersion()
    {
        var plugin = MakePlugin("MyPlugin", "2.5.0");

        var json = await _sut.FormatPluginAsync(plugin);

        json.Should().Contain("2.5.0");
    }

    /// <summary>
    /// Verifies that the FormatPluginAsync method includes the dependency count in the JSON output.
    /// </summary>
    [Fact]
    public async Task FormatPluginAsync_ContainsDependencyCount()
    {
        var plugin = MakePlugin();
        plugin.AddDependency(new PluginDependency
        {
            PluginId = plugin.Id,
            DependencyPluginId = Guid.NewGuid(),
            MinimumVersion = "1.0.0"
        });

        var json = await _sut.FormatPluginAsync(plugin);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("dependencyCount").GetInt32().Should().Be(1);
    }

    // ── FormatPluginsAsync ──────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the FormatPluginsAsync method produces valid JSON.
    /// </summary>
    [Fact]
    public async Task FormatPluginsAsync_ProducesValidJson()
    {
        var plugins = new[] { MakePlugin("A"), MakePlugin("B") };

        var json = await _sut.FormatPluginsAsync(plugins);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the FormatPluginsAsync method includes the correct count of plugins in the JSON output.
    /// </summary>
    [Fact]
    public async Task FormatPluginsAsync_CountMatchesPluginCount()
    {
        var plugins = new[] { MakePlugin("A"), MakePlugin("B"), MakePlugin("C") };

        var json = await _sut.FormatPluginsAsync(plugins);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("count").GetInt32().Should().Be(3);
    }

    /// <summary>
    /// Verifies that the FormatPluginsAsync method returns a count of 0 when given an empty collection.
    /// </summary>
    [Fact]
    public async Task FormatPluginsAsync_WithEmptyCollection_ReturnsZeroCount()
    {
        var json = await _sut.FormatPluginsAsync(Array.Empty<Plugin>());
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("count").GetInt32().Should().Be(0);
    }

    // ── FormatDetailedReportAsync ───────────────────────────────────────────

    /// <summary>
    /// Verifies that the FormatDetailedReportAsync method produces valid JSON.
    /// </summary>
    [Fact]
    public async Task FormatDetailedReportAsync_ProducesValidJson()
    {
        var plugin = MakePlugin();

        var json = await _sut.FormatDetailedReportAsync(plugin);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the FormatDetailedReportAsync method includes metadata in the JSON output when it is set.
    /// </summary>
    [Fact]
    public async Task FormatDetailedReportAsync_ContainsMetadataWhenSet()
    {
        var plugin = MakePlugin();
        plugin.Metadata = new PluginMetadata
        {
            PluginId = plugin.Id,
            Author = "TestAuthor",
            Description = "Some description"
        };

        var json = await _sut.FormatDetailedReportAsync(plugin);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("metadata").ValueKind.Should().NotBe(JsonValueKind.Null);
        doc.RootElement.GetProperty("metadata").GetProperty("author").GetString().Should().Be("TestAuthor");
    }

    /// <summary>
    /// Verifies that the FormatDetailedReportAsync method does not include metadata in the JSON output when it is not set.
    /// </summary>
    [Fact]
    public async Task FormatDetailedReportAsync_MetadataIsNullWhenNotSet()
    {
        var plugin = MakePlugin();

        var json = await _sut.FormatDetailedReportAsync(plugin);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("metadata").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ── FormatHealthReportAsync ─────────────────────────────────────────────

    /// <summary>
    /// Verifies that the FormatHealthReportAsync method produces valid JSON.
    /// </summary>
    [Fact]
    public async Task FormatHealthReportAsync_ProducesValidJson()
    {
        var health = new PluginHealthInfo
        {
            PluginId = Guid.NewGuid(),
            PluginName = "HealthyPlugin",
            Status = "Active",
            DependencyCount = 2,
            CapabilityCount = 1,
            IsHealthy = true
        };

        var json = await _sut.FormatHealthReportAsync(health);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the FormatHealthReportAsync method reflects the isHealthy property in the JSON output.
    /// </summary>
    [Fact]
    public async Task FormatHealthReportAsync_IsHealthyReflectsInput()
    {
        var health = new PluginHealthInfo
        {
            PluginId = Guid.NewGuid(),
            PluginName = "Plugin",
            Status = "Active",
            DependencyCount = 0,
            CapabilityCount = 0,
            IsHealthy = false,
            Issues = ["Missing dependency"]
        };

        var json = await _sut.FormatHealthReportAsync(health);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("health").GetProperty("isHealthy").GetBoolean().Should().BeFalse();
    }
}
