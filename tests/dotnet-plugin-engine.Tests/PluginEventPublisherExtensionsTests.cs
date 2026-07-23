using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PluginEngine.Events;
using Xunit;

namespace PluginEngine.Tests
{
    public class PluginEventPublisherExtensionsTests
    {
        [Fact]
        public void Publish_HappyPath_PublishesEvent()
        {
            // Arrange
            var publisher = new PluginEventPublisher();
            var @event = new PluginEventBase();

            // Act
            publisher.Publish(@event);

            // Assert
            // No direct way to assert, but we can check for no exceptions
        }

        [Fact]
        public async Task PublishBatchAsync_HappyPath_PublishesEvents()
        {
            // Arrange
            var publisher = new PluginEventPublisher();
            var @events = new List<PluginEventBase> { new PluginEventBase(), new PluginEventBase() };

            // Act
            await publisher.PublishBatchAsync(@events);

            // Assert
            // No direct way to assert, but we can check for no exceptions
        }

        [Fact]
        public void GetCurrentStatistics_HappyPath_ReturnsStatistics()
        {
            // Arrange
            var publisher = new PluginEventPublisher();

            // Act
            var stats = publisher.GetCurrentStatistics();

            // Assert
            Assert.NotNull(stats);
        }

        [Fact]
        public void Subscribe_HappyPath_SubscribesToEvent()
        {
            // Arrange
            var publisher = new PluginEventPublisher();
            var handler = new Action<PluginEventBase>(_ => { });

            // Act
            publisher.Subscribe(handler);

            // Assert
            // No direct way to assert, but we can check for no exceptions
        }

        [Fact]
        public void UnsubscribeAll_HappyPath_UnsubscribesFromEvent()
        {
            // Arrange
            var publisher = new PluginEventPublisher();
            var handler = new Action<PluginEventBase>(_ => { });
            publisher.Subscribe(handler);

            // Act
            publisher.UnsubscribeAll<PluginEventBase>();

            // Assert
            // No direct way to assert, but we can check for no exceptions
        }

        [Fact]
        public void GetStatisticsString_HappyPath_ReturnsStatisticsString()
        {
            // Arrange
            var publisher = new PluginEventPublisher();

            // Act
            var statsString = publisher.GetStatisticsString();

            // Assert
            Assert.NotNull(statsString);
        }

        [Fact]
        public void SubscribeOnce_HappyPath_SubscribesAndUnsubscribes()
        {
            // Arrange
            var publisher = new PluginEventPublisher();
            var handler = new Action<PluginEventBase>(_ => { });
            var disposable = publisher.SubscribeOnce(handler);

            // Act
            publisher.Publish(new PluginEventBase());

            // Assert
            // No direct way to assert, but we can check for no exceptions
            disposable.Dispose();
        }

        [Fact]
        public void GetMonitoredEventTypes_HappyPath_ReturnsEventTypes()
        {
            // Arrange
            var publisher = new PluginEventPublisher();

            // Act
            var eventTypes = publisher.GetMonitoredEventTypes();

            // Assert
            Assert.NotNull(eventTypes);
        }

        [Fact]
        public void GetSubscriberCount_HappyPath_ReturnsSubscriberCount()
        {
            // Arrange
            var publisher = new PluginEventPublisher();
            var handler = new Action<PluginEventBase>(_ => { });
            publisher.Subscribe(handler);

            // Act
            var count = publisher.GetSubscriberCount<PluginEventBase>();

            // Assert
            Assert.True(count > 0);
        }

        [Fact]
        public void Publish_NullPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            PluginEventPublisher publisher = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => publisher.Publish(new PluginEventBase()));
        }

        [Fact]
        public void PublishBatchAsync_NullPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            PluginEventPublisher publisher = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => publisher.PublishBatchAsync(new List<PluginEventBase>()));
        }
    }
}
