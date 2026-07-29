[TestClass]
public class PluginEventBaseTests
{
    [Test]
    public void TestPluginEventBaseImplementsIPluginEvent()
    {
        // Arrange
        var pluginEventBase = new PluginEventBase();

        // Act
        var pluginEvent = (IPluginEvent)pluginEventBase;

        // Assert
        Assert.IsNotNull(pluginEvent);
    }

    [Test]
    public void TestPluginEventBaseTimestampIsSetAtConstructionTimeAndNotMutableAfterward()
    {
        // Arrange
        var pluginEventBase = new PluginEventBase();

        // Act
        var timestamp = pluginEventBase.Timestamp;
        pluginEventBase.Timestamp = DateTime.Now;

        // Assert
        Assert.AreEqual(timestamp, pluginEventBase.Timestamp);
    }

    [Test]
    public void TestPluginUpdatedEventBehaviorWhenOldVersionAndNewVersionFieldsAreEqual()
    {
        // Arrange
        var pluginUpdatedEvent = new PluginUpdatedEvent(null, null);

        // Act
        var exception = pluginUpdatedEvent.Exception;

        // Assert
        Assert.IsNotNull(exception);
    }

    [Test]
    public void TestPluginErrorEventBehaviorWhenConstructedWithNullUnderlyingExceptionErrorMessage()
    {
        // Arrange
        var pluginErrorEvent = new PluginErrorEvent(null);

        // Act
        var exception = pluginErrorEvent.Exception;

        // Assert
        Assert.IsNotNull(exception);
    }

    [Test]
    public void TestIPluginEventHandlerRegisteredForBaseInterfaceTypeDoesOrDoesNotAlsoReceiveDerivedEventTypesWhenPublishedThroughIPluginEventPublisher()
    {
        // Arrange
        var pluginEventPublisher = new PluginEventPublisher();
        var pluginEventHandler = new PluginEventHandler();

        // Act
        pluginEventPublisher.Subscribe(pluginEventHandler);
        pluginEventPublisher.Publish(new PluginEventBase());

        // Assert
        Assert.IsTrue(pluginEventHandler.ReceivedEvent is PluginEventBase);
    }
}
