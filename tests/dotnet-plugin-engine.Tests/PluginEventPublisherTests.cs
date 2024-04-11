#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Events;
using Xunit;

namespace PluginEngine.Tests;

public sealed class PluginEventPublisherTests
{
    private readonly Mock<ILogger<PluginEventPublisher>> _mockLogger;
    private readonly PluginEventPublisher _sut;

    public PluginEventPublisherTests()
    {
        _mockLogger = new Mock<ILogger<PluginEventPublisher>>();
        _sut = new PluginEventPublisher(_mockLogger.Object);
    }

    private static PluginLoadedEvent MakeLoadedEvent(Guid? pluginId = null) => new()
    {
        PluginId = pluginId ?? Guid.NewGuid(),
        PluginName = "TestPlugin",
        Version = "1.0.0"
    };

    // ── Subscribe / Publish ─────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesWithoutError()
    {
        var @event = MakeLoadedEvent();

        var act = () => _sut.PublishAsync(@event);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_WithOneSubscriber_InvokesHandler()
    {
        var invoked = false;
        _sut.Subscribe<PluginLoadedEvent>(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await _sut.PublishAsync(MakeLoadedEvent());

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_InvokesAllHandlers()
    {
        var count = 0;
        _sut.Subscribe<PluginLoadedEvent>(_ => { count++; return Task.CompletedTask; });
        _sut.Subscribe<PluginLoadedEvent>(_ => { count++; return Task.CompletedTask; });
        _sut.Subscribe<PluginLoadedEvent>(_ => { count++; return Task.CompletedTask; });

        await _sut.PublishAsync(MakeLoadedEvent());

        count.Should().Be(3);
    }

    [Fact]
    public async Task PublishAsync_HandlerReceivesCorrectEvent()
    {
        var pluginId = Guid.NewGuid();
        PluginLoadedEvent? received = null;

        _sut.Subscribe<PluginLoadedEvent>(e =>
        {
            received = e;
            return Task.CompletedTask;
        });

        await _sut.PublishAsync(MakeLoadedEvent(pluginId));

        received.Should().NotBeNull();
        received!.PluginId.Should().Be(pluginId);
    }

    // ── Unsubscribe ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Unsubscribe_AfterSubscribing_HandlerIsNotInvokedOnNextPublish()
    {
        var invoked = false;
        Func<PluginLoadedEvent, Task> handler = _ =>
        {
            invoked = true;
            return Task.CompletedTask;
        };

        _sut.Subscribe(handler);
        _sut.Unsubscribe(handler);

        await _sut.PublishAsync(MakeLoadedEvent());

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Unsubscribe_ForHandlerThatWasNeverRegistered_DoesNotThrow()
    {
        Func<PluginLoadedEvent, Task> handler = _ => Task.CompletedTask;

        var act = () => _sut.Unsubscribe(handler);

        act.Should().NotThrow();
    }

    // ── Different event types ───────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_WithDifferentEventTypes_DeliverOnlyToMatchingSubscribers()
    {
        var loadedCount = 0;
        var unloadedCount = 0;

        _sut.Subscribe<PluginLoadedEvent>(_ => { loadedCount++; return Task.CompletedTask; });
        _sut.Subscribe<PluginUnloadedEvent>(_ => { unloadedCount++; return Task.CompletedTask; });

        await _sut.PublishAsync(MakeLoadedEvent());

        loadedCount.Should().Be(1);
        unloadedCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishAsync_ErrorEvent_InvokesCorrectSubscriber()
    {
        var invoked = false;
        _sut.Subscribe<PluginErrorEvent>(_ => { invoked = true; return Task.CompletedTask; });

        await _sut.PublishAsync(new PluginErrorEvent
        {
            PluginId = Guid.NewGuid(),
            ErrorMessage = "Something failed"
        });

        invoked.Should().BeTrue();
    }

    // ── Statistics ──────────────────────────────────────────────────────────

    [Fact]
    public void GetStatistics_Initially_ReturnsZeroCounts()
    {
        var stats = _sut.GetStatistics();

        stats.EventsPublished.Should().Be(0);
        stats.RegisteredSubscribers.Should().Be(0);
        stats.MonitoredEventTypes.Should().Be(0);
    }

    [Fact]
    public void GetStatistics_AfterSubscribing_ReflectsSubscriberCount()
    {
        _sut.Subscribe<PluginLoadedEvent>(_ => Task.CompletedTask);
        _sut.Subscribe<PluginLoadedEvent>(_ => Task.CompletedTask);

        var stats = _sut.GetStatistics();

        stats.RegisteredSubscribers.Should().Be(2);
        stats.MonitoredEventTypes.Should().Be(1);
    }

    [Fact]
    public async Task GetStatistics_AfterPublishing_IncrementsEventCount()
    {
        await _sut.PublishAsync(MakeLoadedEvent());
        await _sut.PublishAsync(MakeLoadedEvent());

        var stats = _sut.GetStatistics();

        stats.EventsPublished.Should().Be(2);
    }

    [Fact]
    public void GetStatistics_AfterUnsubscribing_DecreasesCount()
    {
        Func<PluginLoadedEvent, Task> handler = _ => Task.CompletedTask;
        _sut.Subscribe(handler);
        _sut.Unsubscribe(handler);

        var stats = _sut.GetStatistics();

        stats.RegisteredSubscribers.Should().Be(0);
    }

    // ── Concurrent publish ──────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_ConcurrentPublishes_AllHandlersReceiveEvents()
    {
        var receivedCount = 0;
        _sut.Subscribe<PluginLoadedEvent>(_ =>
        {
            Interlocked.Increment(ref receivedCount);
            return Task.CompletedTask;
        });

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _sut.PublishAsync(MakeLoadedEvent()))
            .ToList();

        await Task.WhenAll(tasks);

        receivedCount.Should().Be(20);
    }
}
