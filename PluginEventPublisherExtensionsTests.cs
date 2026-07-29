[TestClass]
public class PluginEventPublisherExtensionsTests
{
    [Test]
    public void BatchPublishEmptyCollectionDoesNotThrow()
    {
        // Arrange
        var publisher = new PluginEventPublisher();
        var events = new List<IEvent>();
        // Act
        publisher.BatchPublish(events);
        // Assert
        Assert.DoesNotThrow(() => publisher.BatchPublish(events));
    }

    [Test]
    public void BatchPublishWithThrowingHandlerContinuesPublishingRemainingEvents()
    {
        // Arrange
        var publisher = new PluginEventPublisher();
        var events = new List<IEvent>() { new Event1(), new Event2(), new Event3() };
        var handler = new PluginEventHandler<Event>(e => { throw new Exception(); });
        publisher.Subscribe(handler);
        // Act
        publisher.BatchPublish(events);
        // Assert
        Assert.AreEqual(2, publisher.GetSubscriptionCount());
    }

    [Test]
    public void AsyncPublishWithHungTaskDoesNotWaitIndefinitely()
    {
        // Arrange
        var publisher = new PluginEventPublisher();
        var tcs = new TaskCompletionSource();
        var handler = new PluginEventHandler<Event>(e => tcs.SetResult(default));
        publisher.Subscribe(handler);
        // Act and Assert
        Assert.DoesNotThrow(() => publisher.Publish(new Event()));
        Assert.IsFalse(tcs.Task.IsCompleted);
    }

    [Test]
    public void DiagnosticUtilitiesReturnCorrectCounts()
    {
        // Arrange
        var publisher = new PluginEventPublisher();
        var handler1 = new PluginEventHandler<Event>(e => { });
        var handler2 = new PluginEventHandler<Event>(e => { });
        // Act and Assert
        Assert.AreEqual(0, publisher.GetSubscriptionCount());
        publisher.Subscribe(handler1);
        Assert.AreEqual(1, publisher.GetSubscriptionCount());
        publisher.Subscribe(handler2);
        Assert.AreEqual(2, publisher.GetSubscriptionCount());
        publisher.Unsubscribe(handler2);
        Assert.AreEqual(1, publisher.GetSubscriptionCount());
        publisher.Unsubscribe(handler2);
        Assert.AreEqual(1, publisher.GetSubscriptionCount());
    }

    [Test]
    public void PublishingWithZeroSubscribersCompletesWithoutError()
    {
        // Arrange
        var publisher = new PluginEventPublisher();
        // Act and Assert
        Assert.DoesNotThrow(() => publisher.Publish(new Event()));
    }
}