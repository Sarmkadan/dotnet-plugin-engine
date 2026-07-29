public void Subscribe(IPluginEventSubscriber subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }
            if (subscribers.ContainsKey(subscriber))
            {
                throw new InvalidOperationException("Subscriber is already registered");
            }
            if (subscribers.Count >= maxSubscriptionsPerPublisher)
            {
                throw new InvalidOperationException("Maximum number of subscriptions exceeded");
            }
            subscribers.Add(subscriber);
        }

        public void Publish(IEnumerable<IPluginEvent> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }
            if (events.Count() > maxBatchSize)
            {
                throw new ArgumentException("Batch size exceeded", nameof(events));
            }
            foreach (var @event in events)
            {
                // ... existing code
            }
        }
