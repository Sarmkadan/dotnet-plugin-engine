using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine;
using System;
using System.Collections.Generic;

namespace dotnet_plugin_engine.Benchmarks
{
    /// <summary>
    /// Benchmarks for measuring the performance of PluginEventPublisher operations.
    /// </summary>
    [MemoryDiagnoser]
    public class PluginEventPublisherBenchmarks
    {
        private PluginEventPublisher _publisher;

        /// <summary>
        /// Sets up the benchmark by creating a PluginEventPublisher instance and pre-populating it with subscribers.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _publisher = new PluginEventPublisher();
            // Pre-populate with 100 subscribers to simulate a realistic scenario for publishing benchmarks
            for (int i = 0; i < 100; i++)
            {
                _publisher.Subscribe(new TestSubscriber());
            }
        }

        /// <summary>
        /// Measures the time to subscribe a specified number of subscribers to a new publisher instance.
        /// </summary>
        [Benchmark]
        [Params(1, 10, 100)]
        public void Subscribe_Benchmark(int count)
        {
            var publisher = new PluginEventPublisher();
            for (int i = 0; i < count; i++)
            {
                publisher.Subscribe(new TestSubscriber());
            }
        }

        /// <summary>
        /// Measures the time to publish a specified number of events to a publisher with existing subscribers.
        /// </summary>
        [Benchmark]
        [Params(1, 10, 100)]
        public void Publish_Benchmark(int count)
        {
            var events = new List<IPluginEvent>();
            for (int i = 0; i < count; i++)
            {
                events.Add(new PluginEventBase());
            }
            _publisher.Publish(events);
        }

        /// <summary>
        /// Cleans up resources after each benchmark iteration.
        /// </summary>
        [GlobalCleanup]
        public void Cleanup()
        {
            _publisher = null;
        }

        private class TestSubscriber : IPluginEventSubscriber
        {
            public void Handle(IPluginEvent @event)
            {
                // Intentionally left empty
            }
        }
    }
}