#nullable enable
using FluentAssertions;
using PluginEngine.Domain.Entities;
using PluginEngine.Formatters;
using System.Text.Json;
using Xunit;

namespace PluginEngine.Tests;

public sealed class JsonPluginFormatterTests
{
    private readonly JsonPluginFormatter _sut = new();

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

    [Fact]
    public void FormatType_IsJson()
    {
        _sut.FormatType.Should().Be("json");
    }

    // ── FormatPluginAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task FormatPluginAsync_ProducesValidJson()
    {
        var plugin = MakePlugin();

        var json = await _sut.FormatPluginAsync(plugin);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task FormatPluginAsync_ContainsPluginName()
    {
        var plugin = MakePlugin("AuthPlugin");

        var json = await _sut.FormatPluginAsync(plugin);

        json.Should().Contain("AuthPlugin");
    }

    [Fact]
    public async Task FormatPluginAsync_ContainsVersion()
    {
        var plugin = MakePlugin("MyPlugin", "2.5.0");

        var json = await _sut.FormatPluginAsync(plugin);

        json.Should().Contain("2.5.0");
    }

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

    [Fact]
    public async Task FormatPluginsAsync_ProducesValidJson()
    {
        var plugins = new[] { MakePlugin("A"), MakePlugin("B") };

        var json = await _sut.FormatPluginsAsync(plugins);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task FormatPluginsAsync_CountMatchesPluginCount()
    {
        var plugins = new[] { MakePlugin("A"), MakePlugin("B"), MakePlugin("C") };

        var json = await _sut.FormatPluginsAsync(plugins);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("count").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task FormatPluginsAsync_WithEmptyCollection_ReturnsZeroCount()
    {
        var json = await _sut.FormatPluginsAsync(Array.Empty<Plugin>());
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("count").GetInt32().Should().Be(0);
    }

    // ── FormatDetailedReportAsync ───────────────────────────────────────────

    [Fact]
    public async Task FormatDetailedReportAsync_ProducesValidJson()
    {
        var plugin = MakePlugin();

        var json = await _sut.FormatDetailedReportAsync(plugin);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

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

    [Fact]
    public async Task FormatDetailedReportAsync_MetadataIsNullWhenNotSet()
    {
        var plugin = MakePlugin();

        var json = await _sut.FormatDetailedReportAsync(plugin);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("metadata").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ── FormatHealthReportAsync ─────────────────────────────────────────────

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
