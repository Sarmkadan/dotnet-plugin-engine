#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Events;
using Xunit;

public sealed class PluginEventPublisherConcurrencyTests
{
    private readonly PluginEventPublisher _sut =
        new(new Mock<ILogger<PluginEventPublisher>>().Object);

    [Fact]
    public async Task PublishAsync_WithMultipleFailingAsyncHandlers_ThrowsAllHandlerExceptions()
    {
        var firstException = new InvalidOperationException("First handler failed.");
        var secondException = new ArgumentException("Second handler failed.");

        _sut.Subscribe<PluginLoadedEvent>(async _ =>
        {
            await Task.Yield();
            throw firstException;
        });
        _sut.Subscribe<PluginLoadedEvent>(async _ =>
        {
            await Task.Yield();
            throw secondException;
        });

        var exception = await Assert.ThrowsAsync<AggregateException>(
            () => _sut.PublishAsync(MakeLoadedEvent()));

        exception.InnerExceptions.Should().BeEquivalentTo(
            new Exception[] { firstException, secondException });
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerPublishesSameEventType_SkipsReentrantPublish()
    {
        var invocationCount = 0;

        _sut.Subscribe<PluginLoadedEvent>(async @event =>
        {
            Interlocked.Increment(ref invocationCount);
            await _sut.PublishAsync(@event);
        });

        var act = () => _sut.PublishAsync(MakeLoadedEvent());

        await act.Should().NotThrowAsync();
        invocationCount.Should().Be(1);
    }

    [Fact]
    public async Task SubscribeAndUnsubscribe_WhilePublishingConcurrently_DoesNotThrowOrCorruptState()
    {
        const int operationCount = 200;
        Func<PluginLoadedEvent, Task>[] handlers = Enumerable.Range(0, operationCount)
            .Select(_ => new Func<PluginLoadedEvent, Task>(_ => Task.CompletedTask))
            .ToArray();

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = handlers.Select((handler, index) => Task.Run(async () =>
        {
            await start.Task;
            _sut.Subscribe(handler);
            await _sut.PublishAsync(MakeLoadedEvent());
            _sut.Unsubscribe(handler);

            if (index % 2 == 0)
            {
                _sut.Subscribe(handler);
            }
        })).ToArray();

        start.SetResult();
        var act = () => Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();

        var statistics = _sut.GetStatistics();
        statistics.RegisteredSubscribers.Should().Be(operationCount / 2);
        statistics.MonitoredEventTypes.Should().Be(1);
    }

    [Fact]
    public async Task PublishAsync_WithSynchronousAndAsyncFailures_CollectsBothExceptions()
    {
        var synchronousException = new InvalidOperationException("Synchronous failure.");
        var asynchronousException = new ApplicationException("Asynchronous failure.");

        _sut.Subscribe<PluginLoadedEvent>(_ => throw synchronousException);
        _sut.Subscribe<PluginLoadedEvent>(async _ =>
        {
            await Task.Yield();
            throw asynchronousException;
        });

        var exception = await Assert.ThrowsAsync<AggregateException>(
            () => _sut.PublishAsync(MakeLoadedEvent()));

        exception.InnerExceptions.Should().BeEquivalentTo(
            new Exception[] { synchronousException, asynchronousException });
    }

    private static PluginLoadedEvent MakeLoadedEvent() => new()
    {
        PluginId = Guid.NewGuid(),
        PluginName = "TestPlugin",
        Version = "1.0.0"
    };
}
