#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Events;
using PluginEngine.Formatters;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;
using PluginEngine.Utils.Helpers;
using PluginEngine.Utils.Validators;
using System.Text.Json;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// End-to-end integration scenarios covering the main library use cases:
/// event-driven workflows, formatter pipelines, configuration combinations,
/// and concurrent multi-plugin scenarios.
/// </summary>
public sealed class PluginSystemIntegrationTests
{
    private readonly Mock<ILogger<DependencyResolutionService>> _mockDepLogger = new();
    private readonly Mock<ILogger<PluginValidator>> _mockValLogger = new();
    private readonly Mock<ILogger<VersionHelper>> _mockVerLogger = new();
    private readonly Mock<ILogger<PluginEventPublisher>> _mockPubLogger = new();
    private readonly Mock<ILogger<PluginEventSubscriber>> _mockSubLogger = new();
    private readonly Mock<IPluginLoaderService> _mockLoaderService = new();

    private Plugin MakePlugin(string name, string version = "1.0.0", PluginStatus status = PluginStatus.Active)
    {
        return new Plugin
        {
            Id = Guid.NewGuid(),
            Name = name,
            Version = version,
            AssemblyPath = $"/plugins/{name}.dll",
            Status = status
        };
    }

    // ── README main use case: validate → resolve → publish events ───────────

    [Fact]
    public async Task MainUseCase_ValidateResolveAndPublishEvents_FullPipeline()
    {
        // Arrange
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();

        var plugin = MakePlugin("MainPlugin");
        plugin.Id = pluginId;
        plugin.Metadata = new PluginMetadata
        {
            PluginId = pluginId,
            Author = "DevTeam",
            Description = "Main application plugin"
        };

        var dependency = MakePlugin("CoreDependency", "2.0.0");
        dependency.Id = depId;

        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "1.5.0"
        });

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { dependency });

        var validator = new PluginValidator(_mockValLogger.Object, new VersionHelper(_mockVerLogger.Object));
        var resolver = new DependencyResolutionService(_mockLoaderService.Object);
        var publisher = new PluginEventPublisher(_mockPubLogger.Object);

        var publishedEvents = new List<IPluginEvent>();
        publisher.Subscribe<PluginLoadedEvent>(e => { publishedEvents.Add(e); return Task.CompletedTask; });

        // Act: Step 1 – validate
        var validation = validator.Validate(plugin);
        validation.IsValid.Should().BeTrue();

        // Step 2 – resolve dependencies
        var isValid = await resolver.ValidateDependenciesAsync(plugin);
        isValid.Should().BeTrue();

        var resolved = await resolver.ResolveDependenciesAsync(plugin);
        resolved.Should().ContainSingle();

        // Step 3 – simulate load and publish event
        await publisher.PublishAsync(new PluginLoadedEvent
        {
            PluginId = pluginId,
            PluginName = plugin.Name,
            Version = plugin.Version,
            LoadTimeMs = 42
        });

        // Assert
        publishedEvents.Should().ContainSingle()
            .Which.PluginId.Should().Be(pluginId);
    }

    // ── Event subscriber lifecycle ──────────────────────────────────────────

    [Fact]
    public async Task EventSystem_SubscriberReceivesAllRegisteredEventTypes()
    {
        var publisher = new PluginEventPublisher(_mockPubLogger.Object);
        var subscriber = new PluginEventSubscriber(publisher, _mockSubLogger.Object);

        var loadedReceived = false;
        var errorReceived = false;

        subscriber.OnPluginLoaded(_ => { loadedReceived = true; return Task.CompletedTask; });
        subscriber.OnPluginError(_ => { errorReceived = true; return Task.CompletedTask; });

        await publisher.PublishAsync(new PluginLoadedEvent
        {
            PluginId = Guid.NewGuid(),
            PluginName = "P",
            Version = "1.0.0"
        });

        await publisher.PublishAsync(new PluginErrorEvent
        {
            PluginId = Guid.NewGuid(),
            ErrorMessage = "oops"
        });

        loadedReceived.Should().BeTrue();
        errorReceived.Should().BeTrue();
    }

    [Fact]
    public void EventSystem_SubscriberGetSubscriptionCount_ReflectsRegistrations()
    {
        var publisher = new PluginEventPublisher(_mockPubLogger.Object);
        var subscriber = new PluginEventSubscriber(publisher, _mockSubLogger.Object);

        subscriber.GetSubscriptionCount().Should().Be(0);

        subscriber.OnPluginLoaded(_ => Task.CompletedTask);
        subscriber.OnPluginUnloaded(_ => Task.CompletedTask);

        subscriber.GetSubscriptionCount().Should().Be(2);
    }

    [Fact]
    public async Task EventSystem_UnsubscribeAll_StopsDelivery()
    {
        var publisher = new PluginEventPublisher(_mockPubLogger.Object);
        var subscriber = new PluginEventSubscriber(publisher, _mockSubLogger.Object);

        var invoked = false;
        subscriber.OnPluginLoaded(_ => { invoked = true; return Task.CompletedTask; });

        subscriber.UnsubscribeAll<PluginLoadedEvent>();

        await publisher.PublishAsync(new PluginLoadedEvent
        {
            PluginId = Guid.NewGuid(),
            PluginName = "P",
            Version = "1.0.0"
        });

        invoked.Should().BeFalse();
    }

    // ── Formatter factory pipeline ──────────────────────────────────────────

    [Fact]
    public async Task FormatterFactory_JsonFormatter_ProducesValidOutput()
    {
        var jsonFormatter = new JsonPluginFormatter();
        var csvFormatter = new CsvPluginFormatter();
        var xmlFormatter = new XmlPluginFormatter();
        var factory = new FormatterFactory(jsonFormatter, csvFormatter, xmlFormatter);

        var plugin = MakePlugin("FormatMe", "1.0.0");

        var formatter = factory.GetFormatter("json");
        formatter.Should().NotBeNull();

        var output = await formatter!.FormatPluginAsync(plugin);
        output.Should().NotBeNullOrWhiteSpace();

        var act = () => JsonDocument.Parse(output);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task FormatterFactory_AllSupportedFormats_ProduceOutput()
    {
        var factory = new FormatterFactory(
            new JsonPluginFormatter(),
            new CsvPluginFormatter(),
            new XmlPluginFormatter());

        var plugin = MakePlugin("MultiFormat", "2.0.0");

        foreach (var format in factory.GetSupportedFormats())
        {
            var formatter = factory.GetFormatter(format);
            formatter.Should().NotBeNull(because: $"format '{format}' should be supported");

            var output = await formatter!.FormatPluginAsync(plugin);
            output.Should().NotBeNullOrWhiteSpace(because: $"format '{format}' should produce output");
        }
    }

    [Fact]
    public void FormatterFactory_UnknownFormat_ReturnsNull()
    {
        var factory = new FormatterFactory(
            new JsonPluginFormatter(),
            new CsvPluginFormatter(),
            new XmlPluginFormatter());

        factory.GetFormatter("yaml").Should().BeNull();
    }

    [Fact]
    public void FormatterFactory_GetSupportedFormats_ContainsExpectedFormats()
    {
        var factory = new FormatterFactory(
            new JsonPluginFormatter(),
            new CsvPluginFormatter(),
            new XmlPluginFormatter());

        var formats = factory.GetSupportedFormats().ToList();

        formats.Should().Contain("json")
            .And.Contain("csv")
            .And.Contain("xml");
    }

    // ── Concurrency: multiple plugins loading simultaneously ─────────────────

    [Fact]
    public async Task Concurrency_SimultaneousValidationsAndResolutions_AllSucceed()
    {
        var allPlugins = Enumerable.Range(0, 10)
            .Select(i => MakePlugin($"Plugin{i}"))
            .ToList();

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPlugins);

        var validator = new PluginValidator(_mockValLogger.Object, new VersionHelper(_mockVerLogger.Object));
        var resolver = new DependencyResolutionService(_mockLoaderService.Object);

        // Each plugin depends on the previous
        for (int i = 1; i < allPlugins.Count; i++)
        {
            allPlugins[i].AddDependency(new PluginDependency
            {
                PluginId = allPlugins[i].Id,
                DependencyPluginId = allPlugins[i - 1].Id,
                MinimumVersion = "1.0.0"
            });
        }

        var validationTasks = allPlugins.Select(p => Task.Run(() => validator.Validate(p))).ToList();
        var resolutionTasks = allPlugins.Select(p => resolver.ValidateDependenciesAsync(p)).ToList();

        var validations = await Task.WhenAll(validationTasks);
        var resolutions = await Task.WhenAll(resolutionTasks);

        validations.Should().AllSatisfy(r => r.IsValid.Should().BeTrue());
        resolutions.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    // ── Configuration combinations ──────────────────────────────────────────

    [Fact]
    public async Task Configuration_PluginWithOptionalAndRequiredDependencies_ValidatesCorrectly()
    {
        var pluginId = Guid.NewGuid();
        var requiredDepId = Guid.NewGuid();
        var optionalDepId = Guid.NewGuid();

        var plugin = MakePlugin("ConfigPlugin");
        plugin.Id = pluginId;
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = requiredDepId,
            MinimumVersion = "1.0.0",
            IsOptional = false
        });
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = optionalDepId,
            MinimumVersion = "1.0.0",
            IsOptional = true   // optional – not present in the registry
        });

        var requiredDep = MakePlugin("RequiredDep");
        requiredDep.Id = requiredDepId;

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { requiredDep });  // optional dep absent

        var resolver = new DependencyResolutionService(_mockLoaderService.Object);

        var isValid = await resolver.ValidateDependenciesAsync(plugin);

        isValid.Should().BeTrue(
            because: "the required dependency is present; missing optional dependency should be ignored");
    }

    [Fact]
    public async Task Configuration_PluginWithVersionConstraints_EnforcesMinimumVersion()
    {
        var pluginId = Guid.NewGuid();
        var depId = Guid.NewGuid();

        var plugin = MakePlugin("ConstrainedPlugin");
        plugin.Id = pluginId;
        plugin.AddDependency(new PluginDependency
        {
            PluginId = pluginId,
            DependencyPluginId = depId,
            MinimumVersion = "2.0.0"
        });

        var oldDep = MakePlugin("OldDep", "1.5.0");
        oldDep.Id = depId;

        _mockLoaderService
            .Setup(x => x.GetAllLoadedPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { oldDep });

        var resolver = new DependencyResolutionService(_mockLoaderService.Object);

        var isValid = await resolver.ValidateDependenciesAsync(plugin);

        isValid.Should().BeFalse(because: "dependency version 1.5.0 does not meet minimum 2.0.0");
    }

    // ── Multi-event publish–subscribe chain ──────────────────────────────────

    [Fact]
    public async Task EventChain_PublishLoadedThenUpdatedThenUnloaded_AllDelivered()
    {
        var publisher = new PluginEventPublisher(_mockPubLogger.Object);
        var received = new List<string>();

        publisher.Subscribe<PluginLoadedEvent>(e => { received.Add("loaded"); return Task.CompletedTask; });
        publisher.Subscribe<PluginUpdatedEvent>(e => { received.Add("updated"); return Task.CompletedTask; });
        publisher.Subscribe<PluginUnloadedEvent>(e => { received.Add("unloaded"); return Task.CompletedTask; });

        var id = Guid.NewGuid();
        await publisher.PublishAsync(new PluginLoadedEvent { PluginId = id, PluginName = "P", Version = "1.0.0" });
        await publisher.PublishAsync(new PluginUpdatedEvent { PluginId = id, PreviousVersion = "1.0.0", NewVersion = "2.0.0", ChangesSummary = "minor" });
        await publisher.PublishAsync(new PluginUnloadedEvent { PluginId = id, PluginName = "P" });

        received.Should().ContainInOrder("loaded", "updated", "unloaded");
    }
}
