#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;
using PluginEngine.Utils.Helpers;
using Xunit;

namespace PluginEngine.Tests;

public sealed class DependencyGraphAnalyzerTests
{
    private readonly Mock<IDependencyResolutionService> _mockResolver;
    private readonly Mock<ILogger<DependencyGraphAnalyzer>> _mockLogger;
    private readonly DependencyGraphAnalyzer _sut;

    public DependencyGraphAnalyzerTests()
    {
        _mockResolver = new Mock<IDependencyResolutionService>();
        _mockLogger = new Mock<ILogger<DependencyGraphAnalyzer>>();
        _sut = new DependencyGraphAnalyzer(_mockResolver.Object, _mockLogger.Object);
    }

    private static Plugin MakePlugin(string name = "TestPlugin", int depCount = 0)
    {
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = name,
            Version = "1.0.0",
            AssemblyPath = $"/plugins/{name}.dll"
        };

        for (int i = 0; i < depCount; i++)
        {
            plugin.AddDependency(new PluginDependency
            {
                PluginId = plugin.Id,
                DependencyPluginId = Guid.NewGuid(),
                MinimumVersion = "1.0.0"
            });
        }

        return plugin;
    }

    // ── FindDependentsAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task FindDependentsAsync_WithNoDependents_ReturnsEmpty()
    {
        var targetId = Guid.NewGuid();
        var plugins = new List<Plugin> { MakePlugin("Standalone") };

        var result = await _sut.FindDependentsAsync(plugins, targetId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindDependentsAsync_WithOneDependentPlugin_ReturnsThatPlugin()
    {
        var targetId = Guid.NewGuid();
        var dependent = MakePlugin("Dependent");
        dependent.AddDependency(new PluginDependency
        {
            PluginId = dependent.Id,
            DependencyPluginId = targetId,
            MinimumVersion = "1.0.0"
        });

        var result = await _sut.FindDependentsAsync(new[] { dependent }, targetId);

        result.Should().ContainSingle()
            .Which.Should().Be(dependent.Id);
    }

    [Fact]
    public async Task FindDependentsAsync_WithMultipleDependents_ReturnsAll()
    {
        var targetId = Guid.NewGuid();
        var dep1 = MakePlugin("Dep1");
        var dep2 = MakePlugin("Dep2");
        var unrelated = MakePlugin("Unrelated");

        dep1.AddDependency(new PluginDependency { PluginId = dep1.Id, DependencyPluginId = targetId, MinimumVersion = "1.0.0" });
        dep2.AddDependency(new PluginDependency { PluginId = dep2.Id, DependencyPluginId = targetId, MinimumVersion = "1.0.0" });

        var result = await _sut.FindDependentsAsync(new[] { dep1, dep2, unrelated }, targetId);

        result.Should().HaveCount(2)
            .And.Contain(dep1.Id)
            .And.Contain(dep2.Id);
    }

    [Fact]
    public async Task FindDependentsAsync_WithEmptyPluginList_ReturnsEmpty()
    {
        var result = await _sut.FindDependentsAsync(Array.Empty<Plugin>(), Guid.NewGuid());

        result.Should().BeEmpty();
    }

    // ── AnalyzeAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task AnalyzeAsync_WithNoDependencies_ReturnsZeroCounts()
    {
        var plugin = MakePlugin("Simple");
        _mockResolver.Setup(x => x.ResolveDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Plugin>());
        _mockResolver.Setup(x => x.HasCircularDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var report = await _sut.AnalyzeAsync(plugin);

        report.DirectDependencies.Should().Be(0);
        report.TotalDependencies.Should().Be(0);
        report.HasCircularDependencies.Should().BeFalse();
    }

    [Fact]
    public async Task AnalyzeAsync_WithCircularDependency_ReportsIssue()
    {
        var plugin = MakePlugin("Circular", depCount: 1);
        _mockResolver.Setup(x => x.ResolveDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Plugin>());
        _mockResolver.Setup(x => x.HasCircularDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var report = await _sut.AnalyzeAsync(plugin);

        report.HasCircularDependencies.Should().BeTrue();
        report.Issues.Should().Contain(i => i.Contains("Circular"));
    }

    [Fact]
    public async Task AnalyzeAsync_WithFewDependencies_HasSimpleComplexityLevel()
    {
        var plugin = MakePlugin("Simple", depCount: 1);
        _mockResolver.Setup(x => x.ResolveDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { MakePlugin("Dep") });
        _mockResolver.Setup(x => x.HasCircularDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var report = await _sut.AnalyzeAsync(plugin);

        report.GetComplexityLevel().Should().Be("Simple");
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsPluginName()
    {
        var plugin = MakePlugin("MyPlugin");
        _mockResolver.Setup(x => x.ResolveDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Plugin>());
        _mockResolver.Setup(x => x.HasCircularDependenciesAsync(plugin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var report = await _sut.AnalyzeAsync(plugin);

        report.PluginName.Should().Be("MyPlugin");
    }

    // ── GenerateGraphVisualizationAsync ─────────────────────────────────────

    [Fact]
    public async Task GenerateGraphVisualizationAsync_WithSimplePlugin_ReturnsNonEmptyString()
    {
        var plugin = MakePlugin("Root");
        _mockResolver.Setup(x => x.GetDependencyGraphAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DependencyGraph { RootPluginId = plugin.Id });

        var result = await _sut.GenerateGraphVisualizationAsync(plugin);

        result.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Root");
    }

    // ── DependencyAnalysisReport.GetComplexityLevel ─────────────────────────

    [Theory]
    [InlineData(0, "Simple")]
    [InlineData(19, "Simple")]
    [InlineData(20, "Moderate")]
    [InlineData(49, "Moderate")]
    [InlineData(50, "Complex")]
    [InlineData(74, "Complex")]
    [InlineData(75, "Very Complex")]
    [InlineData(100, "Very Complex")]
    public void GetComplexityLevel_MapsScoreToExpectedLabel(int score, string expected)
    {
        var report = new DependencyAnalysisReport { PluginName = "P" };
        report.ComplexityScore = score;

        report.GetComplexityLevel().Should().Be(expected);
    }
}
